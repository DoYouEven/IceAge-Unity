using UnityEditor;
using UnityEngine;
using System.Collections;

	namespace RootMotion.FinalIK {

	/*
	 * Custom inspector for FullBodyBipedIK.
	 * */
	[CustomEditor(typeof(FullBodyBipedIK))]
	public class FullBodyBipedIKInspector : IKInspector {

		private FullBodyBipedIK script { get { return target as FullBodyBipedIK; }}
		private int selectedEffector;
		private SerializedProperty references;
		private bool autodetected;

		private static Color color {
			get {
				return new Color(0f, 0.75f, 1f);
			}
		}

		protected override MonoBehaviour GetMonoBehaviour(out int executionOrder) {
			executionOrder = 9999;
			return script;
		}

		protected override void OnEnableVirtual() {
			references = serializedObject.FindProperty("references");

			// Autodetecting References
			if (script.references.IsEmpty(false) && script.enabled) {
				BipedReferences.AutoDetectReferences(ref script.references, script.transform, new BipedReferences.AutoDetectParams(true, false));

				script.solver.rootNode = IKSolverFullBodyBiped.DetectRootNodeBone(script.references);

				Initiate();

				if (Application.isPlaying) Warning.Log("Biped references were auto-detected on a FullBodyBipedIK component that was added in runtime. Note that this only happens in the Editor and if the GameObject is selected (for quick and convenient debugging). If you want to add FullBodyBipedIK dynamically in runtime via script, you will have to use BipedReferences.AutodetectReferences() for automatic biped detection.", script.transform);
				
				references.isExpanded = !script.references.isValid;
			}
		}

		protected override void AddInspector() {
			// While in editor
			if (!Application.isPlaying) {
				// Editing References, if they have changed, reinitiate.
				if (BipedReferencesInspector.AddModifiedInspector(references)) {
					Initiate();
					return; // Don't draw immediatelly to avoid errors
				}
				// Root Node
				IKSolverFullBodyBipedInspector.AddReferences(true, solver);

				// Reinitiate if rootNode has changed
				if (serializedObject.ApplyModifiedProperties()) {
					Initiate();
					return; // Don't draw immediatelly to avoid errors
				}
			} else {
				// While in play mode

				// Draw the references and the root node for UMA
				BipedReferencesInspector.AddModifiedInspector(references);	
				IKSolverFullBodyBipedInspector.AddReferences(true, solver);
			}

			if (IsValidAndInitiated(false)) {
				// Draw the inspector for IKSolverFullBody
				IKSolverFullBodyBipedInspector.AddInspector(solver, !Application.isPlaying, false);
			} else {
				AddWarningBox(script.solver);
			}

			EditorGUILayout.Space();
		}

		// Override the default warning box
		protected override void AddWarningBox(IKSolver solver) {
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal("Box");
			
			EditorGUILayout.LabelField("Invalid/incomplete setup, can't initiate solver.");
			
			if (GUILayout.Button("What's wrong?")) {
				Warning.logged = false;
				IsValidAndInitiated(true);
			}
			
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}

		// Is everything OK and initiated?
		private bool IsValidAndInitiated(bool log) {
			// Are the references OK?
			if (!CheckError(script.references, script.solver, script.transform, log)) return false;
			
			// Is the solver valid?
			if (!script.solver.IsValid(log)) return false;
			return true;
		}

		private void Initiate() {
			Warning.logged = false;

			// Check for possible errors, if found, do not initiate
			if (!CheckError(script.references, script.solver, script.transform, Application.isPlaying)) return;

			// Notify of possible problems, but still initiate
			CheckWarning(script.references, script.solver, script.transform, Application.isPlaying);

			// Initiate
			script.solver.SetToReferences(script.references, script.solver.rootNode);
		}

		// Checks the biped references for errors
		private bool CheckError(BipedReferences references, IKSolverFullBodyBiped solver, Transform context, bool log) {
			// All the errors common to all bipeds
			if (!BipedReferences.CheckSetupError(script.references, log)) {
				if (log) Warning.Log("Invalid references, can't initiate the solver.", context, true);
				return false;
			}

			// All the errors specific to FBBIK
			if (references.spine.Length == 0) {
				if (log) Warning.Log("Biped has no spine bones, can't initiate the solver.", context, true);
				return false;
			}

			if (solver.rootNode == null) {
				if (log) Warning.Log("Root Node bone is null, can't initiate the solver.", context, true);
				return false;
			}

			if (solver.rootNode != references.pelvis) {
				bool inSpine = false;

				for (int i = 0; i < references.spine.Length; i++) {
					if (solver.rootNode == references.spine[i]) {
						inSpine = true;
						break;
					}
				}

				if (!inSpine) {
					if (log) Warning.Log("The Root Node has to be one of the bones in the Spine or the Pelvis, can't initiate the solver.", context, true);
				}
			}

			return true;
		}

		// Check for possible warnings with the biped references setup
		private void CheckWarning(BipedReferences references, IKSolverFullBodyBiped solver, Transform context, bool log) {
			// Check for all the warnings common to all bipeds
			BipedReferences.CheckSetupWarning(script.references, log);

			// Check for warnings specific to FBBIK
			Vector3 toRightShoulder = references.rightUpperArm.position - references.leftUpperArm.position;
			Vector3 shoulderToRootNode = solver.rootNode.position - references.leftUpperArm.position;
			float dot = Vector3.Dot(toRightShoulder.normalized, shoulderToRootNode.normalized);
			
			if (dot > 0.95f) {
				if (log) Warning.Log ("The root node, the left upper arm and the right upper arm bones should ideally form a triangle that is as close to equilateral as possible. " +
					"Currently the root node bone seems to be very close to the line between the left upper arm and the right upper arm bones. This might cause unwanted behaviour like the spine turning upside down when pulled by a hand effector." +
					"Please set the root node bone to be one of the lower bones in the spine.", context, true);
			}
			
			Vector3 toRightThigh = references.rightThigh.position - references.leftThigh.position;
			Vector3 thighToRootNode = solver.rootNode.position - references.leftThigh.position;
			dot = Vector3.Dot(toRightThigh.normalized, thighToRootNode.normalized);
			
			if (dot > 0.95f && log) {
				Warning.Log ("The root node, the left thigh and the right thigh bones should ideally form a triangle that is as close to equilateral as possible. " +
					"Currently the root node bone seems to be very close to the line between the left thigh and the right thigh bones. This might cause unwanted behaviour like the hip turning upside down when pulled by an effector." +
					"Please set the root node bone to be one of the higher bones in the spine.", context, true);
			}
		}

		// Draw the scene view handles
		void OnSceneGUI() {
			// Draw the scene veiw helpers
			if (!script.references.isValid) return;

			IKSolverFullBodyBipedInspector.AddScene(script.solver, color, true, ref selectedEffector, script.transform);
		}
	}
}
