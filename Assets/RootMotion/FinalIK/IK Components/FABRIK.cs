using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Forward and Backward Reaching %IK solver component.
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/FABRIK")]
	public class FABRIK : IK {
		
		/// <summary>
		/// The %FABRIK solver.
		/// </summary>
		public IKSolverFABRIK solver = new IKSolverFABRIK();
		
		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		protected override void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page4.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_f_a_b_r_i_k.html");
		}
	}
}

