using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {
	
	/// <summary>
	/// Contols animation for a third person person controller.
	/// </summary>
	public class CharacterAnimationThirdPerson: CharacterAnimationBase {

		public virtual void UpdateState(CharacterThirdPerson.AnimState _state) {}

		public override Vector3 GetPivotPoint() {
			return transform.position;
		}
	}
}
