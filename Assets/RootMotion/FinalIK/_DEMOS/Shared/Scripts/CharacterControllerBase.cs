using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// The base abstract class for all Character controllers.
	/// </summary>
	public abstract class CharacterControllerBase : MonoBehaviour {

		// Reads the Input to get the movement direction.
		protected Vector3 GetInputDirection() {
			Vector3 d = new Vector3(
				Input.GetAxis("Horizontal"),
				0f,
				Input.GetAxis("Vertical")
				);
			
			d.z += Mathf.Abs(d.x) * 0.05f;
			d.x -= Mathf.Abs(d.z) * 0.05f;
			
			return d.normalized;
		}
	}
}
