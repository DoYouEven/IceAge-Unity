using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	// Extends the default Animator controller for 3rd person view to add IK
	[RequireComponent(typeof(AimIK))]
	[RequireComponent(typeof(FullBodyBipedIK))]
	public class AnimatorController3rdPersonIK: AnimatorController3rdPerson {

		// For debugging only
		void OnGUI() {
			GUILayout.Label("Press F to switch Final IK on/off");
			GUILayout.Label("Press C to toggle between 3rd person/1st person camera");
		}

		[SerializeField] bool useIK = true;
		[SerializeField] Transform rightHandTarget; // Target for the right hand effector
		[SerializeField] Transform leftHandTarget; // Target for the left hand effector
		[SerializeField] Transform head;
		[SerializeField] Vector3 headLookAxis = Vector3.forward;
		[SerializeField] float headLookWeight = 1f;
		[SerializeField] Camera firstPersonCam; // The FPS camera

		// Just quick shortcuts to the hand effectors for better readability
		private IKEffector leftHand { get { return ik.solver.leftHandEffector; }}
		private IKEffector rightHand { get { return ik.solver.rightHandEffector; }}

		// The IK components
		private AimIK aim;
		private FullBodyBipedIK ik;
		
		private Quaternion rightHandRotation;
		private Quaternion fpsCamDefaultRot;

		protected override void Start() {
			base.Start();

			// Find the IK components
			aim = GetComponent<AimIK>();
			ik = GetComponent<FullBodyBipedIK>();

			// Disable the IK components to manage their updating
			aim.Disable();
			ik.Disable();

			fpsCamDefaultRot = firstPersonCam.transform.localRotation;
		}

		// Make sure this updates AFTER the camera is moved/rotated
		public override void Move(Vector3 moveInput, bool isMoving, Vector3 faceDirection, Vector3 aimTarget) {
			base.Move(moveInput, isMoving, faceDirection, aimTarget);

			// Toggle FPS/3PS
			if (Input.GetKeyDown(KeyCode.C)) firstPersonCam.enabled = !firstPersonCam.enabled;

			// Toggle IK
			if (Input.GetKeyDown(KeyCode.F)) useIK = !useIK;
			if (!useIK) return;

			// Set AimIK target position
			aim.solver.IKPosition = aimTarget;

			// FBBIK Pass 1 - Setting the right hand to the gun that is parented to the spine (If we had gun holding animation, we could avoid that pass)
			FBBIKPass1();

			aim.solver.Update(); // Update AimIK

			// FBBIK Pass 2 - Positioning the left hand on the gun after aiming has finished
			FBBIKPass2();

			HeadLookAt(aimTarget); // Rotate the head to look at the aim target

			if (firstPersonCam.enabled) StabilizeFPSCamera(); // Remove FPS camera banking
		}

		// Settig the right hand to the gun that is parented to the spine
		private void FBBIKPass1() {
			rightHandRotation = rightHandTarget.rotation; // Store the right hand target rotation because the gun will be rotated off by FBBIK
			
			rightHand.position = rightHandTarget.position; // Position the right hand to the gun
			rightHand.positionWeight = 1f;
			
			leftHand.positionWeight = 0f; // Ignore this pass for the left hand
			
			ik.solver.Update(); // FBBIK Pass 1
			
			rightHand.bone.rotation = rightHandRotation; // Rotate the right hand to the original right hand target rotation
		}

		// Positioning the left hand on the gun after aiming has finished
		private void FBBIKPass2() {
			rightHand.position = rightHand.bone.position; // Fix the right hand to it's current position
			rightHandRotation = rightHand.bone.rotation; // Store the right hand rotation
			
			leftHand.position = leftHandTarget.position; // Place the left hand on the gun
			leftHand.positionWeight = 1f;
			
			ik.solver.Update(); // FBBIK Pass 2

			// Rotating the hand bones after IK has finished
			rightHand.bone.rotation = rightHandRotation;
			leftHand.bone.rotation = leftHandTarget.rotation;
		}

		// Rotating the head to look at the target
		private void HeadLookAt(Vector3 lookAtTarget) {
			if (head == null) return;

			Quaternion headRotationTarget = Quaternion.FromToRotation(head.rotation * headLookAxis, lookAtTarget - head.position);
			head.rotation = Quaternion.Lerp(Quaternion.identity, headRotationTarget, headLookWeight) * head.rotation;
		}

		// Removes camera banking
		private void StabilizeFPSCamera() {
			// Rotate the FPS camera to its default rotation
			firstPersonCam.transform.localRotation = fpsCamDefaultRot;

			// Get the camera up ortho-normalized to camera forward
			Vector3 normal = firstPersonCam.transform.forward;
			Vector3 cameraUp = firstPersonCam.transform.up;
			Vector3.OrthoNormalize(ref normal, ref cameraUp);

			// Get the world up ortho-normalized to camera forward
			normal = firstPersonCam.transform.forward;
			Vector3 worldUp = Vector3.up;
			Vector3.OrthoNormalize(ref normal, ref worldUp);

			// The rotation that rotates camera up to world up.
			Quaternion fromTo = Quaternion.FromToRotation(cameraUp, worldUp);

			// Fade out this effect when looking directly up/down to avoid singularity problems
			float dot = Vector3.Dot(transform.forward, firstPersonCam.transform.forward);

			// Twist the camera so that it's up vector will always be pointed towards world up
			firstPersonCam.transform.rotation = Quaternion.Lerp(Quaternion.identity, fromTo, dot) * firstPersonCam.transform.rotation;
		}
	}
}
