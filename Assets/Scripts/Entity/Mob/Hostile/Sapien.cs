using UnityEngine;
using System.Collections;

public class Sapien : NPC {

    public int type;

	// Use this for initialization

    new void Start () {
        base.Start();
	    //TODO
	}
	
	// Update is called once per frame
	void Update () {
        base.Update();
	    //TODO
	}

    public override void Die()
    {
        base.Die();

        if (type == 0)
        {
            if (UnityEngine.Random.Range(0, 1f) < 0.4f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
                    GameObject itemGO = (GameObject)GameObject.Instantiate(Item.allItemModels [Item.Type.SPEAR], gameObject.transform.position + randomOffset, Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360), 0)));
                    Item item = itemGO.GetComponent<Item>();
                    item.Init(Item.Type.SPEAR);
                    item.Drop();
                }
            }
        }
        else if (type == 1)
        {
            if (UnityEngine.Random.Range(0,1f) < 0.3f)
            {
                Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
                GameObject itemGO = (GameObject)GameObject.Instantiate(Item.allItemModels [Item.Type.PICKAXE], gameObject.transform.position + randomOffset, Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0,360), 0)));
                Item item = itemGO.GetComponent<Item>();
                item.Init(Item.Type.PICKAXE);
                item.Drop();
            }
        }

        if (UnityEngine.Random.Range(0,1f) < 0.1f)
        {
            Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
            GameObject itemGO = (GameObject)GameObject.Instantiate(Item.allItemModels [Item.Type.MEAT], gameObject.transform.position + randomOffset, Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0,360), 0)));
            Item item = itemGO.GetComponent<Item>();
            item.Init(Item.Type.MEAT);
            item.Drop();
        }
    }
}
    