using UnityEditor;
using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector for Biped IK.
	 * */
	[CustomEditor(typeof(BipedIK))]
	public class BipedIKInspector : Editor {
		
		private BipedIK script { get { return target as BipedIK; }}
		
		private int selectedSolver = -1;
		
		private SerializedProperty references, solvers;
		private SerializedProperty[] solversProps;
		private SerializedContent fixTransforms;

		public void OnEnable() {
			if (serializedObject == null) return;

			// Store the MonoScript for changing script execution order
			if (!Application.isPlaying) {
				MonoScript monoScript = MonoScript.FromMonoBehaviour(script);

				// Changing the script execution order to make sure BipedIK always executes after any other script except FullBodyBipedIK
				int executionOrder = MonoImporter.GetExecutionOrder(monoScript);
				if (executionOrder != 9998) MonoImporter.SetExecutionOrder(monoScript, 9998);
			}

			references = serializedObject.FindProperty("references");
			solvers = serializedObject.FindProperty("solvers");
			solversProps = BipedIKSolversInspector.FindProperties(solvers);
			fixTransforms = new SerializedContent(serializedObject.FindProperty("fixTransforms"), new GUIContent("Fix Transforms", "If true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance."));
			
			// Automatically detecting references
			if (!Application.isPlaying) {
				if (script.references.isEmpty) {
					BipedReferences.AutoDetectReferences(ref script.references, script.transform, new BipedReferences.AutoDetectParams(false, true));
					
					references.isExpanded = true;
					solvers.isExpanded = false;
					for (int i = 0; i < solversProps.Length; i++) solversProps[i].isExpanded = false;
					
					// Setting default values and initiating
					script.InitiateBipedIK();
					script.SetToDefaults();
					EditorUtility.SetDirty(script);
				} else script.InitiateBipedIK();
				
				Warning.logged = false;

				if (Application.isPlaying) {
					if (ReferencesValid(true)) BipedReferences.CheckSetupWarning(script.references, true);
				}
			}
		}

		private bool ReferencesValid(bool log) {
			return BipedReferences.CheckSetupError(script.references, log);
		}

		private void WarningBox() {
			// Warning box
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal("Box");
			
			EditorGUILayout.LabelField("Invalid/incomplete setup, can't initiate solver.");
			
			if (GUILayout.Button("What's wrong?")) {
				Warning.logged = false;

				if (ReferencesValid(true)) BipedReferences.CheckSetupWarning(script.references, true);
			}
			
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}
		
		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.Space();

			Inspector.AddContent(fixTransforms);
			
			// Editing References
			if (BipedReferencesInspector.AddModifiedInspector(references)) {
				if (!Application.isPlaying) {
					Warning.logged = false;

					if (ReferencesValid(false)) {
						BipedReferences.CheckSetupWarning(script.references, true);

						// Initiate only when inspector changed
						script.InitiateBipedIK();
					}
				}
			}

			if (ReferencesValid(Application.isPlaying)) {
				// Editing Solvers
				BipedIKSolversInspector.AddInspector(solvers, solversProps);
			} else {
				// Warning box
				WarningBox();
			}

			EditorGUILayout.Space();
			
			serializedObject.ApplyModifiedProperties();
		}
		
		void OnSceneGUI() {
			if (!script.enabled) return;

			// Draw the scene view helpers for the solvers
			BipedIKSolversInspector.AddScene(script.solvers, ref selectedSolver);
		}
	}
}
	
