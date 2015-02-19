using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Manages solver initiation and updating
	/// </summary>
	public class SolverManager: MonoBehaviour {
		
		#region Main Interface
		
		/// <summary>
		/// The updating frequency
		/// </summary>
		public float timeStep;
		/// <summary>
		/// If true, will fix all the Transforms used by the solver to their default local states in each Update.
		/// </summary>
		public bool fixTransforms = true;

		/// <summary>
		/// Safely disables this component, making sure the solver is still initated. Use this instead of "enabled = false" if you need to disable the component to manually control it's updating.
		/// </summary>
		public void Disable() {
			Initiate();
			enabled = false;
		}
		
		#endregion Main

		protected virtual void InitiateSolver() {}
		protected virtual void UpdateSolver() {}
		protected virtual void FixTransforms() {}
		
		private float lastTime;
		private Animator animator;
		private new Animation animation;
		private bool updateFrame;
		private bool componentInitiated;

		private bool animatePhysics {
			get {
				if (animator != null) return animator.updateMode == AnimatorUpdateMode.AnimatePhysics;
				if (animation != null) return animation.animatePhysics;
				return false;
			}
		}

		void Start() {
			Initiate();
		}

		void Update() {
			if (animatePhysics) return;

			if (fixTransforms) FixTransforms();
		}

		private void Initiate() {
			if (componentInitiated) return;

			FindAnimatorRecursive(transform, true);

			InitiateSolver();
			componentInitiated = true;
		}

		// Finds the first Animator/Animation up the hierarchy
		private void FindAnimatorRecursive(Transform t, bool findInChildren) {
			if (isAnimated) return;

			animator = t.GetComponent<Animator>();
			animation = t.GetComponent<Animation>();

			if (isAnimated) return;

			if (animator == null && findInChildren) animator = t.GetComponentInChildren<Animator>();
			if (animation == null && findInChildren) animation = t.GetComponentInChildren<Animation>();

			if (!isAnimated && t.parent != null) FindAnimatorRecursive(t.parent, false);
		}

		private bool isAnimated {
			get {
				return animator != null || animation != null;
			}
		}

		/*
		 * Workaround hack for the solver to work with animatePhysics
		 * */
		void FixedUpdate() {
			updateFrame = true;

			if (animatePhysics && fixTransforms) FixTransforms();
		}

		/*
		 * Updating by timeStep
		 * */
		void LateUpdate() {
			// Check if either animatePhysics is false or FixedUpdate has been called
			if (!animatePhysics) updateFrame = true;
			if (!updateFrame) return;
			updateFrame = false;

			if (timeStep == 0) UpdateSolver();
			else {
				if (Time.time >= lastTime + timeStep) {
					UpdateSolver();
					lastTime = Time.time;
				}
			}
		}
	}
}
