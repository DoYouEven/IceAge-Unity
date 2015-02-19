using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {
	
	/// <summary>
	/// The default Character Controller for characters using the Legacy animation system.
	/// </summary>
	public class CharacterControllerLegacy : CharacterControllerDefault {
		
		public AnimationCurve animationSpeedRelativeToVelocity;
		
		protected override void Update() {
			base.Update();

			// Animation
			animation.CrossFade(state.clipName, 0.3f, PlayMode.StopSameLayer);
			animation[state.clipName].speed = animationSpeedRelativeToVelocity.Evaluate(moveVector.magnitude) * state.animationSpeed;
		}
	}
}

