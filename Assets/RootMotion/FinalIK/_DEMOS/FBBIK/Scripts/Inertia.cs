using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Demo script that adds the illusion of mass to your character using FullBodyBipedIK.
	/// </summary>
	public class Inertia : OffsetModifier {

		/// <summary>
		/// Body is just following it's transform in a lazy and bouncy way.
		/// </summary>
		[System.Serializable]
		public class Body {

			/// <summary>
			/// Linking this to an effector
			/// </summary>
			[System.Serializable]
			public class EffectorLink {
				public FullBodyBipedEffector effector;
				public float weight;
			}

			public Transform transform; // The Transform to follow, can be any bone of the character
			public EffectorLink[] effectorLinks; // Linking the body to effectors. One Body can be used to offset more than one effector.
			public float speed = 10f; // The speed to follow the Transform
			public float acceleration = 3f; // The acceleration, smaller values means lazyer following
			public float matchVelocity; // 0-1 matching target velocity
			public float gravity; // gravity applied to the Body

			private Vector3 delta;
			private Vector3 lazyPoint;
			private Vector3 direction;
			private Vector3 lastPosition;
			private bool firstUpdate = true;

			// Reset to Transform
			public void Reset() {
				if (transform == null) return;
				lazyPoint = transform.position;
				lastPosition = transform.position;
				direction = Vector3.zero;
			}

			// Update this body, apply the offset to the effector
			public void Update(IKSolverFullBodyBiped solver, float weight, float deltaTime) {
				if (transform == null) return;

				// If first update, set this body to Transform
				if (firstUpdate) {
					Reset();
					firstUpdate = false;
				}

				// Acceleration
				direction = Vector3.Lerp(direction, ((transform.position - lazyPoint) / deltaTime) * 0.01f, deltaTime * acceleration);

				// Lazy follow
				lazyPoint += direction * deltaTime * speed;

				// Match velocity
				delta = transform.position - lastPosition;
				lazyPoint += delta * matchVelocity;
				
				// Gravity
				lazyPoint.y += gravity * deltaTime;

				// Apply position offset to the effector
				foreach (EffectorLink effectorLink in effectorLinks) {
					solver.GetEffector(effectorLink.effector).positionOffset += (lazyPoint - transform.position) * effectorLink.weight * weight;
				}

				lastPosition = transform.position;
			}
		}

		public Body[] bodies; // The array of Bodies
		public OffsetLimits[] limits; // The array of OffsetLimits

		// Reset all Bodies
		public void ResetBodies() {
			foreach (Body body in bodies) body.Reset();
		}

		// Called by IKSolverFullBody before updating
		protected override void OnModifyOffset() {
			// Update the Bodies
			foreach (Body body in bodies) body.Update(ik.solver, weight, deltaTime);

			// Apply the offset limits
			ApplyLimits(limits);
		}
	}
}
