using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Demo controller for the Full Body FPS scene.
	/// </summary>
	public class FPSCharacter: MonoBehaviour {

		[SerializeField] AnimationClip aimAnim; // The aiming animation
		[SerializeField] Transform mixingTransformRecursive; // The recursive mixing Transform for the aiming animation
		[SerializeField] FPSAiming FPSAiming; // The aiming IK controller

		private float sVel;

		void Start() {
			// Making the character aim while walking
			animation[aimAnim.name].AddMixingTransform(mixingTransformRecursive);
			animation[aimAnim.name].layer = 1;
			animation.Play(aimAnim.name);
		}

		void Update() {
			// Aiming down the sight of the gun when RMB is down
			FPSAiming.sightWeight = Mathf.SmoothDamp(FPSAiming.sightWeight, (Input.GetMouseButton(1)? 1f: 0f), ref sVel, 0.1f);

			// Set to full values to optimize IK
			if (FPSAiming.sightWeight < 0.001f) FPSAiming.sightWeight = 0f;
			if (FPSAiming.sightWeight > 0.999f) FPSAiming.sightWeight = 1f;
		}

		void OnGUI() {
			GUI.Label(new Rect(Screen.width - 210, 10, 200, 25), "Hold RMB to aim down the sight");
		}

	}
}
