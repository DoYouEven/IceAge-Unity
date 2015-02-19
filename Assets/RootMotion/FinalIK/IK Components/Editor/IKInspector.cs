using UnityEditor;
using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Base abstract class for IK component inspectors.
	 * */
	public abstract class IKInspector : Editor {
		
		protected abstract void AddInspector();
		protected abstract MonoBehaviour GetMonoBehaviour(out int executionOrder);

		protected SerializedProperty solver;
		protected SerializedContent timeStep;
		protected SerializedContent fixTransforms;
		protected SerializedContent[] content;
		protected virtual void OnApplyModifiedProperties() {}
		protected virtual void OnEnableVirtual() {}

		private MonoScript monoScript;

		void OnEnable() {
			if (serializedObject == null) return;

			// Changing the script execution order
			if (!Application.isPlaying) {
				int executionOrder = 0;
				monoScript = MonoScript.FromMonoBehaviour(GetMonoBehaviour(out executionOrder));
				int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
				if (currentExecutionOrder != executionOrder) MonoImporter.SetExecutionOrder(monoScript, executionOrder);
			}

			solver = serializedObject.FindProperty("solver");
			timeStep = new SerializedContent(serializedObject.FindProperty("timeStep"), new GUIContent("Time Step", "If zero, will update the solver in every LateUpdate(). Use this for chains that are animated. If > 0, will be used as updating frequency so that the solver will reach its target in the same time on all machines."));
			fixTransforms = new SerializedContent(serializedObject.FindProperty("fixTransforms"), new GUIContent("Fix Transforms", "If true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance. Not recommended for CCD and FABRIK solvers"));

			OnEnableVirtual();
		}

		protected virtual void AddWarningBox(IKSolver solver) {
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal("Box");
			
			EditorGUILayout.LabelField("Invalid/incomplete setup, can't initiate solver.");
			
			if (GUILayout.Button("What's wrong?")) {
				Warning.logged = false;
				solver.IsValid(true);
			}
			
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}

		#region Inspector
		
		public override void OnInspectorGUI() {
			if (serializedObject == null) return;

			serializedObject.Update();
			
			Inspector.AddClampedFloat(timeStep, 0f, Mathf.Infinity);
			Inspector.AddContent(fixTransforms);
			
			AddInspector();

			if (serializedObject.ApplyModifiedProperties()) {
				OnApplyModifiedProperties();
			}
		}

		#endregion Inspector
	}
}
