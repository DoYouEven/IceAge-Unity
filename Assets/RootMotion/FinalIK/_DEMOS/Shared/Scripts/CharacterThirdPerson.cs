using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Third person character controller. This class is based on the ThirdPersonCharacter.cs of the Unity Exmaple Assets.
	/// </summary>
	public class CharacterThirdPerson : CharacterBase {

		// Is the character always rotating to face the move direction or is he strafing?
		[System.Serializable]
		public enum MoveMode {
			Directional,
			Strafe
		}

		// Animation state
		public struct AnimState {
			public Vector3 moveDirection; // the forward speed
			public bool jump; // should the character be jumping?
			public bool crouch; // should the character be crouching?
			public bool onGround; // is the character grounded
			public bool isStrafing; // should the character always rotate to face the move direction or strafe?
			public float yVelocity; // y velocity of the character
		}

		[System.Serializable]
		public class AdvancedSettings
		{
			public float stationaryTurnSpeedMlp = 1f;			// additional turn speed added when the player is stationary (added to animation root rotation)
			public float movingTurnSpeed = 5f;					// additional turn speed added when the player is moving (added to animation root rotation)
			public float lookResponseSpeed = 2f;				// speed at which head look follows its target
			public float jumpRepeatDelayTime = 0.25f;			// amount of time that must elapse between landing and being able to jump again
			public float groundStickyEffect = 5f;				// power of 'stick to ground' effect - prevents bumping down slopes.
			public float platformFriction = 7f;					// the acceleration of adapting the velocities of moving platforms
			public float maxVerticalVelocityOnGround = 3f;		// the maximum y velocity while the character is grounded
			public float crouchCapsuleScaleMlp = 0.6f;			// the capsule collider scale multiplier while crouching
			public float velocityToGroundTangentWeight = 1f;	// the weight of rotating character velocity vector to the ground tangent
			public float wallRunMaxLength = 1f;					// max duration of a wallrun
			public float wallRunMinHorVelocity = 3f;			// the minimum horizontal velocity of doing a wall run
			public float wallRunMinVelocityY = -1f;				// the minimum vertical velocity of doing a wall run
			public float wallRunRotationSpeed = 1.5f;			// the speed of rotating the character to the wall normal
			public float wallRunMaxRotationAngle = 70f;			// max angle of character rotation
			public float wallRunWeightSpeed = 5f;				// the speed of blending in/out the wall running effect
		}

		// Variables you probably don't need to change with external scripts
		[SerializeField] CharacterAnimationThirdPerson characterAnimation; // the animation controller
		[SerializeField] protected Grounder grounder; // reference to the Grounder
		[SerializeField] LayerMask wallRunLayers; // walkable vertical surfaces
		[Range(1f, 4f)] [SerializeField] float gravityMultiplier = 2f;	// gravity modifier - often higher than natural gravity feels right for game characters

		// Variables you might want to change with external scripts
		public MoveMode moveMode; // Is the character always rotating to face the move direction or is he strafing?
		public float accelerationTime = 0.2f; // The acceleration of the speed of the character
		public float airSpeed = 6f; // determines the max speed of the character while airborne
		public float airControl = 2f; // determines the response speed of controlling the character while airborne
		public float jumpPower = 12f; // determines the jump force applied when jumping (and therefore the jump height)
		public bool lookInCameraDirection; // should the character be looking in the same direction that the camera is facing
		public AdvancedSettings advancedSettings; // Container for the advanced settings class , thiss allows the advanced settings to be in a foldout in the inspector

		public bool onGround { get; private set; }

		private UserControlThirdPerson.State inputState = new UserControlThirdPerson.State(); // We get the input state from UserControlerThirdPerson 
		private Vector3 moveDirection; // The current move direction of the character in Strafe move mode
		private Vector3 lookPosSmooth;
		private Animator animator;
		private Vector3 normal, platformVelocity;
		private RaycastHit hit;
		private float jumpLeg, jumpEndTime, forwardMlp, groundDistance, lastAirTime, stickyForce;
		private Vector3 wallNormal = Vector3.up;
		private Vector3 moveDirectionVelocity;
		private float wallRunWeight;
		private float lastWallRunWeight;
		private AnimState animState = new AnimState();

		// Use this for initialization
		protected override void Start () {
			base.Start();

			animator = GetComponent<Animator>();
			wallNormal = Vector3.up;

			lookPosSmooth = transform.position + transform.forward * 10f;
		}

		void OnAnimatorMove() {
			Move (animator.deltaPosition);
		}

		// Should the character be enabled to do a wall run?
		private bool CanWallRun() {
			if (Time.time < jumpEndTime - 0.1f) return false;
			if (Time.time > jumpEndTime - 0.1f + advancedSettings.wallRunMaxLength) return false;
			if (rigidbody.velocity.y < advancedSettings.wallRunMinVelocityY) return false;
			if (inputState.move == Vector3.zero) return false;

			Vector3 horVel = rigidbody.velocity;
			horVel.y = 0f;
			if (horVel.magnitude < advancedSettings.wallRunMinHorVelocity) return false;

			return true;
		}

		// Processing horizontal wall running
		private void WallRun() {
			wallRunWeight = Mathf.MoveTowards(wallRunWeight, (CanWallRun()? 1f: 0f), Time.deltaTime * advancedSettings.wallRunWeightSpeed);

			if (wallRunWeight <= 0f) {
				// Reset
				if (lastWallRunWeight > 0f) {
					transform.rotation = Quaternion.LookRotation(new Vector3(transform.forward.x, 0f, transform.forward.z), Vector3.up);
					wallNormal = Vector3.up;
				}
			}
			lastWallRunWeight = wallRunWeight;

			if (wallRunWeight <= 0f) return;

			// Make sure the character won't fall down
			if (onGround && rigidbody.velocity.y < 0f) rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
			
			// transform.forward flattened
			Vector3 f = transform.forward;
			f.y = 0f;
			
			// Raycasting to find a walkable wall
			RaycastHit velocityHit = new RaycastHit();
			velocityHit.normal = Vector3.up;
			Physics.Raycast(onGround? transform.position: collider.bounds.center, f, out velocityHit, 3f, wallRunLayers);
			
			// Finding the normal to rotate to
			wallNormal = Vector3.Slerp(wallNormal, velocityHit.normal, Time.deltaTime * advancedSettings.wallRunRotationSpeed);

			// Clamping wall normal to max rotation angle
			wallNormal = Vector3.RotateTowards(Vector3.up, wallNormal, advancedSettings.wallRunMaxRotationAngle * Mathf.Deg2Rad, 0f);
			
			// Get transform.forward ortho-normalized to the wall normal
			Vector3 fW = transform.forward;
			Vector3 nW = wallNormal;
			Vector3.OrthoNormalize(ref nW, ref fW);

			// Rotate from upright to wall normal
			transform.rotation = Quaternion.Slerp(Quaternion.LookRotation(f, Vector3.up), Quaternion.LookRotation(fW, wallNormal), wallRunWeight);
		}

		// When the Animator moves
		public override void Move(Vector3 deltaPosition) {
			// Process horizontal wall-running
			WallRun();

			Vector3 velocity = deltaPosition / Time.deltaTime;

			// Add velocity of the rigidbody the character is standing on
			velocity += new Vector3(platformVelocity.x, 0f, platformVelocity.z);

			if (onGround) {
				// Rotate velocity to ground tangent
				if (advancedSettings.velocityToGroundTangentWeight > 0f) {
					Quaternion r = Quaternion.FromToRotation(transform.up, normal);
					velocity = Quaternion.Lerp(Quaternion.identity, r, advancedSettings.velocityToGroundTangentWeight) * velocity;
				}
			} else {
				// Air move
				Vector3 airMove = new Vector3 (inputState.move.x * airSpeed, 0f, inputState.move.z * airSpeed);
				velocity = Vector3.Lerp(rigidbody.velocity, airMove, Time.deltaTime * airControl);
			}

			if (onGround && Time.time > jumpEndTime) {
				rigidbody.velocity = rigidbody.velocity - transform.up * stickyForce * Time.deltaTime;
			}

			// Vertical velocity
			velocity.y = Mathf.Clamp(rigidbody.velocity.y, rigidbody.velocity.y, onGround? advancedSettings.maxVerticalVelocityOnGround: rigidbody.velocity.y);

			rigidbody.velocity = velocity;

			// Dampering forward speed on the slopes
			float slopeDamper = !onGround? 1f: GetSlopeDamper(-deltaPosition / Time.deltaTime, normal);
			forwardMlp = Mathf.Lerp(forwardMlp, slopeDamper, Time.deltaTime * 5f);
		}

		public void UpdateState(UserControlThirdPerson.State _inputState) {
			// Make a local copy because it needs to be handled also elsewhere
			this.inputState = _inputState;

			Look(); // rotate the character
			GroundCheck (); // detect and stick to ground

			// Friction
			if (inputState.move == Vector3.zero && groundDistance < airborneThreshold * 0.5f) HighFriction();
			else ZeroFriction();

			if (onGround) {

				// Jumping
				animState.jump = Jump();
			} else {

				// Additional gravity
				rigidbody.AddForce((Physics.gravity * gravityMultiplier) - Physics.gravity);
			}

			// Scale the capsule colllider while crouching
			ScaleCapsule(inputState.crouch? advancedSettings.crouchCapsuleScaleMlp: 1f);

			// Fill in animState
			animState.moveDirection = GetMoveDirection();
			animState.crouch = inputState.crouch;
			animState.onGround = onGround;
			animState.yVelocity = rigidbody.velocity.y;
			animState.isStrafing = moveMode == MoveMode.Strafe;

			// Update animation
			characterAnimation.UpdateState(animState);
		}

		// Get the move direction of the character relative to the character rotation
		private Vector3 GetMoveDirection() {
			switch(moveMode) {
			case MoveMode.Directional:
				moveDirection = Vector3.SmoothDamp(moveDirection, new Vector3(0f, 0f, inputState.move.magnitude), ref moveDirectionVelocity, accelerationTime);
				return moveDirection * forwardMlp;
			case MoveMode.Strafe:
				moveDirection = Vector3.SmoothDamp(moveDirection, inputState.move, ref moveDirectionVelocity, accelerationTime);
				return transform.InverseTransformDirection(moveDirection);
			}

			return Vector3.zero;
		}

		// Rotate the character
		private void Look() {
			// update the current head-look position
			lookPosSmooth = Vector3.Lerp(lookPosSmooth, inputState.lookPos, Time.deltaTime * advancedSettings.lookResponseSpeed);

			float angle = GetAngleFromForward(GetLookDirection());
			
			if (inputState.move == Vector3.zero) angle *= (1.01f - (Mathf.Abs(angle) / 180f)) * advancedSettings.stationaryTurnSpeedMlp;
			
			// Rotating the character
			RigidbodyRotateAround(characterAnimation.GetPivotPoint(), transform.up, angle * Time.deltaTime * advancedSettings.movingTurnSpeed);
		}

		// Which way to look at?
		private Vector3 GetLookDirection() {
			bool isMoving = inputState.move != Vector3.zero;

			switch (moveMode) {
			case MoveMode.Directional:
				if (isMoving) return inputState.move;
				return lookInCameraDirection? inputState.lookPos - rigidbody.position: transform.forward;
			case MoveMode.Strafe:
				if (isMoving) return inputState.lookPos - rigidbody.position;
				return lookInCameraDirection? inputState.lookPos - rigidbody.position: transform.forward;
			}

			return Vector3.zero;
		}

		private bool Jump() {
			// check whether conditions are right to allow a jump:
			if (!inputState.jump) return false;
			if (inputState.crouch) return false;
			if (!characterAnimation.animationGrounded) return false;
			if (Time.time < lastAirTime + advancedSettings.jumpRepeatDelayTime) return false;

			// Jump
			onGround = false;
			jumpEndTime = Time.time + 0.1f;

			Vector3 jumpVelocity = inputState.move * airSpeed;
			rigidbody.velocity = jumpVelocity;
			rigidbody.velocity += transform.up * jumpPower;

			return true;
		}

		// Spherecasting for the root
		private RaycastHit GetHit() {
			if (grounder == null) return GetSpherecastHit();
			if (grounder.solver.quality != Grounding.Quality.Best) return GetSpherecastHit(); // We need a spherecast so if grounding quality is lower and it returns a raycast, we need to do it ourselves.

			if (grounder.enabled && grounder.weight > 0f) return grounder.solver.rootHit; // Already calculated by Grounding
			return grounder.solver.GetRootHit(); // Grounding is not updated, so get a fresh root hit
		}

		// Is the character grounded?
		private void GroundCheck () {
			Vector3 platformVelocityTarget = Vector3.zero;
			float stickyForceTarget = 0f;

			// Spherecasting
			hit = GetHit();

			normal = hit.normal;
			groundDistance = rigidbody.position.y - hit.point.y;

			// if not jumping...
			bool findGround = Time.time > jumpEndTime && rigidbody.velocity.y < jumpPower * 0.5f;

			if (findGround) {
				bool g = onGround;
				onGround = false;

				// The distance of considering the character grounded
				float groundHeight = !g? airborneThreshold * 0.5f: airborneThreshold;

				Vector3 horizontalVelocity = rigidbody.velocity;
				horizontalVelocity.y = 0f;
				
				float velocityF = horizontalVelocity.magnitude;

				if (groundDistance < groundHeight) {
					// Force the character on the ground
					stickyForceTarget = advancedSettings.groundStickyEffect * velocityF * groundHeight;

					// On moving platforms
					if (hit.rigidbody != null) platformVelocityTarget = hit.rigidbody.GetPointVelocity(hit.point);

					// Flag the character grounded
					onGround = true;
				}
			}

			// Interpolate the additive velocity of the platform the character might be standing on
			platformVelocity = Vector3.Lerp(platformVelocity, platformVelocityTarget, Time.deltaTime * advancedSettings.platformFriction);

			stickyForce = stickyForceTarget;//Mathf.Lerp(stickyForce, stickyForceTarget, Time.deltaTime * 5f);

			// remember when we were last in air, for jump delay
			if (!onGround) lastAirTime = Time.time;
		}
	}
}
