using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Base class for all FBBIK effector positionOffset modifiers. Works with animatePhysics, safe delegates, offset limits.
	/// </summary>
	public abstract class OffsetModifier: MonoBehaviour {

		/// <summary>
		/// Limiting effector position offsets
		/// </summary>
		[System.Serializable]
		public class OffsetLimits {
			
			public FullBodyBipedEffector effector; // The effector type (this is just an enum)
			public float spring = 0f; // Spring force, if zero then this is a hard limit, if not, offset can exceed the limit.
			public bool x, y, z; // Which axes to limit the offset on?
			public float minX, maxX, minY, maxY, minZ, maxZ; // The limits
			
			// Apply the limit to the effector
			public void Apply(IKEffector e, Quaternion rootRotation) {
				Vector3 offset = Quaternion.Inverse(rootRotation) * e.positionOffset;
				
				if (spring <= 0f) {
					// Hard limits
					if (x) offset.x = Mathf.Clamp(offset.x, minX, maxX);
					if (y) offset.y = Mathf.Clamp(offset.y, minY, maxY);
					if (z) offset.z = Mathf.Clamp(offset.z, minZ, maxZ);
				} else {
					// Soft limits
					if (x) offset.x = SpringAxis(offset.x, minX, maxX);
					if (y) offset.y = SpringAxis(offset.y, minY, maxY);
					if (z) offset.z = SpringAxis(offset.z, minZ, maxZ);
				}
				
				// Apply to the effector
				e.positionOffset = rootRotation * offset;
			}
			
			// Just math for limiting floats
			private float SpringAxis(float value, float min, float max) {
				if (value > min && value < max) return value;
				if (value < min) return Spring(value, min, true);
				return Spring(value, max, false);
			}
			
			// Spring math
			private float Spring(float value, float limit, bool negative) {
				float illegal = value - limit;
				float s = illegal * spring;
				
				if (negative) return value + Mathf.Clamp(-s, 0, -illegal);
				return value - Mathf.Clamp(s, 0, illegal);
			}
		}

		public float weight = 1f; // The master weight

		protected abstract void OnModifyOffset();

		[SerializeField] protected FullBodyBipedIK ik; // Reference to the FBBIK component
		// not using Time.deltaTime or Time.fixedDeltaTime here, because we don't know if animatePhysics is true or not on the character, so we have to keep track of time ourselves.
		protected float deltaTime { get { return Time.time - lastTime; }}

		private float lastTime;

		protected virtual void Start() {
			StartCoroutine(Initiate());
		}
		
		private IEnumerator Initiate() {
			while (ik == null) yield return null;

			// You can use just LateUpdate, but note that it doesn't work when you have animatePhysics turned on for the character.
			ik.solver.OnPreUpdate += ModifyOffset;
			lastTime = Time.time;
		}

		// The main function that checks for all conditions and calls OnModifyOffset if they are met
		private void ModifyOffset() {
			if (!enabled) return;
			if (weight <= 0f) return;
			if (deltaTime <= 0f) return;
			if (ik == null) return;
			weight = Mathf.Clamp(weight, 0f, 1f);

			OnModifyOffset();

			lastTime = Time.time;
		}

		protected void ApplyLimits(OffsetLimits[] limits) {
			// Apply the OffsetLimits
			foreach (OffsetLimits limit in limits) {
				limit.Apply(ik.solver.GetEffector(limit.effector), transform.rotation);
			}
		}

		// Remove the delegate when destroyed
		void OnDestroy() {
			if (ik != null) ik.solver.OnPreUpdate -= ModifyOffset;
		}
	}

}
