using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// The base abstract class for all character controllers, provides common functionality.
	/// </summary>
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	public abstract class CharacterBase: MonoBehaviour {

		[SerializeField] protected float airborneThreshold = 0.6f; // Height from ground after which the character is considered airborne
		[SerializeField] float slopeStartAngle = 50f; // The start angle of velocity dampering on slopes
		[SerializeField] float slopeEndAngle = 85f; // The end angle of velocity dampering on slopes
		[SerializeField] float spherecastRadius = 0.1f; // The radius of sperecasting
		[SerializeField] LayerMask groundLayers; // The walkable layers
		[SerializeField] PhysicMaterial zeroFrictionMaterial; // Minimum friction for movement
		[SerializeField] PhysicMaterial highFrictionMaterial; // Maximum friction for standing still

		protected const float half = 0.5f;
		protected float originalHeight;
		protected Vector3 originalCenter;
		protected CapsuleCollider capsule;

		public abstract void Move(Vector3 deltaPosition);

		protected virtual void Start() {
			capsule = collider as CapsuleCollider;

			// Store the collider volume
			originalHeight = capsule.height;
			originalCenter = capsule.center;

			// Making sure rigidbody rotation is fixed
			rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
		}

		// Spherecast from the root to find ground height
		protected virtual RaycastHit GetSpherecastHit() {
			Ray ray = new Ray (rigidbody.position + Vector3.up * airborneThreshold, Vector3.down);
			RaycastHit h = new RaycastHit();
			
			Physics.SphereCast(ray, spherecastRadius, out h, airborneThreshold * 2f, groundLayers);
			return h;
		}

		// Gets angle around y axis from a world space direction
		public float GetAngleFromForward(Vector3 worldDirection) {
			Vector3 local = transform.InverseTransformDirection(worldDirection);
			return Mathf.Atan2 (local.x, local.z) * Mathf.Rad2Deg;
		}

		// Rotate a rigidbody around a point and axis by angle
		protected void RigidbodyRotateAround(Vector3 point, Vector3 axis, float angle) {
			Quaternion r = Quaternion.AngleAxis(angle, axis);
			Vector3 d = rigidbody.position - point;
			rigidbody.MovePosition(point + r * d);
			rigidbody.MoveRotation(r * rigidbody.rotation);
		}

		// Scale the capsule collider to 'mlp' of the initial value
		protected void ScaleCapsule (float mlp) {
			if (capsule.height != originalHeight * mlp) {
				capsule.height = Mathf.MoveTowards (capsule.height, originalHeight * mlp, Time.deltaTime * 4);
				capsule.center = Vector3.MoveTowards (capsule.center, originalCenter * mlp, Time.deltaTime * 2);
			}
		}

		// Set the collider to high friction material
		protected void HighFriction() {
			collider.material = highFrictionMaterial;
		}

		// Set the collider to zero friction material
		protected void ZeroFriction() {
			collider.material = zeroFrictionMaterial;
		}

		// Get the damper of velocity on the slopes
		protected float GetSlopeDamper(Vector3 velocity, Vector3 groundNormal) {
			float angle = 90f - Vector3.Angle(velocity, groundNormal);
			angle -= slopeStartAngle;
			float range = slopeEndAngle - slopeStartAngle;
			return 1f - Mathf.Clamp(angle / range, 0f, 1f);
		}
	}

}
