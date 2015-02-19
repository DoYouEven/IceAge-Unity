using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK {

	/// <summary>
	/// If a character with an InteractionSystem component enters the trigger collider of this game object, this component will register itself to the InteractionSystem. 
	/// The InteractionSystem can then use it to find the most appropriate InteractionObject and effectors to interact with.
	/// Use InteractionSystem.GetClosestTriggerIndex() and InteractionSystem.TriggerInteration() to trigger the interactions that the character is in contact with.
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Trigger")]
	public class InteractionTrigger: MonoBehaviour {

		/// <summary>
		/// Defines the position and rotation of the character in which it is in range with the specified interaction
		/// </summary>
		[System.Serializable]
		public class Range {

			/// <summary>
			/// Defines the interaction object and effectors that will be triggered when calling InteractionSystem.TriggerInteraction().
			/// </summary>
			[System.Serializable]
			public class Interaction {
				/// <summary>
				/// The InteractionObject to interact with.
				/// </summary>
				public InteractionObject interactionObject;
				/// <summary>
				/// The effectors to interact with.
				/// </summary>
				public FullBodyBipedEffector[] effectors;
			}

			/// <summary>
			/// Definitions of the interactions associated with this range.
			/// </summary>
			public Interaction[] interactions;
			/// <summary>
			/// The position offset from the position of the InteractionTrigger game object.
			/// </summary>
			public Vector3 positionOffset;
			/// <summary>
			/// If true, the character can be positioned anywhere on the orbit around the InteractionTrigger game object (min radius of the orbit is positionOffset.magnitude - maxDistance, max is positionOffset.magnitude + maxDistance).
			/// </summary>
			public bool orbit;
			/// <summary>
			/// The maximum distance from the origin (transform.position + transform.rotation * positionOffset) of this range.
			/// </summary>
			public float maxDistance = 0.5f;
			/// <summary>
			/// The angular offset of the range. Defines the direction of the character's forward in which it is in range to start the interactions.
			/// </summary>
			[Range(-180f, 180f)] public float angleOffset;
			/// <summary>
			/// The maximum angular offset from the direction.
			/// </summary>
			[Range(0f, 180f)] public float maxAngle = 50f;

			// Is the character in range?
			public bool IsInRange(Vector3 transformPosition, Vector3 triggerPosition, Vector3 objectPosition, Transform character, out float angle) {
				angle = 180f;

				if (orbit) {
					float mag = positionOffset.magnitude;
					float dist = Vector3.Distance(character.position, transformPosition);
					if (dist < mag - maxDistance || dist > mag + maxDistance) return false;
				} else {
					if (Vector3.Distance(character.position, triggerPosition) > maxDistance) return false;
				}

				if (character.position == objectPosition) return true;

				Vector3 direction = objectPosition - character.position;
				Vector3 normal = character.up;
				Vector3.OrthoNormalize(ref normal, ref direction);
				direction = Quaternion.AngleAxis(angleOffset, normal) * direction;

				Vector3 characterAxis = character.forward;

				angle = Vector3.Angle(direction, characterAxis);
				if (angle > maxAngle) return false;

				return true;
			}
		}

		/// <summary>
		/// The target Transform. Vector from transform.position to target.position will define the orientation of this trigger.
		/// </summary>
		public Transform target;
		/// <summary>
		/// The interaction ranges.
		/// </summary>
		public Range[] ranges;

		// Returns the most appropriate range of interaction based on the position and rotation of the character.
		public int GetBestRangeIndex(Transform character) {
			if (collider == null) {
				Warning.Log("Using the InteractionTrigger requires a Collider component.", transform);
				return -1;
			}

			if (target == null) {
				Warning.Log("InteractionTrigger has no target Transform.", transform);
				return -1;
			}

			int bestRangeIndex = -1;
			float smallestAngle = 180f;

			for (int i = 0; i < ranges.Length; i++) {
				Vector3 position = transform.position + transform.rotation * ranges[i].positionOffset;

				float angle = 0f;

				if (ranges[i].IsInRange(transform.position, position, target.position, character, out angle)) {
					if (angle <= smallestAngle) {
						smallestAngle = angle;
						bestRangeIndex = i;
					}
				}
			}

			return bestRangeIndex;
		}
	}
}
