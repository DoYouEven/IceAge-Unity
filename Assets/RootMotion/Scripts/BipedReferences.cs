using UnityEngine;
using System.Collections;
using System;

namespace RootMotion {

	/// <summary>
	/// Contains references to bones common to all biped characters.
	/// </summary>
	[System.Serializable]
	public class BipedReferences {
		
		#region Main Interface
		
		/// <summary>
		/// The root transform is the parent of all the biped's bones and should be located at ground level.
		/// </summary>
		public Transform root;
		/// <summary>
		/// The pelvis (hip) bone.
		/// </summary>
		public Transform pelvis;
		/// <summary>
		/// The first bone of the left leg.
		/// </summary>
		public Transform leftThigh;
		/// <summary>
		/// The second bone of the left leg.
		/// </summary>
		public Transform leftCalf;
		/// <summary>
		/// The third bone of the left leg.
		/// </summary>
		public Transform leftFoot;
		/// <summary>
		/// The first bone of the right leg.
		/// </summary>
		public Transform rightThigh;
		/// <summary>
		/// The second bone of the right leg.
		/// </summary>
		public Transform rightCalf;
		/// <summary>
		/// The third bone of the right leg.
		/// </summary>
		public Transform rightFoot;
		/// <summary>
		/// The first bone of the left arm.
		/// </summary>
		public Transform leftUpperArm;
		/// <summary>
		/// The second bone of the left arm.
		/// </summary>
		public Transform leftForearm;
		/// <summary>
		/// The third bone of the left arm.
		/// </summary>
		public Transform leftHand;
		/// <summary>
		/// The first bone of the right arm.
		/// </summary>
		public Transform rightUpperArm;
		/// <summary>
		/// The second bone of the right arm.
		/// </summary>
		public Transform rightForearm;
		/// <summary>
		/// The third bone of the right arm.
		/// </summary>
		public Transform rightHand;
		/// <summary>
		/// The head.
		/// </summary>
		public Transform head;
		/// <summary>
		/// The spine hierarchy. Should not contain any bone deeper in the hierarchy than the arms (neck or head).
		/// </summary>
		public Transform[] spine = new Transform[0];
		/// <summary>
		/// The eyes.
		/// </summary>
		public Transform[] eyes = new Transform[0];

		/// <summary>
		/// Check for null references.
		/// </summary>
		public bool isValid {
			get {
				if (root == null) return false;
				if (pelvis == null) return false;
				if (leftThigh == null || leftCalf == null || leftFoot == null) return false;
				if (rightThigh == null || rightCalf == null || rightFoot == null) return false;
				if (leftUpperArm == null || leftForearm == null || leftHand == null) return false;
				if (rightUpperArm == null || rightForearm == null || rightHand == null) return false;
					
				foreach (Transform s in spine) if (s == null) return false;
				foreach (Transform eye in eyes) if (eye == null) return false;
				return true;
			}
		}
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="BipedReferences"/> is empty.
		/// </summary>
		public bool isEmpty {
			get {
				return IsEmpty(true);
			}
		}	
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="BipedReferences"/> is empty. If includeRoot is false, returns true(is empty) even if root Transform has been assigned.
		/// </summary>
		public bool IsEmpty(bool includeRoot) {
			if (includeRoot && root != null) return false;
			if (pelvis != null || head != null) return false;
			if (leftThigh != null || leftCalf != null || leftFoot != null) return false;
			if (rightThigh != null || rightCalf != null || rightFoot != null) return false;
			if (leftUpperArm != null || leftForearm != null || leftHand != null) return false;
			if (rightUpperArm != null || rightForearm != null || rightHand != null) return false;
				
			foreach (Transform s in spine) if (s != null) return false;
			foreach (Transform eye in eyes) if (eye != null) return false;
			return true;
		}

		/// <summary>
		/// Params for automatic biped recognition. (Using a struct here because I might need to add more parameters in the future).
		/// </summary>
		public struct AutoDetectParams {
			
			/// <summary>
			/// Should the immediate parent of the legs be included in the spine?.
			/// </summary>
			public bool legsParentInSpine;
			public bool includeEyes;
			
			public AutoDetectParams(bool legsParentInSpine, bool includeEyes) {
				this.legsParentInSpine = legsParentInSpine;
				this.includeEyes = includeEyes;
			}
			
