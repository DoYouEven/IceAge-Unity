using UnityEngine;
using System.Collections;
using Pathfinding.RVO;


    /** AI controller specifically made for the spider robot.
     * The spider robot (or mine-bot) which is got from the Unity Example Project
     * can have this script attached to be able to pathfind around with animations working properly.\n
     * This script should be attached to a parent GameObject however since the original bot has Z+ as up.
     * This component requires Z+ to be forward and Y+ to be up.\n
     * 
     * It overrides the AIPath class, see that class's documentation for more information on most variables.\n
     * Animation is handled by this component. The Animation component refered to in #anim should have animations named "awake" and "forward".
     * The forward animation will have it's speed modified by the velocity and scaled by #animationSpeed to adjust it to look good.
     * The awake animation will only be sampled at the end frame and will not play.\n
     * When the end of path is reached, if the #endOfPathEffect is not null, it will be instantiated at the current position. However a check will be
     * done so that it won't spawn effects too close to the previous spawn-point.
     * \shadowimage{mine-bot.png}
     * 
     * \note This script assumes Y is up and that character movement is mostly on the XZ plane.
     */
[RequireComponent(typeof(Seeker))]
public class NPC : AIPath
{

    /** Animation component.
     * Should hold animations "awake" and "forward"
     * 
     * 
     * 
     */

    public float wanderDist = 8.0f;
    public enum AIState
    {
        Wander,
        Seek,
        Battle,
        MoveAway
    }
    
    public AIState State = AIState.Wander;
    /** Minimum velocity for moving */
    public float sleepVelocity = 0.4F;

    /** Speed relative to velocity with which to play animations */
    public float animationSpeed = 0.2F;

    /** Effect which will be instantiated when end of path is reached.
     * \see OnTargetReached */
    public GameObject endOfPathEffect;

    public Transform player;
    bool isAttacking = false;
    bool isBlockingBreaking = false;

    public new void Start()
    {

        player = GameObject.Find("Player").transform;
        //Prioritize the walking animation
        //anim["forward"].layer = 10;

        //Play all animations
        //anim.Play ("awake");
        //anim.Play ("forward");

        //Setup awake animations properties
        //anim["awake"].wrapMode = WrapMode.Clamp;
        //anim["awake"].speed = 0;
        //anim["awake"].normalizedTime = 1F;

        //Call Start in base script (AIPath)
        base.Start();
    }

    /** Point for the last spawn of #endOfPathEffect */
    protected Vector3 lastTarget;

    /**
     * Called when the end of path has been reached.
     * An effect (#endOfPathEffect) is spawned when this function is called
     * However, since paths are recalculated quite often, we only spawn the effect
     * when the current position is some distance away from the previous spawn-point
    */
    public override void OnTargetReached()
    {

        if (endOfPathEffect != null && Vector3.Distance(tr.position, lastTarget) > 1)
        {
            GameObject.Instantiate(endOfPathEffect, tr.position, tr.rotation);
            lastTarget = tr.position;
        }
    }

    public override Vector3 GetFeetPosition()
    {
        return tr.position;
    }

    protected new void Update()
    {
        switch (State)
        {
            case AIState.Wander: Wander(); break;
            case AIState.Seek: Seek(); break;
            case AIState.Battle: Battle(); break;
            case AIState.MoveAway: MoveAway(); break;
        }
        //Get velocity in world-space
        Vector3 velocity;
        if (canMove)
        {

            //Calculate desired velocity
            Vector3 dir = CalculateVelocity(GetFeetPosition());

            //Rotate towards targetDirection (filled in by CalculateVelocity)
            RotateTowards(targetDirection);

            dir.y = 0;
            if (dir.sqrMagnitude > sleepVelocity * sleepVelocity)
            {
                //If the velocity is large enough, move
            }
            else
            {
                //Otherwise, just stand still (this ensures gravity is applied)
                dir = Vector3.zero;
            }

            if (this.rvoController != null)
            {
                rvoController.Move(dir);
                velocity = rvoController.velocity;
            }
            else
                if (navController != null)
                {
#if FALSE
					navController.SimpleMove (GetFeetPosition(), dir);
#endif
                    velocity = Vector3.zero;
                }
                else if (controller != null)
                {
                    controller.SimpleMove(dir);
                    velocity = controller.velocity;
                }
                else
                {
                    Debug.LogWarning("No NavmeshController or CharacterController attached to GameObject");
                    velocity = Vector3.zero;
                }
        }
        else
        {
            velocity = Vector3.zero;
        }


        //Animation

        //Calculate the velocity relative to this transform's orientation
        Vector3 relVelocity = tr.InverseTransformDirection(velocity);
        relVelocity.y = 0;

        if (velocity.sqrMagnitude <= sleepVelocity * sleepVelocity)
        {
            //Fade out walking animation
            anim.Blend("human_run", 0, 0.2F);
        }
        else
        {
            //Fade in walking animation
            anim.Blend("human_run", 1, 0.2F);

            //Modify animation speed to match velocity
            AnimationState state = anim["human_run"];

            float speed = relVelocity.z;
            state.speed = speed * animationSpeed;
        }
    }

