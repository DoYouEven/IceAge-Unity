using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Resets an interaction object to it's initial position and rotation after "resetDelay" from interaction trigger.
	/// </summary>
	public class ResetInteractionObject : MonoBehaviour {

		public float resetDelay = 1f; // Time since interaction trigger to reset this Transform

		private Vector3 defaultPosition;
		private Quaternion defaultRotation;

		void Start() {
			// Store the defaults
			defaultPosition = transform.position;
			defaultRotation = transform.rotation;
		}

		// Called by the Interaction Object
		void OnPickUp(Transform t) {
			StopAllCoroutines();
			StartCoroutine(ResetObject(Time.time + resetDelay));
		}

		// Reset after a certain delay
		private IEnumerator ResetObject(float resetTime) {
			while (Time.time < resetTime) yield return null;

			var poser = transform.parent.GetComponent<Poser>();
			if (poser != null) {
				poser.poseRoot = null;
				poser.weight = 0f;
			}

			transform.parent = null;
			transform.position = defaultPosition;
			transform.rotation = defaultRotation;

			if (rigidbody != null) rigidbody.isKinematic = false;
		}
	}
}
