using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Absorbing motion on FBBIK effectors on impact. Attach this to the GameObject that receives OnCollisionEnter calls.
	/// </summary>
	public class MotionAbsorb : MonoBehaviour {

		// Manages motion absorbing for an effector
		[System.Serializable]
		public class Absorber {

			public FullBodyBipedEffector effector; // The type of effector (hand, foot, shoulder...) - this is just an enum
			public float weight = 1f; // How much should motion be absorbed on this effector

			// Set effector position and rotation to match it's bone
			public void SetToBone(IKSolverFullBodyBiped solver) {
				// Using world space position and rotation here for the sake of simplicity of the demo
				// Ideally we should use position and rotation relative to character's root, so we could move around while doing this.
				solver.GetEffector(effector).position = solver.GetEffector(effector).bone.position;
				solver.GetEffector(effector).rotation = solver.GetEffector(effector).bone.rotation;
			}

			// Set effector position and rotation weight to match the value, multiply with the weight of this Absorber
			public void SetEffectorWeights(IKSolverFullBodyBiped solver, float w) {
				solver.GetEffector(effector).positionWeight = w * weight;
				solver.GetEffector(effector).rotationWeight = w * weight;
			}
		}

		public FullBodyBipedIK ik; // Reference to the FBBIK component
		public Absorber[] absorbers; // Array containing the absorbers
		public float weight = 1f; // The master weight
		public AnimationCurve falloff; // Weight falloff curve  (how fast will the effect reduce after impact)
		public float falloffSpeed = 1f; // How fast will the impact fade away. (if 1, effect lasts for 1 second)

		private float timer; // Used for fading out the effect of the impact

		void OnCollisionEnter() {
			// Don't register another contact until the effect of the last one has faded 
			if (timer > 0f) return;

			// Start absorbing motion
			StartCoroutine(AbsorbMotion());
		}

		// Motion absorbing coroutine
		private IEnumerator AbsorbMotion() {
			// Reset timer
			timer = 1f;

			// Set effector position and rotation to match it's bone
			for (int i = 0; i < absorbers.Length; i++) absorbers[i].SetToBone(ik.solver);

			while (timer > 0) {
				// Fading out the effect
				timer -= Time.deltaTime * falloffSpeed;

				// Evaluate the absorb weight
				float w = falloff.Evaluate(timer);
				
				// Set the weights of the effectors
				for (int i = 0; i < absorbers.Length; i++) absorbers[i].SetEffectorWeights(ik.solver, w * weight);

				yield return null;
			}

			yield return null;
		}
	}
}
