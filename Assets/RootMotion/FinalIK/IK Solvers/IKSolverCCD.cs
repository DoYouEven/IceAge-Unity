using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.FinalIK {

	/// <summary>
	/// CCD (Cyclic Coordinate Descent) constrainable heuristic inverse kinematics algorithm.
	/// </summary>
	[System.Serializable]
	public class IKSolverCCD : IKSolverHeuristic {
		
		#region Main Interface

		/// <summary>
		/// CCD tends to overemphasise the rotations of the bones closer to the target position. Reducing bone weight down the hierarchy will compensate for this effect.
		/// </summary>
		public void FadeOutBoneWeights() {
			if (bones.Length < 2) return;
			
			bones[0].weight = 1f;
			float step = 1f / (bones.Length - 1);
			
			for (int i = 1; i < bones.Length; i++) {
				bones[i].weight = step * (bones.Length - 1 - i);
			}
		}

		/// <summary>
		/// Called before each iteration of the solver.
		/// </summary>
		public IterationDelegate OnPreIteration;
		
		#endregion Main Interface
		
		protected override void OnInitiate() {
			if (firstInitiation || !Application.isPlaying) IKPosition = bones[bones.Length - 1].transform.position;
			
			InitiateBones();
		}
		
		protected override void OnUpdate() {
			if (IKPositionWeight <= 0) return;	
			IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);

			if (target != null) IKPosition = target.position;
			
			Vector3 singularityOffset = maxIterations > 1? GetSingularityOffset(): Vector3.zero;

			// Iterating the solver
			for (int i = 0; i < maxIterations; i++) {
				
				// Optimizations
				if (singularityOffset == Vector3.zero && i >= 1 && tolerance > 0 && positionOffset < tolerance * tolerance) break;
				lastLocalDirection = localDirection;

				if (OnPreIteration != null) OnPreIteration(i);
				
				Solve(IKPosition + (i == 0? singularityOffset: Vector3.zero));
			}
			
			lastLocalDirection = localDirection;
		}
		
		/*
		 * Solve the CCD algorithm
		 * */
		private void Solve(Vector3 targetPosition) {
			for (int i = bones.Length - 2; i > -1; i--) {
				Vector3 toLastBone = bones[bones.Length - 1].transform.position - bones[i].transform.position;
				Vector3 toTarget = targetPosition - bones[i].transform.position;
				
				// Get the rotation to direct the last bone to the target
				Quaternion targetRotation = Quaternion.FromToRotation(toLastBone, toTarget) * bones[i].transform.rotation;

				// Slerp if weight is < 0
				//CCD tends to overemphasise the rotations of the bones closer to the target position. Reducing bone weight down the hierarchy will compensate for this effect.
				float w = bones[i].weight * IKPositionWeight;

				if (w >= 1) bones[i].transform.rotation = targetRotation;
				else bones[i].transform.rotation = Quaternion.Lerp(bones[i].transform.rotation, targetRotation, w);
					
				// Rotation Constraints
				if (useRotationLimits && bones[i].rotationLimit != null) bones[i].rotationLimit.Apply();
			}
		}
	}
}
