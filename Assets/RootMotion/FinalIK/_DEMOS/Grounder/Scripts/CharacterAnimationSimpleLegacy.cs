using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK.Demos {
	
	/// <summary>
	/// Contols animation for a simple Legacy character
	/// </summary>
	public class CharacterAnimationSimpleLegacy: CharacterAnimationThirdPerson {
		
		[SerializeField] new Animation animation; // Reference to the Animation component, in case it's not on this gameobject
		[SerializeField] float pivotOffset; // Offset of the rotating pivot point from the root
		[SerializeField] string idleName; // Name of the idle animation state
		[SerializeField] string moveName; // Name of the movement animation state
		[SerializeField] float idleAnimationSpeed = 0.3f; // The speed of idle animation
		[SerializeField] float moveAnimationSpeed = 0.75f; // The speed of movement animation
		[SerializeField] AnimationCurve moveSpeed; // The moving speed relative to input forward

		protected override void Start() {
			base.Start();

			// animation speeds
			animation[idleName].speed = idleAnimationSpeed;
			animation[moveName].speed = moveAnimationSpeed;
		}
		
		public override Vector3 GetPivotPoint() {
			return transform.position + transform.forward * pivotOffset;
		}
		
		// Update the Animator with the current state of the character controller
		public override void UpdateState(CharacterThirdPerson.AnimState state) {
			if (Time.deltaTime == 0f) return;
			
			// Is the Animator playing the grounded animations?
			animationGrounded = true;
			
			// Crossfading
			if (state.moveDirection.z > 0.4f) animation.CrossFade(moveName, 0.1f);
			else animation.CrossFade(idleName);
			
			// Moving
			character.Move(character.transform.forward * Time.deltaTime * moveSpeed.Evaluate(state.moveDirection.z));
		}
	}
}

