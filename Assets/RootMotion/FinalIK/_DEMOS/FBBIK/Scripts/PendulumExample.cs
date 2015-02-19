using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Making a character hold on to a target and swing about it while maintaining his animation.
	/// </summary>
	[RequireComponent(typeof(FullBodyBipedIK))]
	public class PendulumExample : MonoBehaviour {

		[SerializeField] Transform target;
		[SerializeField] Transform leftHandTarget;
		[SerializeField] Transform rightHandTarget;
		[SerializeField] Transform leftFootTarget;
		[SerializeField] Transform rightFootTarget;
		[SerializeField] Transform pelvisTarget;
		[SerializeField] Transform bodyTarget;
		[SerializeField] Transform headTarget;
		[SerializeField] Vector3 pelvisDownAxis = Vector3.right;
		
		public float hangingDistanceMlp = 1.3f;

		private FullBodyBipedIK ik;
		private Quaternion rootRelativeToPelvis;
		private Vector3 pelvisToRoot;
		
		void Start() {
			ik = GetComponent<FullBodyBipedIK>();

			// Connect the left hand to the target
			Quaternion targetRotation = target.rotation;
			target.rotation = leftHandTarget.rotation;
			
			FixedJoint j = target.gameObject.AddComponent<FixedJoint>();
			j.connectedBody = leftHandTarget.rigidbody;
			
			target.rotation = targetRotation;

			// Remember the rotation of the root relative to the pelvis
			rootRelativeToPelvis = Quaternion.Inverse(pelvisTarget.rotation) * transform.rotation;

			// Remember the position of the root relative to the pelvis
			pelvisToRoot = Quaternion.Inverse(ik.references.pelvis.rotation) * (transform.position - ik.references.pelvis.position);
			
			// Set effector weights
			ik.solver.leftHandEffector.positionWeight = 1f;
			ik.solver.leftHandEffector.rotationWeight = 1f;
		}
		
		void LateUpdate() {
			// Rotate the character to the ragdoll pelvis
			transform.rotation = pelvisTarget.rotation * rootRelativeToPelvis;

			// Position the character relative to the ragdoll pelvis
			transform.position = pelvisTarget.position + pelvisTarget.rotation * pelvisToRoot * hangingDistanceMlp;
			
			// Set ik effector positions
			ik.solver.leftHandEffector.position = leftHandTarget.position;
			ik.solver.leftHandEffector.rotation = leftHandTarget.rotation;

			// Get the normal hanging direction
			Vector3 dir = ik.references.pelvis.rotation * pelvisDownAxis;

			// Rotating the limbs
			// Get the rotation from normal hangind direction to the right arm ragdoll direction
			Quaternion rightArmRot = Quaternion.FromToRotation(dir, rightHandTarget.position - headTarget.position);
			// Rotate the right arm by that offset
			ik.references.rightUpperArm.rotation = rightArmRot * ik.references.rightUpperArm.rotation;
			
			Quaternion leftLegRot = Quaternion.FromToRotation(dir, leftFootTarget.position - bodyTarget.position);
			ik.references.leftThigh.rotation = leftLegRot * ik.references.leftThigh.rotation;
			
			Quaternion rightLegRot = Quaternion.FromToRotation(dir, rightFootTarget.position - bodyTarget.position);
			ik.references.rightThigh.rotation = rightLegRot * ik.references.rightThigh.rotation;
		}
	}
}
