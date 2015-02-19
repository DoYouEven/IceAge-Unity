using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RootMotion;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Managing Interactions for a single FBBIK effector.
	/// </summary>
	[System.Serializable]
	public class InteractionEffector {

		// The type of the effector
		public FullBodyBipedEffector effectorType { get; private set; }

		// Has the interaction been paused?
		public bool isPaused { get; private set; }
		// The current InteractionObject (null if there is no interaction going on)
		public InteractionObject interactionObject { get; private set; }
		// Is this InteractionEffector currently in the middle of an interaction?
		public bool inInteraction { get { return interactionObject != null; }}

		// Internal values
		private Poser poser;
		private IKEffector effector;
		private float timer, length, weight, fadeInSpeed, defaultPull, defaultReach, defaultPush, defaultPushParent, resetTimer;
		private bool pickedUp, defaults, pickUpOnPostFBBIK;
		private Vector3 pickUpPosition, pausePositionRelative;
		private Quaternion pickUpRotation, pauseRotationRelative;
		private InteractionTarget interactionTarget;
		private Transform target;
		private List<bool> triggered = new List<bool>();
		private InteractionSystem interactionSystem;

		// The custom constructor
		public InteractionEffector (FullBodyBipedEffector effectorType) {
			this.effectorType = effectorType;
		}

		// Initiate this, get the default values
		public void Initiate(InteractionSystem interactionSystem, IKSolverFullBodyBiped solver) {
			this.interactionSystem = interactionSystem;

			// Find the effector if we haven't already
			if (effector == null) {
				effector = solver.GetEffector(effectorType);
				poser = effector.bone.GetComponent<Poser>();
			}

			defaultPull = solver.GetChain(effectorType).pull;
			defaultReach = solver.GetChain(effectorType).reach;
			defaultPush = solver.GetChain(effectorType).push;
			defaultPushParent = solver.GetChain(effectorType).pushParent;
		}

		// Interpolate to default values when currently not in interaction
		public bool ResetToDefaults(IKSolverFullBodyBiped solver, float speed) {
			if (inInteraction) return false;
			if (isPaused) return false;
			if (defaults) return false; 

			resetTimer = Mathf.Clamp(resetTimer -= Time.deltaTime * speed, 0f, 1f);

			// Pull and Reach
			if (effector.isEndEffector) {
				solver.GetChain(effectorType).pull = Mathf.Lerp(defaultPull, solver.GetChain(effectorType).pull, resetTimer);
				solver.GetChain(effectorType).reach = Mathf.Lerp(defaultReach, solver.GetChain(effectorType).reach, resetTimer);
				solver.GetChain(effectorType).push = Mathf.Lerp(defaultPush, solver.GetChain(effectorType).push, resetTimer);
				solver.GetChain(effectorType).pushParent = Mathf.Lerp(defaultPushParent, solver.GetChain(effectorType).pushParent, resetTimer);
			}

			// Effector weights
			effector.positionWeight = Mathf.Lerp(0f, effector.positionWeight, resetTimer);
			effector.rotationWeight = Mathf.Lerp(0f, effector.rotationWeight, resetTimer);

			if (resetTimer <= 0f) defaults = true;

			return true;
		}

		// Pause this interaction
		public bool Pause() {
			if (!inInteraction) return false;
			isPaused = true;

			pausePositionRelative = target.InverseTransformPoint(effector.position);
			pauseRotationRelative = Quaternion.Inverse(target.rotation) * effector.rotation;

			if (interactionSystem.OnInteractionPause != null) {
				interactionSystem.OnInteractionPause(effectorType, interactionObject);
			}

			return true;
		}

		// Resume a paused interaction
		public bool Resume() {
			if (!inInteraction) return false;

			isPaused = false;
			if (interactionSystem.OnInteractionResume != null) interactionSystem.OnInteractionResume(effectorType, interactionObject);

			return true;
		}

		// Start interaction
		public bool Start(InteractionObject interactionObject, string tag, float fadeInTime, bool interrupt) {
			// If not in interaction, set effector positions to their bones
			if (!inInteraction) {
				effector.position = effector.bone.position;;
				effector.rotation = effector.bone.rotation;;
			} else {
				if (!interrupt) return false;
			}

			// Get the InteractionTarget
			target = interactionObject.GetTarget(effectorType, tag);
			if (target == null) return false;
			interactionTarget = target.GetComponent<InteractionTarget>();

			// Start the interaction
			this.interactionObject = interactionObject;
			if (interactionSystem.OnInteractionStart != null) interactionSystem.OnInteractionStart(effectorType, interactionObject);

			// Cleared triggered events
			triggered.Clear();

			for (int i = 0; i < interactionObject.events.Length; i++) {
				triggered.Add(false);
			}

			// Posing the hand/foot
			if (poser != null) {
				if (poser.poseRoot == null) poser.weight = 0f;

				if (interactionTarget != null) poser.poseRoot = target.transform;
				else poser.poseRoot = null;

				poser.AutoMapping();
			}

			// Reset internal values
			timer = 0f;
			weight = 0f;
			fadeInSpeed = fadeInTime > 0f? 1f / fadeInTime: 1000f;
			length = interactionObject.length;
			
			isPaused = false;
			pickedUp = false;
			pickUpPosition = Vector3.zero;
			pickUpRotation = Quaternion.identity;

			if (interactionTarget != null) interactionTarget.RotateTo(effector.bone.position);

			return true;
		}

		// Update the (possibly) ongoing interaction
		public void Update(Transform root, IKSolverFullBodyBiped solver, float speed) {
			if (!inInteraction) return;

			// Rotate target
			if (interactionTarget != null && !interactionTarget.rotateOnce) interactionTarget.RotateTo(effector.bone.position);

			if (isPaused) {
				effector.position = target.TransformPoint(pausePositionRelative);
				effector.rotation = target.rotation * pauseRotationRelative;

				// Apply the current interaction state to the solver
				interactionObject.Apply(solver, effectorType, interactionTarget, timer, weight);

				return;
			}

			// Advance the interaction timer and weight
			timer += Time.deltaTime * speed * (interactionTarget != null? interactionTarget.interactionSpeedMlp: 1f);
			weight = Mathf.Clamp(weight + Time.deltaTime * fadeInSpeed, 0f, 1f);

			// Interaction events
			bool pickUp = false;
			bool pause = false;
			TriggerUntriggeredEvents(true, out pickUp, out pause);

			// Effector target positions and rotations
			Vector3 targetPosition = pickedUp? pickUpPosition: target.position;
			Quaternion targetRotation = pickedUp? pickUpRotation: target.rotation;

			// Interpolate effector position and rotation
			effector.position = Vector3.Lerp(effector.bone.position, targetPosition, weight);
			effector.rotation = Quaternion.Lerp(effector.bone.rotation, targetRotation, weight);

			// Apply the current interaction state to the solver
			interactionObject.Apply(solver, effectorType, interactionTarget, timer, weight);

			if (pickUp) PickUp(root);
			if (pause) Pause();

			// Hand poser weight
			if (poser != null) {
				poser.weight = Mathf.Lerp(poser.weight, interactionObject.GetValue(InteractionObject.WeightCurve.Type.PoserWeight, interactionTarget, timer), weight);
			}

			if (timer >= length) Stop();
		}

		// Get the normalized progress of the interaction
		public float progress {
			get {
				if (!inInteraction) return 0f;
				if (length == 0f) return 0f;
				return timer / length;
			}
		}

		// Go through all the InteractionObject events to trigger the ones that have not yet triggered
		private void TriggerUntriggeredEvents(bool checkTime, out bool pickUp, out bool pause) {
			pickUp = false;
			pause = false;

			for (int i = 0; i < triggered.Count; i++) {
				// If this event has not been triggered by this effector
				if (!triggered[i]) {
					
					// If time has passed...
					if (!checkTime || interactionObject.events[i].time < timer) {
						
						// Activate the event
						interactionObject.events[i].Activate(effector.bone);
						
						// Picking up
						if (interactionObject.events[i].pickUp) {
							if (timer >= interactionObject.events[i].time) timer = interactionObject.events[i].time;

							pickUp = true;
						}
						
						// Pausing
						if (interactionObject.events[i].pause) {
							if (timer >= interactionObject.events[i].time) timer = interactionObject.events[i].time;

							pause = true;
						}
						
						if (interactionSystem.OnInteractionEvent != null) interactionSystem.OnInteractionEvent(effectorType, interactionObject, interactionObject.events[i]);
						
						triggered[i] = true;
					}
				}
			}
		}

		// Trigger the interaction object
		private void PickUp(Transform root) {
			// Picking up the object
			pickUpPosition = effector.position;
			pickUpRotation = effector.rotation;
				
			pickUpOnPostFBBIK = true;

			pickedUp = true;

			if (interactionObject.targetsRoot.rigidbody != null) {
				if (!interactionObject.targetsRoot.rigidbody.isKinematic) {
					interactionObject.targetsRoot.rigidbody.isKinematic = true;
				}

				// Ignore collisions between the character and the colliders of the interaction object
				if (root.collider != null) {
					var colliders = interactionObject.targetsRoot.GetComponentsInChildren<Collider>();

					foreach (Collider collider in colliders) {
						if (!collider.isTrigger) Physics.IgnoreCollision(root.collider, collider);
					}
				}
			}
				
			if (interactionSystem.OnInteractionPickUp != null) interactionSystem.OnInteractionPickUp(effectorType, interactionObject);
		}

		// Stop the interaction
		public bool Stop() {
			if (!inInteraction) return false;

			bool pickUp = false;
			bool pause = false;
			TriggerUntriggeredEvents(false, out pickUp, out pause);

			if (interactionSystem.OnInteractionStop != null) interactionSystem.OnInteractionStop(effectorType, interactionObject);

			// Reset the interaction target
			if (interactionTarget != null) interactionTarget.ResetRotation();

			// Reset the internal values
			interactionObject = null;
			weight = 0f;
			timer = 0f;

			isPaused = false;
			target = null;
			defaults = false;
			resetTimer = 1f;
			if (poser != null && !pickedUp) poser.weight = 0f;
			pickedUp = false;

			return true;
		}

		// Called after FBBIK update
		public void OnPostFBBIK(IKSolverFullBodyBiped fullBody) {
			if (!inInteraction) return;

			// Rotate the hands/feet to the RotateBoneWeight curve
			float rotateBoneWeight = interactionObject.GetValue(InteractionObject.WeightCurve.Type.RotateBoneWeight, interactionTarget, timer) * weight;

			if (rotateBoneWeight > 0f) {
				Quaternion r = pickedUp? pickUpRotation: effector.rotation;

				Quaternion targetRotation = Quaternion.Slerp(effector.bone.rotation, r, rotateBoneWeight * rotateBoneWeight);
				effector.bone.localRotation = Quaternion.Inverse(effector.bone.parent.rotation) * targetRotation;
			}

			// Positioning the interaction object to the effector (not the bone, because it is still at it's animated translation)
			if (pickUpOnPostFBBIK) {
				interactionObject.targetsRoot.parent = effector.bone;
				interactionObject.targetsRoot.localPosition = Quaternion.Inverse(pickUpRotation) * (interactionObject.targetsRoot.position - pickUpPosition);
				//interactionObject.targetsRoot.localRotation = Quaternion.Inverse(pickUpRotation) * interactionObject.targetsRoot.rotation;

				pickUpOnPostFBBIK = false;
			}
		}
	}
}
