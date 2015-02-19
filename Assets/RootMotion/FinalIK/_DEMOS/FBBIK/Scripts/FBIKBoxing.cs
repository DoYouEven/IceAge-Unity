using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Demo script for boxing with the combination of FullBodyBipedIK and Aim IK.
	/// </summary>
	public class FBIKBoxing : MonoBehaviour {
		
		public Transform target; // The target we want to hit

		/// <summary>
		/// The pin Transform is used to reference the exact hit point in the animation (used by AimIK to aim the upper body to follow the target).
		/// In Legacy and Generic modes you can just create and position a reference point in your animating software and include it in the FBX. 
		/// Then in Unity if you added a GameObject with the exact same name under the character's root, it would be animated to the required position.
		/// In Humanoid mode however, Mecanim loses track of any Transform that does not belong to the avatar, so in this case the pin point has to be manually set inside the Unity Editor.
		/// </summary>
		public Transform pin;

		public FullBodyBipedIK ik; // The Full Body Biped IK component
		public AimIK aim; // The Aim IK component. Aim IK is ust used for following the target slightly with the body.
		public float weight; // The master weight
		public FullBodyBipedEffector effector; // The effector type of the punching hand
		public AnimationCurve unityFreeHitWeight; // Workaround for Unity Free users that don't have access to Mecanim curves
		public AnimationCurve aimWeight; // Weight of aiming the body to follow the target

		private Animator animator;
		private AnimatorStateInfo info;

		/*
		 * Get the weight of pinning the fist to the target
		 * */
		private float GetHitWeight() {
			// Workaround for Unity Free users
			// If you have Unity Pro, use Mecanim curves instead: return animator.GetFloat("HitWeight");
			info = animator.GetCurrentAnimatorStateInfo(0);

			if (info.IsName("Boxing")) {
				return unityFreeHitWeight.Evaluate(info.normalizedTime);
			}

			return 0;
		}

		void Start() {
			animator = GetComponent<Animator>();

			// Disable the IK components to update them manually in a specific order
			ik.Disable();
			if (aim != null) aim.Disable();
		}

		void LateUpdate() {
			// Getting the weight of pinning the fist to the target
			float hitWeight = GetHitWeight();

			// Pinning the first with FBIK
			ik.solver.GetEffector(effector).position = target.position;
			ik.solver.GetEffector(effector).positionWeight = hitWeight * weight;

			// Aiming the body with AimIK to follow the target
			if (aim != null) {
				// Make the aim transform always look at the pin. This will normalize the default aim diretion to the animated pose.
				aim.solver.transform.LookAt(pin.position);

				// Set aim target
				aim.solver.IKPosition = target.position;
				
				// Setting aim weight
				aim.solver.IKPositionWeight = aimWeight.Evaluate(hitWeight) * weight;
				
				// Updating the AimIK solver
				aim.solver.Update();
			}

			// Updating the FullBodyBipedIK solver
			ik.solver.Update();
		}
		
	}
}
