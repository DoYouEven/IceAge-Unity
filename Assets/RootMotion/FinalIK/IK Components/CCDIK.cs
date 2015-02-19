using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// CCD (Cyclic Coordinate Descent) %IK solver component.
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/CCD IK")]
	public class CCDIK : IK {
		
		/// <summary>
		/// The CCD %IK solver.
		/// </summary>
		public IKSolverCCD solver = new IKSolverCCD();
		
		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		protected override void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page3.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_c_c_d_i_k.html");
		}
	}
}
