using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static Vector3 cursorPosition;
    private Vector3 startPosition, endPosition;
    public CharacterController controller;
    public CameraController cameraController;
    public AudioManager audioManager;

    public Player player;

    // Use this for initialization
    void Start()
    {
        endPosition = transform.position;
        startPosition = transform.position;

    }
    
    // Update is called once per frame
    void Update()
    {


        float verticalAxis = -Input.GetAxisRaw("Vertical");
        float horizontalAxis = Input.GetAxisRaw("Horizontal");
        Vector3 transformPos2d = transform.position;
        transformPos2d.y = 0;

        Quaternion newRotation;

        float movementSpeed = 4f; //TODO

        if (verticalAxis != 0 || horizontalAxis != 0)
        {
            locatePosition(false);
            
            Vector3 targetDirection = new Vector3(-horizontalAxis + transformPos2d.x, 0, verticalAxis + transformPos2d.z);
            newRotation = Quaternion.LookRotation(targetDirection - transformPos2d, Vector3.up);
                
            animation.CrossFade("human_run");
            if(!audioManager.audiolist[3].isPlaying)
            audioManager.PlayAudio(3);
        } 
        //no movement, mouse controls rotation
        else
        {
            movementSpeed = 0; //no move

            locatePosition(true);

            Vector3 targetVector = endPosition - transformPos2d;
            if (targetVector != Vector3.zero)
            {
                newRotation = Quaternion.LookRotation(targetVector, Vector3.forward);
                newRotation.x = 0f;
                newRotation.z = 0f;
            } else
                newRotation = transform.rotation;
            
            transformPos2d.y = 0;

            if (!animation.isPlaying || animation.IsPlaying("human_run"))
            {
                audioManager.audiolist[3].Stop();
                if (player.isAlive)
                    animation.CrossFade("human_idle");

            }
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 10);
        controller.SimpleMove(Vector3.forward * verticalAxis * movementSpeed + Vector3.left * horizontalAxis * movementSpeed);

       // cameraController.update();
    }

    public void locatePosition(bool setPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        bool highlightedObject = false;
        
        if (Physics.Raycast(ray, out hit, 1000))
        {
            if (hit.collider.tag != "Player")
            {
                if (setPosition)
                {
                    endPosition = hit.point;
                    endPosition.y = 0;
                }
                
                //left click
                GameObject hitGameObject = hit.transform.gameObject;
            }
        }
    }

    public void lose()
    {
        Application.LoadLevel("EndScreenLose");
    }
}
