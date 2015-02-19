using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Manages the execution order of IK components.
	/// </summary>
	public class IKExecutionOrder : MonoBehaviour {

		/// <summary>
		/// The IK components.
		/// </summary>
		public IK[] IKComponents;

		// Disable the IK components
		void Start() {
			for (int i = 0; i < IKComponents.Length; i++) IKComponents[i].Disable();
		}

		// Update the IK components in a specific order
		void LateUpdate() {
			for (int i = 0; i < IKComponents.Length; i++) IKComponents[i].GetIKSolver().Update();
		}
	}
}
