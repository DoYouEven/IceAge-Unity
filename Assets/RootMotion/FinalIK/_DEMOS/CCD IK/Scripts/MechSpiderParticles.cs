using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Emitting smoke for the mech spider
	/// </summary>
	public class MechSpiderParticles: MonoBehaviour {
		
		public MechSpiderController mechSpiderController;
		
		private ParticleSystem particles;
		
		void Start() {
			particles = (ParticleSystem)GetComponent(typeof(ParticleSystem));
		}
		
		void Update() {
			// Smoke
			float inputMag = mechSpiderController.inputVector.magnitude;
			particles.emissionRate = Mathf.Clamp(inputMag * 50, 30, 50);
			particles.startColor = new Color (particles.startColor.r, particles.startColor.g, particles.startColor.b, Mathf.Clamp(inputMag, 0.4f, 1f));
		}
	}
}
