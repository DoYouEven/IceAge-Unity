using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
public class Block : MonoBehaviour {

    public static Dictionary<Type, GameObject> allBlockModels;
    public static GameObject blockDestroy;

    public float health;
    public bool alive;
    public int i, j;
    public Level level;
    public GameObject effect;
    public bool destroyed;
    public enum Type{
        STONE,
        BORDER,
    }
	
    public Type type;

    public Block (Type type)
    {
        this.type = type;
        this.alive = false;
    }
    public void Init(Type type, Level level, int i, int j)
    {
        this.type = type;
        this.alive = true;
        this.level = level;
        this.i = i;
        this.j = j;
    }
    void Awake()
    {
        health = BaseStats.BASEBLOCKHP;
    }
    public void Damage(float damage)
    {
        if (type == Type.BORDER)
            return;

        health -= damage;

        //TODO: need to find all childs and do this, but also gotta change renderer back after a delay
        /*
           Color c =  gameObject.renderer.material.color;
          c.b  = Mathf.Max(0,health/BaseStats.BASEBLOCKHP);
          c.g = Mathf.Max(0, health / BaseStats.BASEBLOCKHP);
         gameObject.renderer.material.color= c;

        */
        
        if(health<=0)
        {
            Destroy();
        }
    }
    public void Destroy()
    {
        if (destroyed)
            return;
        destroyed = true;
        level.destroyBlock(i, j);
        //GameObject newEffect = (GameObject)Instantiate(effect, transform.position, Quaternion.identity);
        GameObject particle = (GameObject)Instantiate(blockDestroy, transform.position + new Vector3(0,4,0), Quaternion.identity); 

        if (UnityEngine.Random.Range(0, 1f) < 0.05f)
        {
            Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
            GameObject itemGO = (GameObject)GameObject.Instantiate(Item.allItemModels [Item.Type.MEAT], gameObject.transform.position + randomOffset, Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360), 0)));
            Item item = itemGO.GetComponent<Item>();
            item.Init(Item.Type.MEAT);
            item.Drop();
        }

        Bounds b = gameObject.collider.bounds;
	
			
			//Pathfinding.Console.Write ("// Placing Object\n");
			
				GraphUpdateObject guo = new GraphUpdateObject(b);
				AstarPath.active.UpdateGraphs (guo,0.0f);
                AstarPath.active.FlushGraphUpdates();

        Destroy(this.gameObject,0.1f);

        //TODO drop item
    }
    public static void loadResources()
    {
        allBlockModels = new Dictionary<Type, GameObject>();

        GameObject model; //temp varaible

        model = (GameObject)Resources.Load("block_stone", typeof(GameObject));
        allBlockModels.Add(Type.STONE, model);

        model = (GameObject)Resources.Load("block_border", typeof(GameObject));
        allBlockModels.Add(Type.BORDER, model);

        blockDestroy = (GameObject)Resources.Load("block_break", typeof(GameObject));
    }
}
