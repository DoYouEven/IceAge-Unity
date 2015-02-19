using UnityEngine;
using System.Collections;
using RootMotion;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Controls LookAtIK for the InteractionSystem
	/// </summary>
	[System.Serializable]
	public class InteractionLookAt {

		// Reference to the LookAtIK component
		public LookAtIK ik;
		/// <summary>
		/// Interpolation speed of the LookAtIK target
		/// </summary>
		public float lerpSpeed = 5f;
		/// <summary>
		/// Interpolation speed of the LookAtIK weight
		/// </summary>
		public float weightSpeed = 1f;

		/// <summary>
		/// Look the specified target for the specified time.
		/// </summary>
		public void Look(Transform target, float time) {
			if (ik == null) return;

			if (ik.solver.IKPositionWeight <= 0f) ik.solver.IKPosition = ik.solver.GetRoot().position + ik.solver.GetRoot().forward * 3f;
			lookAtTarget = target;
			stopLookTime = time;
		}

		private Transform lookAtTarget; // The target Transform to look at
		private float stopLookTime; // Time to start fading out the LookAtIK
		private float weight; // Current weight
		private bool firstFBBIKSolve; // Has the FBBIK already solved for this frame? In case it is solved more than once, for example when using the ShoulderRotator

		public void Update() {
			if (ik == null) return;
			if (ik.enabled) ik.Disable();

			if (lookAtTarget == null) return;

			// Interpolate the weight
			float add = Time.time < stopLookTime? weightSpeed: -weightSpeed;
			weight = Mathf.Clamp(weight + add * Time.deltaTime, 0f, 1f);

			// Set LookAtIK weight
			ik.solver.IKPositionWeight = Interp.Float(weight, InterpolationMode.InOutQuintic);

			// Set LookAtIK position
			ik.solver.IKPosition = Vector3.Lerp(ik.solver.IKPosition, lookAtTarget.position, lerpSpeed * Time.deltaTime);

			// Release the LookAtIK for other tasks once we're weighed out
			if (weight <= 0f) lookAtTarget = null;

			firstFBBIKSolve = true;
		}
	
		public void SolveSpine() {
			if (ik == null) return;
			if (!firstFBBIKSolve) return;

			float headWeight = ik.solver.headWeight;
			float eyesWeight = ik.solver.eyesWeight;

			ik.solver.headWeight = 0f;
			ik.solver.eyesWeight = 0f;
			ik.solver.Update();
			ik.solver.headWeight = headWeight;
			ik.solver.eyesWeight = eyesWeight;
		}

		public void SolveHead() {
			if (ik == null) return;
			if (!firstFBBIKSolve) return;

			float bodyWeight = ik.solver.bodyWeight;

			ik.solver.bodyWeight = 0f;
			ik.solver.Update();
			ik.solver.bodyWeight = bodyWeight;

			firstFBBIKSolve = false;
		}
	}
}