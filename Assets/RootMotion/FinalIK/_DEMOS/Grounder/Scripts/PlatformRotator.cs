using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Moving and rotating platforms.
	/// </summary>
	public class PlatformRotator : MonoBehaviour {

		public float maxAngle = 70f; // Maximum angular offset from the default rotation
		public float switchRotationTime = 0.5f; // Base time for switching to another target rotation
		public float random = 0.5f; // The random mlp for timers
		public float rotationSpeed = 50f; // The slerp speed
		public Vector3 movePosition; // Move to offset
		public float moveSpeed = 5f; // Moving speed

		private Quaternion defaultRotation;
		private Quaternion targetRotation;
		private Vector3 targetPosition;
		private Vector3 velocity;

		void Start () {
			// Store defaults
			defaultRotation = transform.rotation;
			targetPosition = transform.position + movePosition;

			// Start switching target rotations
			StartCoroutine(SwitchRotation());
		}

		void FixedUpdate() {
			// Moving
			rigidbody.MovePosition(Vector3.SmoothDamp(rigidbody.position, targetPosition, ref velocity, 1f, moveSpeed));

			if (Vector3.Distance(rigidbody.position, targetPosition) < 0.1f) {
				movePosition = -movePosition;
				targetPosition += movePosition;
			}

			// Rotating
			rigidbody.rotation = Quaternion.RotateTowards(rigidbody.rotation, targetRotation, rotationSpeed * Time.deltaTime);
		}

		// Switching the  target rotation
		private IEnumerator SwitchRotation() {
			while (true) {
				// Random rotation around a random axis
				float angle = UnityEngine.Random.Range(-maxAngle, maxAngle);
				Vector3 axis = UnityEngine.Random.onUnitSphere;
				targetRotation = Quaternion.AngleAxis(angle, axis) * defaultRotation;

				yield return new WaitForSeconds(switchRotationTime + UnityEngine.Random.value * random);
			}
		}
	}
}