			public static AutoDetectParams Default {
				get {
					return new AutoDetectParams(true, true);
				}
			}
		}
		
		/// <summary>
		/// Automatically detects biped bones. Returns true if a valid biped has been referenced.
		/// </summary>
		public static bool AutoDetectReferences(ref BipedReferences references, Transform root, AutoDetectParams autoDetectParams) {
			if (references == null) references = new BipedReferences();
			references.root = root;
			
			// Try with naming and hierarchy first
			DetectReferencesByNaming(ref references, root, autoDetectParams);
			
			if (references.isValid && CheckSetupError(references, false) && CheckSetupWarning(references, false)) return true;
			
			// If that failed try the Animator
			AssignHumanoidReferences(ref references, root.GetComponent<Animator>(), autoDetectParams);
			
			bool isValid = references.isValid;
			
			if (!isValid) {
				Warning.Log("BipedReferences contains one or more missing Transforms.", root, true);
			}
			
			return isValid;
		}
		
		/// <summary>
		/// Detects the references based on naming and hierarchy.
		/// </summary>
		public static void DetectReferencesByNaming(ref BipedReferences references, Transform root, AutoDetectParams autoDetectParams) {
			if (references == null) references = new BipedReferences();

			Transform[] children = root.GetComponentsInChildren<Transform>();
			
			// Find limbs
			DetectLimb(BipedNaming.BoneType.Arm, BipedNaming.BoneSide.Left, ref references.leftUpperArm, ref references.leftForearm, ref references.leftHand, children);
			DetectLimb(BipedNaming.BoneType.Arm, BipedNaming.BoneSide.Right, ref references.rightUpperArm, ref references.rightForearm, ref references.rightHand, children);
			DetectLimb(BipedNaming.BoneType.Leg, BipedNaming.BoneSide.Left, ref references.leftThigh, ref references.leftCalf, ref references.leftFoot, children);
			DetectLimb(BipedNaming.BoneType.Leg, BipedNaming.BoneSide.Right, ref references.rightThigh, ref references.rightCalf, ref references.rightFoot, children);
			
			// Find head bone
			references.head = BipedNaming.GetBone(children, BipedNaming.BoneType.Head);
			
			// Find Pelvis
			references.pelvis = BipedNaming.GetNamingMatch(children, BipedNaming.pelvis);
			
			// If pelvis is not an ancestor of a leg, it is not a valid pelvis
			if (references.pelvis == null || !Hierarchy.IsAncestor(references.leftThigh, references.pelvis)) {
				if (references.leftThigh != null) references.pelvis = references.leftThigh.parent;
			}
			
			// Find spine and head bones
			if (references.leftUpperArm != null && references.rightUpperArm != null && references.pelvis != null && references.leftThigh != null) {
				Transform neck = Hierarchy.GetFirstCommonAncestor(references.leftUpperArm, references.rightUpperArm);

				if (neck != null) {
					Transform[] inverseSpine = new Transform[1] { neck };
					Hierarchy.AddAncestors(inverseSpine[0], references.pelvis, ref inverseSpine);
					
					references.spine = new Transform[0];
					for (int i = inverseSpine.Length - 1; i > -1; i--) {
						if (AddBoneToSpine(inverseSpine[i], ref references, autoDetectParams)) {
							Array.Resize(ref references.spine, references.spine.Length + 1);
							references.spine[references.spine.Length - 1] = inverseSpine[i];
						}
					}
					
					// Head
					if (references.head == null) {
						for (int i = 0; i < neck.childCount; i++) {
							Transform child = neck.GetChild(i);
							
							if (!Hierarchy.ContainsChild(child, references.leftUpperArm) && !Hierarchy.ContainsChild(child, references.rightUpperArm)) {
								references.head = child;
								break;
							}
						}
					}
				}
			}
			
			// Find eye bones
			Transform[] eyes = BipedNaming.GetBonesOfType(BipedNaming.BoneType.Eye, children);
			references.eyes = new Transform[0];
			
			if (autoDetectParams.includeEyes) {
				for (int i = 0; i < eyes.Length; i++) {
					if (AddBoneToEyes(eyes[i], ref references, autoDetectParams)) {
						Array.Resize(ref references.eyes, references.eyes.Length + 1);
						references.eyes[references.eyes.Length - 1] = eyes[i];
					}
				}
			}
		}
		
