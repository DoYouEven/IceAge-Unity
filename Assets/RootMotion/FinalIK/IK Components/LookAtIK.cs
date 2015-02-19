using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Rotates a hierarchy of bones to face a target
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/Look At IK")]
	public class LookAtIK : IK {
		
		/// <summary>
		/// The LookAt %IK solver.
		/// </summary>
		public IKSolverLookAt solver = new IKSolverLookAt();
		
		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		protected override void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page8.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_look_at_i_k.html");
		}
	}
}
