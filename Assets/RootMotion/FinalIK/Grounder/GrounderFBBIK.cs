using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Grounding for FBBIK characters.
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder Full Body Biped")]
	public class GrounderFBBIK: Grounder {
		
		#region Main Interface

		/// <summary>
		/// Contains the bending weights for an effector.
		/// </summary>
		[System.Serializable]
		public class SpineEffector {
			public FullBodyBipedEffector effectorType; // The type of the effector
			public float horizontalWeight = 1f; // The horizontal bend offset weight 
			public float verticalWeight; // The vertical bend offset weight
		}

		/// <summary>
		/// Reference to the FBBIK componet.
		/// </summary>
		public FullBodyBipedIK ik;
		/// <summary>
		/// The amount of spine bending.
		/// </summary>
		public float spineBend = 2f;
		/// <summary>
		/// The interpolation speed of spine bending
		/// </summary>
		public float spineSpeed = 3f;
		/// <summary>
		/// The spine bending effectors.
		/// </summary>
		public SpineEffector[] spine = new SpineEffector[0];

		#endregion Main Interface

		private Transform[] feet = new Transform[2];
		private Vector3 spineOffset;
		private bool firstSolve;

		// Can we initiate the Grounding?
		private bool IsReadyToInitiate() {
			if (ik == null) return false;
			if (!ik.solver.initiated) return false;
			return true;
		}

		// Initiate once we have a FBBIK component
		void Update() {
			firstSolve = true;
			weight = Mathf.Clamp(weight, 0f, 1f);
			if (weight <= 0f) return;

			if (initiated) return;
			if (!IsReadyToInitiate()) return;
			
			Initiate();
		}

		private void Initiate () {
			// Set maintainRotationWeight to 1 for both limbs so their rotation will be maintained as animated
			ik.solver.leftLegMapping.maintainRotationWeight = 1f;
			ik.solver.rightLegMapping.maintainRotationWeight = 1f;

			// Gathering both foot bones from the FBBIK
			feet = new Transform[2];
			feet[0] = ik.solver.leftFootEffector.bone;
			feet[1] = ik.solver.rightFootEffector.bone;

			// Add to the FBBIK OnPreUpdate delegate to know when it solves
			ik.solver.OnPreUpdate += OnSolverUpdate;

			// Initiate Grounding
			solver.Initiate(ik.references.root, feet);

			initiated = true;
		}

		// Called before updating the main IK solver
		private void OnSolverUpdate() {
			if (!firstSolve) return;
			firstSolve = false;
			if (!enabled) return;
			if (weight <= 0f) return;

			if (OnPreGrounder != null) OnPreGrounder();

			solver.Update();

			// Move the pelvis
			ik.references.pelvis.position += solver.pelvis.IKOffset * weight;

			// Set effector positionOffsets for the feet
			SetLegIK(ik.solver.leftFootEffector, solver.legs[0]);
			SetLegIK(ik.solver.rightFootEffector, solver.legs[1]);

			// Bending the spine
			if (spineBend != 0f) {
				spineSpeed = Mathf.Clamp(spineSpeed, 0f, spineSpeed);

				Vector3 spineOffseTarget = GetSpineOffsetTarget() * weight;
				spineOffset = Vector3.Lerp(spineOffset, spineOffseTarget * spineBend, Time.deltaTime * spineSpeed);
				Vector3 verticalOffset = ik.references.root.up * spineOffset.magnitude;

				for (int i = 0; i < spine.Length; i++) {
					ik.solver.GetEffector(spine[i].effectorType).positionOffset += (spineOffset * spine[i].horizontalWeight) + (verticalOffset * spine[i].verticalWeight);
				}
			}

			if (OnPostGrounder != null) OnPostGrounder();
		}

		// Set the effector positionOffset for the foot
		private void SetLegIK(IKEffector effector, Grounding.Leg leg) {
			effector.positionOffset += (leg.IKPosition - effector.bone.position) * weight;

			effector.bone.rotation = Quaternion.Slerp(Quaternion.identity, leg.rotationOffset, weight) * effector.bone.rotation;
		}

		// Cleaning up the delegate
		void OnDestroy() {
			if (initiated && ik != null) ik.solver.OnPreUpdate -= OnSolverUpdate;
		}

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		protected override void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page11.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_f_b_b_i_k.html");
		}
	}
}