		/// <summary>
		/// Fills in BipedReferences using Animator.GetBoneTransform().
		/// </summary>
		public static void AssignHumanoidReferences(ref BipedReferences references, Animator animator, AutoDetectParams autoDetectParams) {
			if (references == null) references = new BipedReferences();

			if (animator == null || !animator.isHuman) return;
			
			references.spine = new Transform[0];
			references.eyes = new Transform[0];
			
			references.head = animator.GetBoneTransform(HumanBodyBones.Head);
			
			references.leftThigh = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
			references.leftCalf = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
			references.leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
			
			references.rightThigh = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
			references.rightCalf = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
			references.rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
			
			references.leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
			references.leftForearm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
			references.leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
			
			references.rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
			references.rightForearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
			references.rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
			
			references.pelvis = animator.GetBoneTransform(HumanBodyBones.Hips);
			
			AddBoneToHierarchy(ref references.spine, animator.GetBoneTransform(HumanBodyBones.Spine));
			AddBoneToHierarchy(ref references.spine, animator.GetBoneTransform(HumanBodyBones.Chest));
			
			// Make sure the neck bone is not above the arms
			if (references.leftUpperArm != null) {
				if (!IsNeckBone(animator.GetBoneTransform(HumanBodyBones.Neck), references.leftUpperArm)) AddBoneToHierarchy(ref references.spine, animator.GetBoneTransform(HumanBodyBones.Neck));
			}
			
			if (autoDetectParams.includeEyes) {
				AddBoneToHierarchy(ref references.eyes, animator.GetBoneTransform(HumanBodyBones.LeftEye));
				AddBoneToHierarchy(ref references.eyes, animator.GetBoneTransform(HumanBodyBones.RightEye));
			}
		}

		/// <summary>
		/// Checks the setup for definite problems.
		/// </summary>
		public static bool CheckSetupError(BipedReferences references, bool log) {
			if (!references.isValid) {
				if (log) Warning.Log("BipedReferences contains one or more missing Transforms.", references.root, true);
				return false;
			}
			
			if (!CheckLimbError(references.leftThigh, references.leftCalf, references.leftFoot, log)) return false;
			if (!CheckLimbError(references.rightThigh, references.rightCalf, references.rightFoot, log)) return false;
			if (!CheckLimbError(references.leftUpperArm, references.leftForearm, references.leftHand, log)) return false;
			if (!CheckLimbError(references.rightUpperArm, references.rightForearm, references.rightHand, log)) return false;
			
			if (!CheckSpineError(references, log)) return false;
			if (!CheckEyesError(references, log)) return false;
			
			return true;
		}

		/// <summary>
		/// Checks the setup for possible problems.
		/// </summary>
		public static bool CheckSetupWarning(BipedReferences references, bool log) {
			if (!CheckLimbWarning(references.leftThigh, references.leftCalf, references.leftFoot, log)) return false;
			if (!CheckLimbWarning(references.rightThigh, references.rightCalf, references.rightFoot, log)) return false;
			if (!CheckLimbWarning(references.leftUpperArm, references.leftForearm, references.leftHand, log)) return false;
			if (!CheckLimbWarning(references.rightUpperArm, references.rightForearm, references.rightHand, log)) return false;
			
			if (!CheckSpineWarning(references, log)) return false;
			if (!CheckEyesWarning(references, log)) return false;
			
			if (!CheckRootHeightWarning(references, log)) return false;
			if (!CheckFacingAxisWarning(references, log)) return false;
			
			return true;
		}

		
		#endregion Main Interface

		// Determines whether a Transform is above the arms
		private static bool IsNeckBone(Transform bone, Transform leftUpperArm) {
			if (leftUpperArm.parent != null && leftUpperArm.parent == bone) return false;
			if (Hierarchy.IsAncestor(leftUpperArm, bone)) return false;
			return true;
		}

		// Determines whether a bone is valid for being added into the eyes array
		private static bool AddBoneToEyes(Transform bone, ref BipedReferences references, AutoDetectParams autoDetectParams) {
			if (references.head != null) {
				if (!Hierarchy.IsAncestor(bone, references.head)) return false;
			}
			
			if (bone.GetComponent<SkinnedMeshRenderer>() != null) return false;
			
			return true;
		}
		
