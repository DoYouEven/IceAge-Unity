using UnityEngine;
using System.Collections.Generic;

public class Tile : MonoBehaviour{
   

    public static Dictionary<Type,GameObject> allTileModels; 

    public Block block;

    public enum Type{
        STONE,
        DIRT,
    }
    
    public Type type;
    
    public Tile (Type type)
    {
        this.type = type;
    }

    public void spawn(Vector3 position)
    {
        GameObject model = allTileModels [type];
        model = (GameObject)GameObject.Instantiate(model, position, Quaternion.Euler(new Vector3(0, 0, 0)));

    }

    public static void loadResources()
    {
        allTileModels = new Dictionary<Type, GameObject>();

        GameObject model; //temp varaible

        model = (GameObject)Resources.Load("tile_stone", typeof(GameObject));
        allTileModels.Add(Type.STONE, model);

        model = (GameObject)Resources.Load("tile_dirt", typeof(GameObject));
        allTileModels.Add(Type.DIRT, model);
    }
}
