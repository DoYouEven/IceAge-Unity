using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Maps a bone or a collection of bones to a node based %IK solver
	/// </summary>
	[System.Serializable]
	public class IKMapping {
		
		#region Main Interface
		
		/// <summary>
		/// Contains mapping information of a single bone
		/// </summary>
		[System.Serializable]
		public class BoneMap {
			/// <summary>
			/// The transform.
			/// </summary>
			public Transform transform;
			/// <summary>
			/// The node in %IK Solver.
			/// </summary>
			public IKSolver.Node node;

			public Vector3 defaultLocalPosition;
			public Quaternion defaultLocalRotation;
			public Vector3 localSwingAxis, localTwistAxis, planePosition, ikPosition;
			public Quaternion defaultLocalTargetRotation;
			private Quaternion maintainRotation;
			public float length;
			public Quaternion animatedRotation;
			
			private IKSolver.Node planeNode1, planeNode2, planeNode3;

			public void Initiate(Transform transform, IKSolver solver) {
				this.transform = transform;

				IKSolver.Point point = solver.GetPoint(transform);
				if (point != null) this.node = point as IKSolver.Node;
			}

			/// <summary>
			/// Gets the current swing direction of the bone in world space.
			/// </summary>
			public Vector3 swingDirection {
				get {
					return transform.rotation * localSwingAxis;
				}
			}

			public void StoreDefaultLocalState() {
				defaultLocalPosition = transform.localPosition;
				defaultLocalRotation = transform.localRotation;
			}
			
			public void FixTransform(bool position) {
				if (position) transform.localPosition = defaultLocalPosition;
				transform.localRotation = defaultLocalRotation;
			}
			
			#region Reading
			
			/*
			 * Does this bone have a node in the IK Solver?
			 * */
			public bool isNodeBone {
				get {
					return node != null;
				}
			}
			
			/*
			 * Calculate length of the bone
			 * */
			public void SetLength(BoneMap nextBone) {
				length = Vector3.Distance(transform.position, nextBone.transform.position);
			}
			
			/*
			 * Sets the direction to the swing target in local space
			 * */
			public void SetLocalSwingAxis(BoneMap swingTarget) {
				SetLocalSwingAxis(swingTarget, this);
			}
			
			/*
			 * Sets the direction to the swing target in local space
			 * */
			public void SetLocalSwingAxis(BoneMap bone1, BoneMap bone2) {
				localSwingAxis = Quaternion.Inverse(transform.rotation) * (bone1.transform.position - bone2.transform.position);
			}
			
			/*
			 * Sets the direction to the twist target in local space
			 * */
			public void SetLocalTwistAxis(Vector3 twistDirection, Vector3 normalDirection) {
				Vector3.OrthoNormalize(ref normalDirection, ref twistDirection);
				localTwistAxis = Quaternion.Inverse(transform.rotation) * twistDirection;
			}

			/*
			 * Sets the 3 points defining a plane for this bone
			 * */
			public void SetPlane(IKSolver.Node planeNode1, IKSolver.Node planeNode2, IKSolver.Node planeNode3) {
				this.planeNode1 = planeNode1;
				this.planeNode2 = planeNode2;
				this.planeNode3 = planeNode3;
				
				UpdatePlane(true, true);
			}
			
			/*
			 * Updates the 3 plane points
			 * */
			public void UpdatePlane(bool rotation, bool position) {
				Quaternion t = lastAnimatedTargetRotation;

				if (rotation) defaultLocalTargetRotation = QuaTools.RotationToLocalSpace(transform.rotation, t);
				if (position) planePosition = Quaternion.Inverse(t) * (transform.position - planeNode1.transform.position);
			}
			
			/*
			 * Sets the virtual position for this bone
			 * */
			public void SetIKPosition() {
				ikPosition = transform.position;
			}

			/*
			 * Stores the current rotation for later use.
			 * */
			public void MaintainRotation() {
				maintainRotation = transform.rotation;
			}
			
			#endregion Reading
			
			#region Writing
			
			/*
			 * Moves the bone to its virtual position
			 * */
			public void SetToIKPosition() {
				transform.position = ikPosition;
			}
			
			/*
			 * Moves the bone to the solver position of it's node
			 * */
			public void FixToNode(float weight, IKSolver.Node fixNode = null) {
				if (fixNode == null) fixNode = node;

				if (weight >= 1f) {
					transform.position = fixNode.solverPosition;
					return;
				}

				transform.position = Vector3.Lerp(transform.position, fixNode.solverPosition, weight);
			}
			
			/*
			 * Gets the bone's position relative to it's 3 plane nodes
			 * */
			public Vector3 GetPlanePosition() {
				return planeNode1.solverPosition + (targetRotation * planePosition);
			}
			
			/*
			 * Positions the bone relative to it's 3 plane nodes
			 * */
			public void PositionToPlane() {
				transform.position = GetPlanePosition();
			}
			
			/*
			 * Rotates the bone relative to it's 3 plane nodes
			 * */
			public void RotateToPlane(float weight) {
				Quaternion r = targetRotation * defaultLocalTargetRotation;

				if (weight >= 1f) {
					transform.rotation = r;
					return;
				}

				transform.rotation = Quaternion.Lerp(transform.rotation, r, weight);
			}

			/*
			 * Swings to the swing target
			 * */
			public void Swing(Vector3 swingTarget, float weight) {
				Swing(swingTarget, transform.position, weight);
			}
			
			/*
			 * Swings to a direction from pos2 to pos1
			 * */
			public void Swing(Vector3 pos1, Vector3 pos2, float weight) {
				Quaternion r = Quaternion.FromToRotation(transform.rotation * localSwingAxis, pos1 - pos2) * transform.rotation;

				if (weight >= 1f) {
					transform.rotation = r;
					return;
				}

				transform.rotation = Quaternion.Lerp(transform.rotation, r, weight);
			}
			
			/*
			 * Twists to the twist target
			 * */
			public void Twist(Vector3 twistDirection, Vector3 normalDirection, float weight) {
				Vector3.OrthoNormalize(ref normalDirection, ref twistDirection);

				Quaternion r = Quaternion.FromToRotation(transform.rotation * localTwistAxis, twistDirection) * transform.rotation;

				if (weight >= 1f) {
					transform.rotation = r;
					return;
				}

				transform.rotation = Quaternion.Lerp(transform.rotation, r, weight);
			}

			/*
			 * Rotates back to the last animated local rotation
			 * */
			public void RotateToMaintain(float weight) {
				if (weight <= 0f) return;

				transform.rotation = Quaternion.Lerp(transform.rotation, maintainRotation, weight);
			}
			
			/*
			 * Rotates to match the effector rotation
			 * */
			public void RotateToEffector(float weight) {
				if (!isNodeBone) return;
				float w = weight * node.effectorRotationWeight;
				if (w <= 0f) return;

				if (w >= 1f) {
					transform.rotation = node.solverRotation;
					return;
				}

				transform.rotation = Quaternion.Lerp(transform.rotation, node.solverRotation, w);
			}
			
			#endregion Writing
			
			/*
			 * Rotation of plane nodes in the solver
			 * */
			private Quaternion targetRotation {
				get {
					if (planeNode1.solverPosition == planeNode3.solverPosition) return Quaternion.identity;
					return Quaternion.LookRotation(planeNode2.solverPosition - planeNode1.solverPosition, planeNode3.solverPosition - planeNode1.solverPosition);
				}
			}
			
			/*
			 * Rotation of plane nodes in the animation
			 * */
			private Quaternion lastAnimatedTargetRotation {
				get {
					if (planeNode1.transform.position == planeNode3.transform.position) return Quaternion.identity;
					return Quaternion.LookRotation(planeNode2.transform.position - planeNode1.transform.position, planeNode3.transform.position - planeNode1.transform.position);
				}
			}
		}
		
		/// <summary>
		/// Determines whether this IKMapping is valid.
		/// </summary>
		public virtual bool IsValid(IKSolver solver, Warning.Logger logger = null) {
			return true;
		}

		#endregion Main Interface
		
		protected IKSolver solver;
		protected virtual void OnInitiate() {}

		public void Initiate(IKSolver solver) {
			this.solver = solver;
			
			OnInitiate();
		}
		
		protected bool BoneIsValid(Transform bone, IKSolver solver, Warning.Logger logger = null) {
			if (bone == null) {
				if (logger != null) logger("IKMappingLimb contains a null reference.");
				return false;
			}
			if (solver.GetPoint(bone) == null) {
				if (logger != null) logger("IKMappingLimb is referencing to a bone '" + bone.name + "' that does not excist in the Node Chain.");
				return false;
			}
			return true;
		}
	}
}
