using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// %IK system for multiple branched %FABRIK chains.
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/FABRIK Root")]
	public class FABRIKRoot : IK {
		
		/// <summary>
		/// The %FABRIKRoot solver.
		/// </summary>
		public IKSolverFABRIKRoot solver = new IKSolverFABRIKRoot();
		
		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		protected override void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page5.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_f_a_b_r_i_k_root.html");
		}
	}
}
