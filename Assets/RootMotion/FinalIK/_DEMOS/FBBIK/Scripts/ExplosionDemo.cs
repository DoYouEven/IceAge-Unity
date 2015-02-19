using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Demo of exploding a viking using FBBIK
	/// </summary>
	public class ExplosionDemo : MonoBehaviour {

		public FullBodyBipedIK ik; // Reference to the FBBIK component
		public new Rigidbody rigidbody; // Rigidbody of the Viking
		public float forceMlp = 1f; // Explosion force
		public float upForce = 1f; // Explosion up forve
		public float weightFalloffSpeed = 1f; // The speed of explosion falloff
		public AnimationCurve weightFalloff; // Explosion weight falloff
		public AnimationCurve explosionForceByDistance; // The force of the explosion relative to character distance to the bomb
		public AnimationCurve scale; // Scaling the bomb GameObject with the explosion

		private float weight = 0f;
		private Vector3 defaultScale = Vector3.one;

		void Start() {
			// Storing the default scale of the bomb
			defaultScale = transform.localScale;
		}
		
		// Update is called once per frame
		void Update () {
			weight = Mathf.Clamp(weight - Time.deltaTime * weightFalloffSpeed, 0f, 1f);

			// Exploding the bomb
			if (Input.GetKeyDown(KeyCode.E)) {
				// Set FBBIK weight to 1
				ik.solver.IKPositionWeight = 1f;

				// Set limb effector positions to where they are at the momemt
				ik.solver.leftHandEffector.position = ik.solver.leftHandEffector.bone.position;
				ik.solver.rightHandEffector.position = ik.solver.rightHandEffector.bone.position;
				ik.solver.leftFootEffector.position = ik.solver.leftFootEffector.bone.position;
				ik.solver.rightFootEffector.position = ik.solver.rightFootEffector.bone.position;

				weight = 1f;

				// Add explosion force to the character rigidbody
				Vector3 direction = rigidbody.position - transform.position;
				float explosionForce = explosionForceByDistance.Evaluate(direction.magnitude);
				rigidbody.AddForce((direction.normalized + (Vector3.up * upForce)) * explosionForce * forceMlp, ForceMode.VelocityChange);
			}

			// Set effector weights
			SetEffectorWeights(weightFalloff.Evaluate(weight));

			// Set bomb scale
			transform.localScale = scale.Evaluate(weight) * defaultScale;
		}

		// Set FBBIK limb end-effector weights to value
		private void SetEffectorWeights(float w) {
			ik.solver.leftHandEffector.positionWeight = w;
			ik.solver.rightHandEffector.positionWeight = w;
			ik.solver.leftFootEffector.positionWeight = w;
			ik.solver.rightFootEffector.positionWeight = w;
		}
	}
}
