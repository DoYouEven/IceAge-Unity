using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// FBBIK driving setup demo.
	/// </summary>
	public class FBIKDrivingRig : MonoBehaviour {
		
		public FullBodyBipedIK ik; // Reference to the FBBIK component
		public Transform leftHandPoseTarget, rightHandPoseTarget;

		private HandPoser[] handPosers;

		void Start() {
			// Create hand posers programmatically to avoid FBX updating problems
			if (leftHandPoseTarget != null) {
				handPosers = new HandPoser[2] {
					ik.solver.leftHandEffector.bone.gameObject.AddComponent<HandPoser>(),
					ik.solver.rightHandEffector.bone.gameObject.AddComponent<HandPoser>()
				};

				handPosers[0].poseRoot = leftHandPoseTarget;
				handPosers[1].poseRoot = rightHandPoseTarget;
			}
		}

		void LateUpdate() {
			// Update hand poser weights
			foreach (HandPoser handPoser in handPosers) {
				handPoser.localRotationWeight = ik.solver.IKPositionWeight; // IKPositionWeight is the master weight for FBBIK
			}
		}
	}
}
