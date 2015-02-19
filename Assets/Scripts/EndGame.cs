using UnityEngine;
using System.Collections;

public class EndGame : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider collision) {
        if (collision.collider.gameObject.GetComponent<Player>() != null)
        {
            Application.LoadLevel("EndScreenWin");
        }
    }
}
