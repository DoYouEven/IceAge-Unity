using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Analytic %IK algorithm based on the law of cosines
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/Trigonometric IK")]
	public class TrigonometricIK : IK {
		
		/// <summary>
		/// The Trigonometric %IK solver.
		/// </summary>
		public IKSolverTrigonometric solver = new IKSolverTrigonometric();
		
		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		protected override void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page9.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_trigonometric_i_k.html");
		}
	}
}
