using UnityEngine;
using System.Collections;

public class Flores : NPC {

	// Use this for initialization
	new void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public override void Die()
    {
        if (UnityEngine.Random.Range(0, 1f) < 0.2f)
        {
            GameObject itemGO = (GameObject)GameObject.Instantiate(Item.allItemModels [Item.Type.PICKAXE], gameObject.transform.position, Quaternion.Euler(new Vector3(0, 0, 0)));
            Item item = itemGO.GetComponent<Item>();
            item.Init(Item.Type.PICKAXE);
            item.Drop();
        }
    }
}
