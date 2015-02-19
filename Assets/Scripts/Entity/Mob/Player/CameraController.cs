using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public new Camera camera;
    public GameObject target;
    public Vector3 offset;

    private readonly Vector3 PLAYER_OFFSET = new Vector3(0,1.2f,0);

	// Use this for initialization
	void Start () {
        offset = new Vector3(0, 30, 10);
        camera.transform.position = target.transform.position + offset;
        camera.transform.LookAt(target.transform);
	}
	
    /* NOTE: not the monodevelop update */
    void LateUpdate()
    {
        Vector3 targetPosition = target.transform.position + PLAYER_OFFSET;

        offset = new Vector3(0, 13, 8);

        Vector3 lerpTarget = targetPosition;
        camera.transform.position = targetPosition + offset;

        lerpTarget = Vector3.Lerp(camera.transform.position, lerpTarget + offset, Time.deltaTime * 0.1f);

        camera.transform.position = lerpTarget;

        camera.transform.rotation = Quaternion.Lerp(camera.transform.rotation, Quaternion.LookRotation(targetPosition - camera.transform.position), Time.deltaTime * 1000f);
    }
   
}