    void Wander()
    {


        target.position = transform.position + Quaternion.Euler(0, Random.value * 360, 0) * Vector3.right * wanderDist;
        Transform obj;
                // Call the 2D WithinSight function to determine if this specific object is within sight
        float angle;
      if ((obj = WithinSight(transform, 180, 50, player, true, out angle)) != null)
      {
          State = AIState.Seek;
      }

    }
    void Seek()
    {

        StartCoroutine(BreakBlock());
        target.position = player.position;
            
             Vector3 direction;
            // check each object. All it takes is one object to be able to return success
                
                direction = transform.position - player.position;
               
                // check to see if the square magnitude is less than what is specified
                if (Vector3.SqrMagnitude(direction) < attackDistance) 
                 {
                         State = AIState.Battle;
                }
    }

    void MoveAway()
    {
        StartCoroutine(CoMoveAway());
    }

    IEnumerator CoMoveAway()
    {

        target.position = player.position + new Vector3(2, 0, 2);

        yield return new WaitForSeconds(attackRate+0.3f);
        State = AIState.Seek;
    }
    void OnCollisionEnter(Collision collision)
    {
     
        if(State == AIState.Seek)

            if (collision.gameObject.layer == 10)
            {
                State = AIState.MoveAway;
            }
        

    }
    IEnumerator BreakBlock()
    {
        if (isBlockingBreaking == false)
        {

            RaycastHit hit;
            if (Physics.Raycast(this.transform.position + new Vector3(0, 0, 0), transform.forward, out hit, 3.5f))
            {
                Block block;
                if ((block = hit.collider.gameObject.GetComponent<Block>()) != null)
                {
                    isBlockingBreaking = true;
                    block.Damage(atkDamage);

                    anim.Play("human_attack");
                    yield return new WaitForSeconds(attackRate);
                    isBlockingBreaking = false;
                }
                else if (hit.collider.gameObject.layer == 10)
                {
                    State = AIState.MoveAway;

                }
            }
        }
          
    }
    void Battle()
    {
        Vector3 direction;
        direction = transform.position - player.position;

        if (Vector3.SqrMagnitude(direction) > attackDistance)
        {
            State = AIState.Seek;
            return;
        }
        
        if(isAttacking == false)
        {
            RotateTowards(direction.normalized);
            isAttacking = true;
            StartCoroutine(CoAttack());
          
        }
    }

    IEnumerator CoAttack()
    {
        Attack("Player");
       

        yield return new WaitForSeconds(attackRate);
        isAttacking = false;
    }

    public static Transform WithinSight2D(Transform transform, float fieldOfViewAngle, float viewDistance, LayerMask objectLayerMask)
    {
        Transform objectFound = null;
        var hitColliders = Physics2D.OverlapCircleAll(transform.position, viewDistance, objectLayerMask);
        if (hitColliders != null)
        {
            float minAngle = Mathf.Infinity;
            for (int i = 0; i < hitColliders.Length; ++i)
            {
                float angle;
                Transform obj;
                // Call the 2D WithinSight function to determine if this specific object is within sight
                if ((obj = WithinSight(transform, fieldOfViewAngle, viewDistance, hitColliders[i].transform, true, out angle)) != null)
                {
                    // This object is within sight. Set it to the objectFound GameObject if the angle is less than any of the other objects
                    if (angle < minAngle)
                    {
                        minAngle = angle;
                        objectFound = obj;
                    }
                }
            }
        }
        return objectFound;

    }
    private static Transform WithinSight(Transform transform, float fieldOfViewAngle, float viewDistance, Transform targetObject, bool usePhysics2D, out float angle)
    {
        // The target object needs to be within the field of view of the current object
        var direction = targetObject.position - transform.position;
        if (usePhysics2D)
        {
            angle = Vector3.Angle(direction, transform.up);
        }
        else
        {
            angle = Vector3.Angle(direction, transform.forward);
        }
        if (direction.magnitude < viewDistance && angle < fieldOfViewAngle * 0.5f)
        {
            // The hit agent needs to be within view of the current agent
            if (LineOfSight(transform, targetObject, usePhysics2D) != null)
            {
                return targetObject; // return the target object meaning it is within sight
            }
            else
            {
                // If the linecast doesn't hit anything then that the target object doesn't have a collider and there is nothing in the way
                if (targetObject.gameObject.activeSelf)
                    return targetObject;
            }
        }
        // return null if the target object is not within sight
        return null;
    }
    public static Transform LineOfSight(Transform transform, Transform targetObject, bool usePhysics2D)
    {
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
        if (usePhysics2D)
        {
            RaycastHit2D hit;
            if ((hit = Physics2D.Linecast(transform.position, targetObject.position)))
            {
                if (hit.transform.Equals(targetObject))
                {
                    return targetObject; // return the target object meaning it is within sight
                }
            }
        }
        else
        {
#endif
            RaycastHit hit;
            if (Physics.Linecast(transform.position, targetObject.position, out hit))
            {
                if (hit.transform.Equals(targetObject))
                {
                    return targetObject; // return the target object meaning it is within sight
                }
            }
#if !(UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
        }
#endif
        return null;
    }
}
  