		// Determines whether a bone is valid for being added into the spine
		private static bool AddBoneToSpine(Transform bone, ref BipedReferences references, AutoDetectParams autoDetectParams) {
			if (bone == references.root) return false;
			
			bool isLegsParent = bone == references.leftThigh.parent;
			if (isLegsParent && !autoDetectParams.legsParentInSpine) return false;
			
			if (references.pelvis != null) {
				if (bone == references.pelvis) return false;
				if (Hierarchy.IsAncestor(references.pelvis, bone)) return false;
			}
			
			return true;
		}
		
		// Tries to guess the limb bones based on naming
		private static void DetectLimb(BipedNaming.BoneType boneType, BipedNaming.BoneSide boneSide, ref Transform firstBone, ref Transform secondBone, ref Transform lastBone, Transform[] transforms) {
			Transform[] limb = BipedNaming.GetBonesOfTypeAndSide(boneType, boneSide, transforms);
			
			if (limb.Length < 3) {
				//Warning.Log("Unable to detect biped bones by bone names. Please manually assign bone references.", firstBone, true);
				return;
			}
			
			// Standard biped characters
			if (limb.Length == 3) {
				firstBone = limb[0];
				secondBone = limb[1];
				lastBone = limb[2];
			}
			
			// For Bootcamp soldier type of characters with more than 3 limb bones
			if (limb.Length > 3) {
				firstBone = limb[0];
				secondBone = limb[2];
				lastBone = limb[limb.Length - 1];
			}
		}
		
		// Adds transform to hierarchy if not null
		private static void AddBoneToHierarchy(ref Transform[] bones, Transform transform) {
			if (transform == null) return;
			
			Array.Resize(ref bones, bones.Length + 1);
			bones[bones.Length - 1] = transform;
		}
		
		// Check if the limb is properly set up
		private static bool CheckLimbError(Transform bone1, Transform bone2, Transform bone3, bool log) {
			if (bone1 == null) {
				if (log) Warning.Log("Bone 1 of a BipedReferences limb is null.", bone2, true);
				return false;
			}

			if (bone2 == null) {
				if (log) Warning.Log("Bone 2 of a BipedReferences limb is null.", bone3, true);
				return false;
			}

			if (bone3 == null) {
				if (log) Warning.Log("Bone 3 of a BipedReferences limb is null.", bone1, true);
				return false;
			}

			if (bone2.position == bone1.position) {
				if (log) Warning.Log("Second bone's position equals first bone's position in the biped's limb.", bone2, true);
				return false;
			}
			
			if (bone3.position == bone2.position) {
				if (log) Warning.Log("Third bone's position equals second bone's position in the biped's limb.", bone3, true);
				return false;
			}
			
			Transform duplicate = (Transform)Hierarchy.ContainsDuplicate(new Transform[3] { bone1, bone2, bone3 });
			if (duplicate != null) {
				if (log) Warning.Log(duplicate.name + " is represented multiple times in the same BipedReferences limb.", bone1, true);
				return false;
			}
			
			if (!Hierarchy.HierarchyIsValid(new Transform[3] { bone1, bone2, bone3 })) {
				if (log) Warning.Log(
					"BipedReferences limb hierarchy is invalid. Bone transforms in a limb do not belong to the same ancestry. Please make sure the bones are parented to each other. " + 
					"Bones: " + bone1.name + ", " + bone2.name + ", " + bone3.name, 
					bone1, 
					true);
				return false;
			}
			
			return true;
		}

		// Check if the limb is properly set up
		private static bool CheckLimbWarning(Transform bone1, Transform bone2, Transform bone3, bool log) {
			Vector3 cross = Vector3.Cross(bone2.position - bone1.position, bone3.position - bone1.position);
			
			if (cross == Vector3.zero) {
				if (log) Warning.Log(
					"BipedReferences limb is completely stretched out in the initial pose. IK solver can not calculate the default bend plane for the limb. " +
					"Please make sure you character's limbs are at least slightly bent in the initial pose. " +
					"First bone: " + bone1.name + ", second bone: " + bone2.name + ".",
					bone1,
					true);
				return false;
			}
			
			return true;
		}
		
