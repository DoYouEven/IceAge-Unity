using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Motion Absorb demo character controller.
	/// </summary>
	public class MotionAbsorbCharacter : MonoBehaviour {

		public Animator animator;
		public MotionAbsorb motionAbsorb;
		public Transform cube; // The cube we are hitting
		public float cubeRandomPosition = 0.1f; // Randomizing cube position after each hit
		public AnimationCurve motionAbsorbWeight;

		private Vector3 cubeDefaultPosition;
		private AnimatorStateInfo info;
		
		void Start() {
			// Storing the default position of the cube
			cubeDefaultPosition = cube.position;
		}

		void Update () {
			// Set motion absorb weight
			//motionAbsorb.weight = animator.GetFloat("MotionAbsorbWeight"); // NB! Using Mecanim curves is PRO only

			// Using an animation curve so it works with Unity Free as well
			info = animator.GetCurrentAnimatorStateInfo(0);
			motionAbsorb.weight = motionAbsorbWeight.Evaluate(info.normalizedTime - (int)info.normalizedTime);
		}

		// Mecanim event
		void SwingStart() {
			// Reset the cube
			cube.rigidbody.MovePosition(cubeDefaultPosition + UnityEngine.Random.insideUnitSphere * cubeRandomPosition);
			cube.rigidbody.MoveRotation(Quaternion.identity);
			cube.rigidbody.velocity = Vector3.zero;
			cube.rigidbody.angularVelocity = Vector3.zero;
		}
	}
}
