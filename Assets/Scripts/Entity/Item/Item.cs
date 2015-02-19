using UnityEngine;
using System.Collections.Generic;

public class Item : Entity {

    public static Dictionary<Type, GameObject> allItemModels;
    public float TimeUntilPickup;

    public enum Type
    {
        PICKAXE,
        SPEAR,
        MEAT,
    }

    public Type type;

    public void Init(Type type)
    {
        this.type = type;
    }

	// Use this for initialization
	new void Start () {
        TimeUntilPickup = 1f;
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    void FixedUpdate(){
        if (TimeUntilPickup > 0)
            TimeUntilPickup -= Time.fixedTime;
    }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }

    public void Drop()
    {
        animation.Play("item_drop");
    }

    public static void loadResources()
    {
        allItemModels = new Dictionary<Type, GameObject>();

        GameObject model; //temp varaible
        
        model = (GameObject)Resources.Load("item_meat", typeof(GameObject));
        allItemModels.Add(Type.MEAT, model);
        model = (GameObject)Resources.Load("item_spear", typeof(GameObject));
        allItemModels.Add(Type.SPEAR, model);
        model = (GameObject)Resources.Load("item_pickaxe", typeof(GameObject));
        allItemModels.Add(Type.PICKAXE, model);
    }
}
