using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Full Body %IK System designed specifically for bipeds
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/Full Body Biped IK")]
	public class FullBodyBipedIK : IK {
		
		/// <summary>
		/// The biped definition. Don't change refences directly in runtime, use SetReferences(BipedReferences references) instead.
		/// </summary>
		public BipedReferences references = new BipedReferences();
		
		/// <summary>
		/// The FullBodyBiped %IK solver.
		/// </summary>
		public IKSolverFullBodyBiped solver = new IKSolverFullBodyBiped();

		/// <summary>
		/// Sets the solver to new biped references.
		/// </summary>
		/// /// <param name="references">Biped references.</param>
		/// <param name="rootNode">Root node. if null, will try to detect the root node bone automatically. </param>
		public void SetReferences(BipedReferences references, Transform rootNode) {
			this.references = references;
			solver.SetToReferences(this.references, rootNode);
		}

		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		protected override void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page6.html");
		}

		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_full_body_biped_i_k.html");
		}
		
		// Reinitiates the solver to the current references
		[ContextMenu("Reinitiate")]
		void Reinitiate() {
			SetReferences(references, solver.rootNode);
		}

		// Open the User Manual URL
		[ContextMenu("Auto-detect References")]
		void AutoDetectReferences() {
			references = new BipedReferences();
			BipedReferences.AutoDetectReferences(ref references, transform, new BipedReferences.AutoDetectParams(true, false));

			solver.rootNode = IKSolverFullBodyBiped.DetectRootNodeBone(references);
			
			solver.SetToReferences(references, solver.rootNode);
		}
	}
}
