using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Foot placement system.
	/// </summary>
	[System.Serializable]
	public partial class Grounding {
		
		#region Main Interface

		/// <summary>
		/// The raycasting quality. Fastest is a single raycast per foot, Simple is three raycasts, Best is one raycast and a capsule cast per foot.
		/// </summary>
		[System.Serializable]
		public enum Quality {
			Fastest,
			Simple,
			Best
		}

		/// <summary>
		/// Layers to ground the character to.
		/// </summary>
		public LayerMask layers;
		/// <summary>
		/// Max step height.
		/// </summary>
		public float maxStep = 0.5f;
		/// <summary>
		/// The height offset of the root
		/// </summary>
		public float heightOffset;
		/// <summary>
		/// Foot lerping speed.
		/// </summary>
		public float footSpeed = 2.5f;
		/// <summary>
		/// CapsuleCast radius. Should match approximately the feet size.
		/// </summary>
		public float footRadius = 0.15f;
		/// <summary>
		/// Amount of velocity based prediction of the foot positions.
		/// </summary>
		public float prediction = 0.05f;
		/// <summary>
		/// Weight of rotating the foot to ground normal offset.
		/// </summary>
		[Range(0f, 1f)]
		public float footRotationWeight = 1f;
		/// <summary>
		/// Speed of slerping the foot to it's grounded rotation.
		/// </summary>
		public float footRotationSpeed = 7f;
		/// <summary>
		/// Max Foot Rotation Angle", "Max angular offset from the foot's rotation (Reasonable range: 0-90 degrees).
		/// </summary>
		[Range(0f, 90f)]
		public float maxFootRotationAngle = 45f;
		/// <summary>
		/// If true, solver will rotate with the character root so the character can be grounded for example to spherical planets. 
		/// For performance reasons leave this off unless needed.
		/// </summary>
		public bool rotateSolver;
		/// <summary>
		/// The pelvis lerping speed.
		/// </summary>
		public float pelvisSpeed = 5f;
		/// <summary>
		/// Used for smoothing out vertical pelvis movement (range 0 - 1).
		/// </summary>
		[Range(0f, 1f)]
		public float pelvisDamper;
		/// <summary>
		/// The weight of lowering the pelvis to the lowest foot
		/// </summary>
		public float lowerPelvisWeight = 1f;
		/// <summary>
		/// The weight of lifting the pelvis to the highest foot
		/// </summary>
		public float liftPelvisWeight;
		/// <summary>
		/// The radius of the spherecast from the root that determines whether the character root is grounded.
		/// </summary>
		public float rootSphereCastRadius = 0.1f;
		/// <summary>
		/// The raycasting quality. Fastest is a single raycast per foot, Simple is three raycasts, Best is one raycast and a capsule cast per foot.
		/// </summary>
		public Quality quality = Quality.Best;

		/// <summary>
		/// The %Grounding legs.
		/// </summary>
		public Leg[] legs { get; private set; }
		/// <summary>
		/// The %Grounding pelvis.
		/// </summary>
		public Pelvis pelvis { get; private set; }
		/// <summary>
		/// Gets a value indicating whether any of the legs are grounded
		/// </summary>
		public bool isGrounded { get; private set; }
		/// <summary>
		/// The root Transform
		/// </summary>
		public Transform root { get; private set; }
		/// <summary>
		/// Ground height at the root position.
		/// </summary>
		public RaycastHit rootHit { get; private set; }
		/// <summary>
		/// Is the RaycastHit from the root grounded?
		/// </summary>
		public bool rootGrounded {
			get {
				return rootHit.distance < maxStep * 2f;
			}
		}

		/// <summary>
		/// Raycasts or sphereCasts to find the root ground point. Distance of the Ray/Sphere cast is maxDistanceMlp x maxStep. Use this instead of rootHit if the Grounder is weighed out/disabled and not updated.
		/// </summary>
		public RaycastHit GetRootHit(float maxDistanceMlp = 10f) {
			RaycastHit h = new RaycastHit();
			Vector3 _up = up;
			h.point = root.position - _up * maxStep * 10f;
			float distMlp = maxDistanceMlp + 1;
			h.distance = maxStep * distMlp;
			
			if (maxStep <= 0f) return h;
			
			if (quality != Quality.Best) Physics.Raycast(root.position + _up * maxStep, -_up, out h, maxStep * distMlp, layers);
			else Physics.SphereCast(root.position + _up * maxStep, rootSphereCastRadius, -up, out h, maxStep * distMlp, layers);
			
			return h;
		}


		/// <summary>
		/// Gets a value indicating whether this <see cref="Grounding"/> is valid.
		/// </summary>
		public bool IsValid(bool log) {
			if (root == null) {
				if (log) LogWarning("Root transform is null. Can't initiate Grounding.");
				return false;
			}
			if (legs == null) {
				if (log) LogWarning("Grounding legs is null. Can't initiate Grounding.");
				return false;
			}
			if (pelvis == null) {
				if (log) LogWarning("Grounding pelvis is null. Can't initiate Grounding.");
				return false;
			}
			
			if (legs.Length == 0) {
				if (log) LogWarning("Grounding has 0 legs. Can't initiate Grounding.");
				return false;
			}
			return true;
		}
		
		/// <summary>
		/// Initiate the %Grounding as an integrated solver by providing the root Transform, leg solvers, pelvis Transform and spine solver.
		/// </summary>
		public void Initiate(Transform root, Transform[] feet) {
			this.root = root;
			initiated = false;

			rootHit = new RaycastHit();

			// Constructing Legs
			if (legs == null) legs = new Leg[feet.Length];
			if (legs.Length != feet.Length) legs = new Leg[feet.Length];
			for (int i = 0; i < feet.Length; i++) if (legs[i] == null) legs[i] = new Leg();
			
			// Constructing pelvis
			if (pelvis == null) pelvis = new Pelvis();
			
			if (!IsValid(true)) return;
			
			// Initiate solvers only if application is playing
			if (Application.isPlaying) {
				for (int i = 0; i < feet.Length; i++) legs[i].Initiate(this, feet[i]);
				pelvis.Initiate(this);
				
				initiated = true;
			}
		}

		/// <summary>
		/// Updates the Grounding.
		/// </summary>
		public void Update() {
			if (!initiated) return;

			if (layers == 0) LogWarning("Grounding layers are set to nothing. Please add a ground layer.");

			maxStep = Mathf.Clamp(maxStep, 0f, maxStep);
			footRadius = Mathf.Clamp(footRadius, 0.0001f, maxStep);
			pelvisDamper = Mathf.Clamp(pelvisDamper, 0f, 1f);
			rootSphereCastRadius = Mathf.Clamp(rootSphereCastRadius, 0.0001f, rootSphereCastRadius);
			maxFootRotationAngle = Mathf.Clamp(maxFootRotationAngle, 0f, 90f);
			prediction = Mathf.Clamp(prediction, 0f, prediction);
			footSpeed = Mathf.Clamp(footSpeed, 0f, footSpeed);

			// Root hit
			rootHit = GetRootHit();

			float lowestOffset = Mathf.NegativeInfinity;
			float highestOffset = Mathf.Infinity;
			isGrounded = false;

			// Process legs
			foreach (Leg leg in legs) {
				leg.Process();

				if (leg.IKOffset > lowestOffset) lowestOffset = leg.IKOffset;
				if (leg.IKOffset < highestOffset) highestOffset = leg.IKOffset;

				if (leg.isGrounded) isGrounded = true;
			}
			
			// Precess pelvis
			pelvis.Process(-lowestOffset * lowerPelvisWeight, -highestOffset * liftPelvisWeight, isGrounded);
		}

		// Calculate the normal of the plane defined by leg positions, so we know how to rotate the body
		public Vector3 GetLegsPlaneNormal() {
			if (!initiated) return Vector3.up;

			Vector3 _up = up;
			Vector3 normal = _up;
			
			// Go through all the legs, rotate the normal by it's offset
			for (int i = 0; i < legs.Length; i++) {
				// Direction from the root to the leg
				Vector3 legDirection = legs[i].IKPosition - root.position; 
				
				// Find the tangent
				Vector3 legNormal = _up;
				Vector3 legTangent = legDirection;
				Vector3.OrthoNormalize(ref legNormal, ref legTangent);
				
				// Find the rotation offset from the tangent to the direction
				Quaternion fromTo = Quaternion.FromToRotation(legTangent, legDirection);

				// Rotate the normal
				normal = fromTo * normal;
			}
			
			return normal;
		}

		#endregion Main Interface
		
		private bool initiated;

		// Logs the warning if no other warning has beed logged in this session.
		public void LogWarning(string message) {
			Warning.Log(message, root);
		}
		
		// The up vector in solver rotation space.
		public Vector3 up {
			get {
				return (useRootRotation? root.up: Vector3.up);
			}
		}
		
		// Gets the vertical offset between two vectors in solver rotation space
		public float GetVerticalOffset(Vector3 p1, Vector3 p2) {
			if (useRootRotation) {
				Vector3 v = Quaternion.Inverse(root.rotation) * (p1 - p2);
				return v.y;
			}
			
			return p1.y - p2.y;
		}
		
		// Flattens a vector to ground plane in solver rotation space
		public Vector3 Flatten(Vector3 v) {
			if (useRootRotation) {
				Vector3 tangent = v;
				Vector3 normal = root.up;
				Vector3.OrthoNormalize(ref normal, ref tangent);
				return Vector3.Project(v, tangent);
			}
			
			v.y = 0;
			return v;
		}
		
		// Determines whether to use root rotation as solver rotation
		private bool useRootRotation {
			get {
				if (!rotateSolver) return false;
				if (root.up == Vector3.up) return false;
				return true;
			}
		}
	}
}


