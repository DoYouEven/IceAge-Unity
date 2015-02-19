using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK.Demos {
	
	/// <summary>
	/// Character controller for the simple AimIK aiming system demo.
	/// </summary>
	public class CharacterControllerSimpleAim : MonoBehaviour {

		public SimpleAimingSystem aimingSystem;
		public Transform target;

		void LateUpdate () {
			// Update aiming system target position
			aimingSystem.targetPosition = target.position;
		}
	}
}
