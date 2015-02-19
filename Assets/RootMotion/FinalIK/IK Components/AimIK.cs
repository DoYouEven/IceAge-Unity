using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Aim %IK solver component.
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/Aim IK")]
	public class AimIK : IK {
		
		/// <summary>
		/// The Aim %IK solver.
		/// </summary>
		public IKSolverAim solver = new IKSolverAim();
		
		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}

		// Open the User Manual URL
		[ContextMenu("User Manual")]
		protected override void OpenUserManual() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page1.html");
		}
		
		// Open the Script Reference URL
		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference() {
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_aim_i_k.html");
		}
	}
}

