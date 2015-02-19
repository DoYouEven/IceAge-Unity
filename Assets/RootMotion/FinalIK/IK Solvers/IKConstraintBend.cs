using UnityEngine;
using System.Collections;

	namespace RootMotion.FinalIK {

	/// <summary>
	/// %Constraint used for fixing bend direction of 3-segment node chains in a node based %IK solver. 
	/// </summary>
	[System.Serializable]
	public class IKConstraintBend {
		
		#region Main Interface

		/// <summary>
		/// The first bone.
		/// </summary>
		public Transform bone1;
		/// <summary>
		/// The second (bend) bone.
		/// </summary>
		public Transform bone2;
		/// <summary>
		/// The third bone.
		/// </summary>
		public Transform bone3;
		/// <summary>
		/// The bend goal Transform.
		/// </summary>
		public Transform bendGoal;
		
		/// <summary>
		/// The bend direction.
		/// </summary>
		public Vector3 direction = Vector3.right;

		/// <summary>
		/// The bend rotation offset.
		/// </summary>
		public Quaternion rotationOffset;
		
		/// <summary>
		/// The weight. If weight is 1, will override effector rotation and the joint will be rotated at the direction. This enables for direct manipulation of the bend direction independent of effector rotation.
		/// </summary>
		[Range(0f, 1f)]
		public float weight = 0f;
		
		/// <summary>
		/// Determines whether this IKConstraintBend is valid.
		/// </summary>
		public bool IsValid(IKSolverFullBody solver, Warning.Logger logger) {
			if (bone1 == null || bone2 == null || bone3 == null) {
				if (logger != null) logger("Bend Constraint contains a null reference.");
				return false;
			}
			if (solver.GetPoint(bone1) == null) {
				if (logger != null) logger("Bend Constraint is referencing to a bone '" + bone1.name + "' that does not excist in the Node Chain.");
				return false;
			}
			if (solver.GetPoint(bone2) == null) {
				if (logger != null) logger("Bend Constraint is referencing to a bone '" + bone2.name + "' that does not excist in the Node Chain.");
				return false;
			}
			if (solver.GetPoint(bone3) == null) {
				if (logger != null) logger("Bend Constraint is referencing to a bone '" + bone3.name + "' that does not excist in the Node Chain.");
				return false;
			}
			return true;
		}
		
		#endregion Main Interface

		public Vector3 defaultLocalDirection, defaultChildDirection;
		[System.NonSerializedAttribute] public float clampF = 0.505f;

		private IKSolver.Node node1, node2, node3;
		public bool initiated { get; private set; }

		public IKConstraintBend() {}
		
		public IKConstraintBend(Transform bone1, Transform bone2, Transform bone3) {
			SetBones(bone1, bone2, bone3);
		}
		
		public void SetBones(Transform bone1, Transform bone2, Transform bone3) {
			this.bone1 = bone1;
			this.bone2 = bone2;
			this.bone3 = bone3;
		}
		
		/*
		 * Initiate the constraint and set defaults
		 * */
		public void Initiate(IKSolverFullBody solver) {
			node1 = solver.GetPoint(bone1) as IKSolver.Node;
			node2 = solver.GetPoint(bone2) as IKSolver.Node;
			node3 = solver.GetPoint(bone3) as IKSolver.Node;
		
			// Find the default bend direction orthogonal to the chain direction
			direction = OrthoToBone1(OrthoToLimb(node2.transform.position - node1.transform.position));

			// Default bend direction relative to the first node
			defaultLocalDirection = Quaternion.Inverse(node1.transform.rotation) * direction;

			// Default plane normal
			Vector3 defaultNormal = Vector3.Cross((node3.transform.position - node1.transform.position).normalized, direction);
			
			// Default plane normal relative to the third node
			defaultChildDirection = Quaternion.Inverse(node3.transform.rotation) * defaultNormal;

			initiated = true;
		}

		/*
		 * Make the limb bend towards the specified local directions of the bones
		 * */
		public void SetLimbOrientation(Vector3 upper, Vector3 lower, Vector3 last) {
			if (upper == Vector3.zero) Debug.LogError("Attempting to set limb orientation to Vector3.zero axis");
			if (lower == Vector3.zero) Debug.LogError("Attempting to set limb orientation to Vector3.zero axis");
			if (last == Vector3.zero) Debug.LogError("Attempting to set limb orientation to Vector3.zero axis");
			
			// Default bend direction relative to the first node
			defaultLocalDirection = upper.normalized;
			defaultChildDirection = last.normalized;
		}

		/*
		 * Limits the bending joint of the limb to 90 degrees from the default 90 degrees of bend direction
		 * */
		public void LimitBend(float solverWeight, float positionWeight) {
			if (!initiated) return;

			Vector3 normalDirection = bone1.rotation * -defaultLocalDirection;
			
			Vector3 axis2 = bone3.position - bone2.position;

			// Clamp the direction from knee/elbow to foot/hand to valid range (90 degrees from right-angledly bent limb)
			bool changed = false;
			Vector3 clampedAxis2 = V3Tools.ClampDirection(axis2, normalDirection, clampF * solverWeight, 0, out changed);

			Quaternion bone3Rotation = bone3.rotation;

			if (changed) {
				Quaternion f = Quaternion.FromToRotation(axis2, clampedAxis2); 
				bone2.rotation = f * bone2.rotation;
			}

			// Rotating bend direction to normal when the limb is stretched out
			if (positionWeight > 0f) {
				Vector3 normal = bone2.position - bone1.position;
				Vector3 tangent = bone3.position - bone2.position;

				Vector3.OrthoNormalize(ref normal, ref tangent);
				Quaternion q = Quaternion.FromToRotation(tangent, normalDirection);

				bone2.rotation = Quaternion.Lerp(bone2.rotation, q * bone2.rotation, positionWeight * solverWeight);
			}

			if (changed || positionWeight > 0f) bone3.rotation = bone3Rotation;
		}

		/*
		 * Computes the direction from the first node to the second node
		 * */
		public Vector3 GetDir() {
			if (!initiated) return Vector3.zero;

			// Apply the bend goal
			if (bendGoal != null) {
				Vector3 b = bendGoal.position - node1.solverPosition;
				if (b != Vector3.zero) direction = b;
			}

			if (weight >= 1f) return direction.normalized;

			Vector3 solverDirection = node3.solverPosition - node1.solverPosition;

			// Get rotation from animated limb direction to solver limb direction
			Quaternion f = Quaternion.FromToRotation(node3.transform.position - node1.transform.position, solverDirection);

			// Rotate the default bend direction by f
			Vector3 dir = f * (node2.transform.position - node1.transform.position);

			// Effector rotation
			if (node3.effectorRotationWeight > 0f) {
				// Bend direction according to the effector rotation
				Vector3 effectorDirection = -Vector3.Cross(solverDirection, node3.solverRotation * defaultChildDirection);
				dir = Vector3.Lerp(dir, effectorDirection, node3.effectorRotationWeight);
			}

			// Rotation Offset
			if (rotationOffset != Quaternion.identity) {
				Quaternion toOrtho = Quaternion.FromToRotation(rotationOffset * solverDirection, solverDirection);
				dir = toOrtho * rotationOffset * dir;
			}

			if (weight <= 0f) return dir;
			return Vector3.Lerp(dir, direction.normalized, weight);
		}

		/*
		 * Ortho-Normalize a vector to the chain direction
		 * */
		private Vector3 OrthoToLimb(Vector3 tangent) {
			Vector3 normal = node3.solverPosition - node1.solverPosition;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			return tangent;
		}

		/*
		 * Ortho-Normalize a vector to the first bone direction
		 * */
		private Vector3 OrthoToBone1(Vector3 tangent) {
			Vector3 normal = node2.solverPosition - node1.solverPosition;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			return tangent;
		}
	}
}
