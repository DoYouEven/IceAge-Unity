using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// The default Character Controller.
	/// </summary>
	public class CharacterControllerDefault : CharacterControllerBase {

		/// <summary>
		/// The state of the character (idle, walking, running... etc)
		/// </summary>
		[System.Serializable]
		public class State {
			public string clipName;
			public float animationSpeed = 1f, moveSpeed = 1f;
		}

		/// <summary>
		/// The character rotation mode
		/// </summary>
		[System.Serializable]
		public enum RotationMode {
			Slerp,
			RotateTowards
		}

		public CameraController cam; // The camera controller
		public State[] states; // The array of States
		public int idleStateIndex = 0, walkStateIndex = 1, runStateIndex = 2; // Which state is for idle, which is for walk and run
		public float acceleration = 5f; // The acceleration of the character
		public float speedAcceleration = 3f; // The acceleration of the speed of the character
		public float angularSpeed = 7f; // The speed of the character rotation
		public RotationMode rotationMode; // Character rotation mode

		protected State state; // The current state of the character
		protected Vector3 moveVector; // The movement vector of the character
		protected float speed; // The current speed of the character (interpolating this between the states)

		protected virtual float accelerationMlp { get { return 1f; }} // The acceleration multiplier, meant for being overrided by extended classes

		protected virtual void Update() {
			// Update the state
			if (GetInputDirection() != Vector3.zero) {
				state = Input.GetKey(KeyCode.LeftShift)? states[runStateIndex]: states[walkStateIndex];
			} else {
				state = states[idleStateIndex];
			}

			// Updating the rotation of the character
			Vector3 targetDirection = Quaternion.LookRotation(new Vector3(cam.transform.forward.x, 0f, cam.transform.forward.z)) * GetInputDirection();
			if (targetDirection != Vector3.zero) {
				Vector3 targetForward = Quaternion.FromToRotation(transform.forward, targetDirection) * transform.forward;
				
				Vector3 forward = transform.forward;
				
				switch(rotationMode) {
				case RotationMode.Slerp:
					forward = Vector3.Slerp(forward, targetForward, Time.deltaTime * angularSpeed * accelerationMlp);
					break;
				case RotationMode.RotateTowards:
					forward = Vector3.RotateTowards(forward, targetForward, Time.deltaTime * angularSpeed * accelerationMlp, 1f);
					break;
				}
				
				forward.y = 0f;
				
				transform.rotation = Quaternion.LookRotation(forward);
			}

			// Updating the position of the character
			moveVector = Vector3.Lerp(moveVector, targetDirection, Time.deltaTime * acceleration * accelerationMlp);
			
			speed = Mathf.Lerp(speed, state.moveSpeed, Time.deltaTime * speedAcceleration * accelerationMlp);
			
			if (rigidbody != null) rigidbody.position += moveVector * Time.deltaTime * speed;
			else transform.position += moveVector * Time.deltaTime * speed;
		}                                                
	}
}
