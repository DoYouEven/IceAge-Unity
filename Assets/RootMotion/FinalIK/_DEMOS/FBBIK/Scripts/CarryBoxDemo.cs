using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Demonstrates how FBBIK can be used to carry stuff around.
	/// </summary>
	public class CarryBoxDemo : MonoBehaviour {

		public FullBodyBipedIK ik; // Reference to the FullBodyBipedIK component
		
		public Transform leftHandTarget, rightHandTarget; // The hand IK targets (posed and copied from runtime)
		
		void LateUpdate() {
			// Setting IK position and rotation for the hands
			ik.solver.leftHandEffector.position = leftHandTarget.position;
			ik.solver.leftHandEffector.rotation = leftHandTarget.rotation;
			
			ik.solver.rightHandEffector.position = rightHandTarget.position;
			ik.solver.rightHandEffector.rotation = rightHandTarget.rotation;
		}
	}
}
