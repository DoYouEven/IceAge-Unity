using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

    public int damage;
    public int thrower; //0 = player, 1 = NPC

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        gameObject.transform.Translate((Vector3.forward * 7f - Vector3.up * 0.6f) * Time.deltaTime);
	}

    void OnCollisionEnter(Collision collision) {

        if (thrower == 0)
        {
            NPC npc = collision.collider.gameObject.GetComponent<NPC>();
            if (npc != null)
            {
                npc.Damage(damage);
            }
           
            if (collision.collider.gameObject.GetComponent<Player>() == null)
            Destroy(gameObject);
        }
        else if (thrower == 1)
        {
            if (collision.collider.gameObject.GetComponent<NPC>() == null)
                Destroy(gameObject);
        }
        
    }
    
}
