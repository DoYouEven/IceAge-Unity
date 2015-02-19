using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// User input for a third person character controller.
	/// </summary>
	[RequireComponent(typeof(CharacterBase))]
	public class UserControlThirdPerson : MonoBehaviour {

		// Input state
		public struct State {
			public Vector3 move;
			public Vector3 lookPos;
			public bool crouch;
			public bool jump;
		}

		[SerializeField] bool walkByDefault;                  // toggle for walking state
		[SerializeField] bool canCrouch = true;
		[SerializeField] bool canJump = true;

		protected Transform cam;                              // A reference to the main camera in the scenes transform
		protected CharacterThirdPerson character;             	// A reference to the character
		protected State state = new State();					// The current state of the user input
		
		// Use this for initialization
		void Start () {
			// get the transform of the main camera
			cam = Camera.main.transform;
			
			// get the third person character ( this should never be null due to require component )
			character = GetComponent<CharacterThirdPerson>();
		}

		// Fixed update is called in sync with physics
		protected virtual void FixedUpdate () {
			// read inputs
			state.crouch = canCrouch && Input.GetKey(KeyCode.C);
			state.jump = canJump && Input.GetButton("Jump");

			float h = Input.GetAxis("Horizontal");
			float v = Input.GetAxis("Vertical");
			
			// calculate move direction to pass to character
			Vector3 camForward = Vector3.Scale (cam.forward, new Vector3(1,0,1)).normalized;
			state.move = (v * camForward + h * cam.right).normalized;	

			bool walkToggle = Input.GetKey(KeyCode.LeftShift);

			// We select appropriate speed based on whether we're walking by default, and whether the walk/run toggle button is pressed:
			float walkMultiplier = (walkByDefault ? walkToggle ? 1 : 0.5f : walkToggle ? 0.5f : 1);

			state.move *= walkMultiplier;
			
			// calculate the head look target position
			state.lookPos = transform.position + cam.forward * 100f;

			character.UpdateState(state);
		}
	}
}

