using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Simple character animation controller for the Hit Reaction demo
	/// </summary>
	public class HitReactionCharacter: MonoBehaviour {

		#region Shooting

		void Update() {
			// On left mouse button...
			if (Input.GetMouseButtonDown(0)) {
				Ray ray = cam.ScreenPointToRay(Input.mousePosition);

				// Raycast to find a ragdoll collider
				RaycastHit hit = new RaycastHit();
				if (Physics.Raycast(ray, out hit, 100f)) {

					// Use the HitReaction
					hitReaction.Hit(hit.collider, ray.direction * hitForce, hit.point);

					// Just for GUI
					colliderName = hit.collider.name;
				}
			}
		}

		private string colliderName;

		void OnGUI() {
			GUILayout.Label("LMB to shoot the Dummy, RMB to rotate the camera.");
			if (colliderName != string.Empty) GUILayout.Label("Last Bone Hit: " + colliderName);
		}

		#endregion Shooting

		[SerializeField] string mixingAnim;
		[SerializeField] Transform recursiveMixingTransform;
		[SerializeField] Camera cam;
		[SerializeField] HitReaction hitReaction;
		[SerializeField] float hitForce = 1f;

		// This is just for setting up Legacy upperbody animation layer
		void Start() {
			if (mixingAnim != string.Empty) {
				animation[mixingAnim].layer = 1;
				animation[mixingAnim].AddMixingTransform(recursiveMixingTransform, true);
				animation.Play(mixingAnim);
			}
		}
	}
}
