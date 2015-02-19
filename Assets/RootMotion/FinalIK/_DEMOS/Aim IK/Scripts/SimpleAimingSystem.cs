using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// The simplest aiming system that crossfades between static aim poses based on direction using the Legacy animation system (because CrossFade in Mecanim still has some issues).
	/// </summary>
	public class SimpleAimingSystem : MonoBehaviour {

		public AimPoser aimPoser; // AimPoser is a tool that returns an animation name based on direction
		public AimIK aim; // Reference to the AimIK component
		public LookAtIK lookAt; // Reference to the LookAt component (only used for the head in this instance)
		public Transform recursiveMixingTransform; // The recursive mixing Transform for the aim poses (only this bone and bones deeper in the hierarchy, will be affected by the aim poses)

		[HideInInspector] public Vector3 targetPosition;

		private AimPoser.Pose aimPose, lastPose;

		void Start() {
			// Set mixing Transforms for all the aim poses
			foreach (AimPoser.Pose pose in aimPoser.poses) {
				animation[pose.name].AddMixingTransform(recursiveMixingTransform, true);
			}

			// Disable IK components to manage their updating order
			aim.Disable();
			lookAt.Disable();
		}

		// LateUpdate is called once per frame
		void LateUpdate () {
			// Switch aim poses (Legacy animation)
			Pose();

			// Set IK target positions
			aim.solver.SetIKPosition(targetPosition);
			lookAt.solver.SetIKPosition(targetPosition);

			// Update IK solvers
			aim.solver.Update();
			lookAt.solver.Update();
		}

		private void Pose() {
			// Get the aiming direction
			Vector3 direction = (targetPosition - aim.solver.bones[0].transform.position);
			// Getting the direction relative to the root transform
			Vector3 localDirection = transform.InverseTransformDirection(direction);

			// Get the Pose from AimPoser
			aimPose = aimPoser.GetPose(localDirection);

			// If the Pose has changed
			if (aimPose != lastPose) {
				// CrossFade to the new pose
				animation.CrossFade(aimPose.name);

				// Increase the angle buffer of the pose so we won't switch back too soon if the direction changes a bit
				aimPoser.SetPoseActive(aimPose);

				// Store the pose so we know if it changes
				lastPose = aimPose;
			}
		}
	}
}
