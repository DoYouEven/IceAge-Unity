using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Handles FBBIK interactions for a character.
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction System")]
	public class InteractionSystem : MonoBehaviour {

		#region Main Interface

		/// <summary>
		/// If not empty, only the targets with the specified tag will be used by this interaction system.
		/// </summary>
		public string targetTag = "";
		/// <summary>
		/// The fade in time of the interaction.
		/// </summary>
		public float fadeInTime = 0.3f;
		/// <summary>
		/// The master speed for all interactions.
		/// </summary>
		public float speed = 1f;
		/// <summary>
		/// If > 0, lerps all the FBBIK channels used by the Interaction System back to their default or initial values when not in interaction
		/// </summary>
		public float resetToDefaultsSpeed = 1f;

		/// <summary>
		/// Returns true if any of the effectors is in interaction and not paused.
		/// </summary>
		public bool inInteraction {
			get {
				if (!IsValid(true)) return false;

				for (int i = 0; i < interactionEffectors.Length; i++) {
					if (interactionEffectors[i].inInteraction && !interactionEffectors[i].isPaused) return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Determines whether this effector is interaction and not paused
		/// </summary>
		public bool IsInInteraction(FullBodyBipedEffector effectorType) {
			if (!IsValid(true)) return false;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].effectorType == effectorType) {
					return interactionEffectors[i].inInteraction && !interactionEffectors[i].isPaused;
				}
			}
			return false;
		}

		/// <summary>
		/// Determines whether this effector is  paused
		/// </summary>
		public bool IsPaused(FullBodyBipedEffector effectorType) {
			if (!IsValid(true)) return false;
			
			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].effectorType == effectorType) {
					return interactionEffectors[i].inInteraction && interactionEffectors[i].isPaused;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns true if any of the effectors is paused
		/// </summary>
		public bool IsPaused() {
			if (!IsValid(true)) return false;
			
			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].inInteraction && interactionEffectors[i].isPaused) return true;
			}
			return false;
		}

		/// <summary>
		/// Returns true if either all effectors in interaction are paused or none is.
		/// </summary>
		public bool IsInSync() {
			if (!IsValid(true)) return false;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].isPaused) {
					for (int n = 0; n < interactionEffectors.Length; n++) {
						if (n != i && interactionEffectors[n].inInteraction && !interactionEffectors[n].isPaused) return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Starts the interaction between an effector and an interaction object.
		/// </summary>
		public bool StartInteraction(FullBodyBipedEffector effectorType, InteractionObject interactionObject, bool interrupt) {
			if (!IsValid(true)) return false;

			if (interactionObject == null) return false;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].effectorType == effectorType) {
					return interactionEffectors[i].Start(interactionObject, targetTag, fadeInTime, interrupt);
				}
			}

			return false;
		}

		/// <summary>
		/// Pauses the interaction of an effector.
		/// </summary>
		public bool PauseInteraction(FullBodyBipedEffector effectorType) {
			if (!IsValid(true)) return false;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].effectorType == effectorType) {
					return interactionEffectors[i].Pause();
				}
			}

			return false;
		}

		/// <summary>
		/// Resumes the paused interaction of an effector.
		/// </summary>
		public bool ResumeInteraction(FullBodyBipedEffector effectorType) {
			if (!IsValid(true)) return false;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].effectorType == effectorType) {
					return interactionEffectors[i].Resume();
				}
			}

			return false;
		}

		/// <summary>
		/// Stops the interaction of an effector.
		/// </summary>
		public bool StopInteraction(FullBodyBipedEffector effectorType) {
			if (!IsValid(true)) return false;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].effectorType == effectorType) {
					return interactionEffectors[i].Stop();
				}
			}

			return false;
		}

		/// <summary>
		/// Pauses all the interaction effectors.
		/// </summary>
		public void PauseAll() {
			if (!IsValid(true)) return;

			for (int i = 0; i < interactionEffectors.Length; i++) interactionEffectors[i].Pause();
		}

		/// <summary>
		/// Resumes all the paused interaction effectors.
		/// </summary>
		public void ResumeAll() {
			if (!IsValid(true)) return;

			for (int i = 0; i < interactionEffectors.Length; i++) interactionEffectors[i].Resume();
		}

		/// <summary>
		/// Stops all interactions.
		/// </summary>
		public void StopAll() {
			for (int i = 0; i < interactionEffectors.Length; i++) interactionEffectors[i].Stop();
		}

		/// <summary>
		/// Gets the current interaction object of an effector.
		/// </summary>
		public InteractionObject GetInteractionObject(FullBodyBipedEffector effectorType) {
			if (!IsValid(true)) return null;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].effectorType == effectorType) {
					return interactionEffectors[i].interactionObject;
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the progress of any interaction with the specified effector.
		/// </summary>
		public float GetProgress(FullBodyBipedEffector effectorType) {
			if (!IsValid(true)) return 0f;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].effectorType == effectorType) {
					return interactionEffectors[i].progress;
				}
			}
			return 0f;
		}

		/// <summary>
		/// Gets the minimum progress of any active interaction
		/// </summary>
		public float GetMinActiveProgress() {
			if (!IsValid(true)) return 0f;
			float min = 1f;

			for (int i = 0; i < interactionEffectors.Length; i++) {
				if (interactionEffectors[i].inInteraction) {
					float p = interactionEffectors[i].progress;
					if (p > 0f && p < min) min = p;
				}
			}

			return min;
		}

		/// <summary>
		/// Triggers all interactions of an InteractionTrigger. Returns false if unsuccessful (maybe out of range).
		/// </summary>
		public bool TriggerInteraction(int index, bool interrupt) {
			if (!IsValid(true)) return false;

			if (!TriggerIndexIsValid(index)) return false;

			bool all = true;

			var range = triggersInRange[index].ranges[bestRangeIndexes[index]];

			for (int i = 0; i < range.interactions.Length; i++) {
				for (int e = 0; e < range.interactions[i].effectors.Length; e++) {
					bool s = StartInteraction(range.interactions[i].effectors[e], range.interactions[i].interactionObject, interrupt);
					if (!s) all = false;
				}
			}

			return all;
		}

		/// <summary>
		/// Returns true if all effectors of a trigger are either not in interaction or paused
		/// </summary>
		public bool TriggerEffectorsReady(int index) {
			if (!IsValid(true)) return false;
			
			if (!TriggerIndexIsValid(index)) return false;


			for (int r = 0; r < triggersInRange[index].ranges.Length; r++) {
				var range = triggersInRange[index].ranges[r];

				for (int i = 0; i < range.interactions.Length; i++) {
					for (int e = 0; e < range.interactions[i].effectors.Length; e++) {
						if (IsInInteraction(range.interactions[i].effectors[e])) return false;
					}
				}

				for (int i = 0; i < range.interactions.Length; i++) {
					for (int e = 0; e < range.interactions[i].effectors.Length; e++) {
						if (IsPaused(range.interactions[i].effectors[e])) {
							for (int n = 0; n < range.interactions[i].effectors.Length; n++) {
								if (n != e && !IsPaused(range.interactions[i].effectors[n])) return false;
							}
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Return the current most appropriate range of an InteractionTrigger listed in triggersInRange.
		/// </summary>
		public InteractionTrigger.Range GetTriggerRange(int index) {
			if (!IsValid(true)) return null;
			
			if (index < 0 || index >= bestRangeIndexes.Count) {
				Warning.Log("Index out of range.", transform);
				return null;
			}

			return triggersInRange[index].ranges[bestRangeIndexes[index]];
		}

		/// <summary>
		/// Returns the InteractionTrigger that is in range and closest to the character.
		/// </summary>
		public int GetClosestTriggerIndex() {
			if (!IsValid(true)) return -1;

			if (triggersInRange.Count == 0) return -1;
			if (triggersInRange.Count == 1) return 0;

			int closest = -1;
			float closestSqrMag = Mathf.Infinity;

			for (int i = 0; i < triggersInRange.Count; i++) {
				if (triggersInRange[i] != null) {
					float sqrMag = Vector3.SqrMagnitude(triggersInRange[i].transform.position - transform.position);

					if (sqrMag < closestSqrMag) {
						closest = i;
						closestSqrMag = sqrMag;
					}
				}
			}

			return closest;
		}

		/// <summary>
		/// Gets the FullBodyBipedIK component.
		/// </summary>
		public FullBodyBipedIK ik {
			get {
				return fullBody;
			}
		}

		/// <summary>
		/// Gets the in contact.
		/// </summary>
		/// <value>The in contact.</value>
		public List<InteractionTrigger> triggersInRange { get; private set; }
		private List<InteractionTrigger> inContact = new List<InteractionTrigger>();
		private List<int> bestRangeIndexes = new List<int>();

		/// <summary>
		/// Interaction delegate
		/// </summary>
		public delegate void InteractionDelegate(FullBodyBipedEffector effectorType, InteractionObject interactionObject);
		/// <summary>
		/// Interaction event delegate
		/// </summary>
		public delegate void InteractionEventDelegate(FullBodyBipedEffector effectorType, InteractionObject interactionObject, InteractionObject.InteractionEvent interactionEvent);

		/// <summary>
		/// Called when an InteractionEvent has been started
		/// </summary>
		public InteractionDelegate OnInteractionStart;
		/// <summary>
		/// Called when an Interaction has been paused
		/// </summary>
		public InteractionDelegate OnInteractionPause;
		/// <summary>
		/// Called when an InteractionObject has been picked up.
		/// </summary>
		public InteractionDelegate OnInteractionPickUp;
		/// <summary>
		/// Called when a paused Interaction has been resumed
		/// </summary>
		public InteractionDelegate OnInteractionResume;
		/// <summary>
		/// Called when an Interaction has been stopped
		/// </summary>
		public InteractionDelegate OnInteractionStop;
		/// <summary>
		/// Called when an interaction event occurs.
		/// </summary>
		public InteractionEventDelegate OnInteractionEvent;

		#endregion Main Interface

		[SerializeField] FullBodyBipedIK fullBody; // Reference to the FBBIK component.

		/// <summary>
		/// Handles looking at the interactions.
		/// </summary>
		public InteractionLookAt lookAt = new InteractionLookAt();

		// The array of Interaction Effectors
		private InteractionEffector[] interactionEffectors = new InteractionEffector[9] {
			new InteractionEffector(FullBodyBipedEffector.Body),
			new InteractionEffector(FullBodyBipedEffector.LeftFoot),
			new InteractionEffector(FullBodyBipedEffector.LeftHand),
			new InteractionEffector(FullBodyBipedEffector.LeftShoulder),
			new InteractionEffector(FullBodyBipedEffector.LeftThigh),
			new InteractionEffector(FullBodyBipedEffector.RightFoot),
			new InteractionEffector(FullBodyBipedEffector.RightHand),
			new InteractionEffector(FullBodyBipedEffector.RightShoulder),
			new InteractionEffector(FullBodyBipedEffector.RightThigh)
		};

		private bool initiated;

		// Initiate
		protected virtual void Start() {
			if (fullBody == null) fullBody = GetComponent<FullBodyBipedIK>();
			if (fullBody == null) {
				Warning.Log("InteractionSystem can not find a FullBodyBipedIK component", transform);
				return;
			}

			// Add to the FBBIK OnPostUpdate delegate to get a call when it has finished updating
			fullBody.solver.OnPreUpdate += OnPreFBBIK;
			fullBody.solver.OnPostUpdate += OnPostFBBIK;
			OnInteractionStart += LookAtInteraction;

			foreach (InteractionEffector e in interactionEffectors) e.Initiate(this, fullBody.solver);
	
			triggersInRange = new List<InteractionTrigger>();
			
			initiated = true;
		}

		// Called by the delegate
		private void LookAtInteraction(FullBodyBipedEffector effector, InteractionObject interactionObject) {
			lookAt.Look(interactionObject.lookAtTarget, Time.time + (interactionObject.length * 0.5f));
		}

		public void OnTriggerEnter(Collider c) {
			if (fullBody == null) return;

			var trigger = c.GetComponent<InteractionTrigger>();
			
			if (inContact.Contains(trigger)) return;
			
			inContact.Add(trigger);
		}
		
		public void OnTriggerExit(Collider c) {
			if (fullBody == null) return;

			var trigger = c.GetComponent<InteractionTrigger>();
			
			inContact.Remove(trigger);
		}

		// Is the InteractionObject trigger in range of any effectors? If the trigger collider is bigger than any effector ranges, then the object in contact is still unreachable.
		private bool ContactIsInRange(int index, out int bestRangeIndex) {
			bestRangeIndex = -1;

			if (!IsValid(true)) return false;
			
			if (index < 0 || index >= inContact.Count) {
				Warning.Log("Index out of range.", transform);
				return false;
			}
			
			if (inContact[index] == null) {
				Warning.Log("The InteractionTrigger in the list 'inContact' has been destroyed", transform);
				return false;
			}
			
			bestRangeIndex = inContact[index].GetBestRangeIndex(transform);
			if (bestRangeIndex == -1) return false;
			
			return true;
		}

		void OnDrawGizmosSelected() {
			if (fullBody == null) fullBody = GetComponent<FullBodyBipedIK>();
		}
		
		void Update() {
			if (fullBody == null) return;

			// Finding the triggers in contact and in range
			triggersInRange.Clear();
			bestRangeIndexes.Clear();

			for (int i = 0; i < inContact.Count; i++) {
				int bestRangeIndex = -1;

				if (inContact[i] != null && ContactIsInRange(i, out bestRangeIndex)) {
					triggersInRange.Add(inContact[i]);
					bestRangeIndexes.Add(bestRangeIndex);
				}
			}

			// Update LookAt
			lookAt.Update();
		}

		// Update the interaction
		void LateUpdate() {
			if (fullBody == null) return;

			for (int i = 0; i < interactionEffectors.Length; i++) interactionEffectors[i].Update(transform, fullBody.solver, speed);

			// Interpolate to default pull, reach values
			for (int i = 0; i < interactionEffectors.Length; i++) interactionEffectors[i].ResetToDefaults(fullBody.solver, resetToDefaultsSpeed);
		}

		// Used for using LookAtIK to rotate the spine
		private void OnPreFBBIK() {
			if (!enabled) return;
			if (fullBody == null) return;
			
			lookAt.SolveSpine();
		}

		// Used for rotating the hands after FBBIK has finished
		private void OnPostFBBIK() {
			if (!enabled) return;
			if (fullBody == null) return;

			for (int i = 0; i < interactionEffectors.Length; i++) interactionEffectors[i].OnPostFBBIK(fullBody.solver);

			// Update LookAtIK head
			lookAt.SolveHead();
		}

		// Remove the delegates
		void OnDestroy() {
			if (fullBody == null) return;
			fullBody.solver.OnPreUpdate -= OnPreFBBIK;
			fullBody.solver.OnPostUpdate -= OnPostFBBIK;

			OnInteractionStart -= LookAtInteraction;
		}

		// Is this InteractionSystem valid and initiated
		private bool IsValid(bool log) {
			if (fullBody == null) {
				if (log) Warning.Log("FBBIK is null. Will not update the InteractionSystem", transform);
				return false;
			}
			if (!initiated) {
				if (log) Warning.Log("The InteractionSystem has not been initiated yet.", transform);
				return false;
			}
			return true;
		}

		// Is the index of triggersInRange valid?
		private bool TriggerIndexIsValid(int index) {
			if (index < 0 || index >= triggersInRange.Count) {
				Warning.Log("Index out of range.", transform);
				return false;
			}
			
			if (triggersInRange[index] == null) {
				Warning.Log("The InteractionTrigger in the list 'inContact' has been destroyed", transform);
				return false;
			}
			
			if (triggersInRange[index].target == null) {
				Warning.Log("InteractionTrigger has no target Transform", transform);
				return false;
			}
			
			return true;
		}

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		private void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page10.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		private void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_system.html");
		}
	}
}