		// Check if spine is properly set up
		private static bool CheckSpineError(BipedReferences references, bool log) {
			if (references.spine.Length == 0) return true;

			for (int i = 0; i < references.spine.Length; i++) {
				if (references.spine[i] == null) {
					if (log) Warning.Log("BipedReferences spine bone at index " + i + " is null.", references.root, true);
					return false;
				}
			}

			Transform duplicate = (Transform)Hierarchy.ContainsDuplicate(references.spine);
			if (duplicate != null) {
				if (log) Warning.Log(duplicate.name + " is represented multiple times in BipedReferences spine.", references.spine[0], true);
				return false;
			}
			
			if (!Hierarchy.HierarchyIsValid(references.spine)) {
				if (log) Warning.Log("BipedReferences spine hierarchy is invalid. Bone transforms in the spine do not belong to the same ancestry. Please make sure the bones are parented to each other.", references.spine[0], true);
				return false;
			}
			
			for (int i = 0; i < references.spine.Length; i++) {
				bool matchesParentPosition = false;
				if (i == 0 && references.spine[i].position == references.pelvis.position) matchesParentPosition = true;
				if (i != 0 && references.spine.Length > 1 && references.spine[i].position == references.spine[i - 1].position) matchesParentPosition = true;

				if (matchesParentPosition) {
					if (log) Warning.Log("Biped's spine bone nr " + i + " position is the same as it's parent spine/pelvis bone's position. Please remove this bone from the spine.", references.spine[i], true);
					return false;
				}
			}
			
			return true;
		}

		// Check if spine is properly set up
		private static bool CheckSpineWarning(BipedReferences references, bool log) {
			// Maybe need to add something here in the future
			return true;
		}
		
		// Check if eyes are properly set up
		private static bool CheckEyesError(BipedReferences references, bool log) {
			if (references.eyes.Length == 0) return true;

			for (int i = 0; i < references.eyes.Length; i++) {
				if (references.eyes[i] == null) {
					if (log) Warning.Log("BipedReferences eye bone at index " + i + " is null.", references.root, true);
					return false;
				}
			}
			
			Transform duplicate = (Transform)Hierarchy.ContainsDuplicate(references.eyes);
			if (duplicate != null) {
				if (log) Warning.Log(duplicate.name + " is represented multiple times in BipedReferences eyes.", references.eyes[0], true);
				return false;
			}
			
			return true;
		}

		// Check if eyes are properly set up
		private static bool CheckEyesWarning(BipedReferences references, bool log) {
			// Maybe need to add something here in the future
			return true;
		}
		
		// Check if BipedIK transform position is at the character's feet
		private static bool CheckRootHeightWarning(BipedReferences references, bool log) {
			if (references.head == null) return true;
			
			float headHeight = GetVerticalOffset(references.head.position, references.leftFoot.position, references.root.rotation);
			float rootHeight = GetVerticalOffset(references.root.position, references.leftFoot.position, references.root.rotation);
			
			if (rootHeight / headHeight > 0.2f) {
				if (log) Warning.Log("Biped's root Transform's position should be at ground level relative to the character (at the character's feet not at it's pelvis).", references.root, true);
				return false;
			}
			
			return true;
		}
		
		// Check if the character is facing the correct axis
		private static bool CheckFacingAxisWarning(BipedReferences references, bool log) {
			Vector3 handsLeftToRight = references.rightHand.position - references.leftHand.position;
			Vector3 feetLeftToRight = references.rightFoot.position - references.leftFoot.position;
			
			float dotHands = Vector3.Dot(handsLeftToRight.normalized, references.root.right);
			float dotFeet = Vector3.Dot(feetLeftToRight.normalized, references.root.right);
			
			if (dotHands < 0 || dotFeet < 0) {
				if (log) Warning.Log(
					"Biped does not seem to be facing it's forward axis. " +
					"Please make sure that in the initial pose the character is facing towards the positive Z axis of the Biped root gameobject.", references.root, true);
				return false;
			}
			
			return true;
		}
		
		// Gets vertical offset relative to a rotation
		private static float GetVerticalOffset(Vector3 p1, Vector3 p2, Quaternion rotation) {
			Vector3 v = Quaternion.Inverse(rotation) * (p1 - p2);
			return v.y;
		}
	}
}
