using UnityEngine;
using System.Collections.Generic;

public class GameMain : MonoBehaviour {

    public enum Sprite
    {
        PICKAXE_0,
        PICKAXE_1,
        SPEAR,
        MEAT,
        LIFE_BAR_LARGE_0,
        LIFE_BAR_LARGE_1,
        LIFE_BAR_LARGE_2,
        LIFE_BAR_LARGE_FILL,
    }

    public static Dictionary<Sprite, Texture2D> allSprites;


    public Player player;
    public Level level;
    public AudioManager audioManager;
	// Use this for initialization
	void Start () {

        loadResources();
        level = new Level(player);
        audioManager.PlayAudio(4);
	}  

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(Screen.width - 300, Screen.height - 49 - 20, 49, 49), allSprites [Sprite.MEAT]);
        GUI.Label(new Rect(Screen.width - 300 + 50, Screen.height - 50 - 20, 49, 49), player.MeatCount+"");
        GUI.Label(new Rect(Screen.width - 300 + 20, Screen.height - 20, 49, 49), "Q");

        if (player.PickaxeDurability == 0)
        {
            GUI.DrawTexture(new Rect(Screen.width - 200, Screen.height - 49 - 20, 49, 49), allSprites [Sprite.PICKAXE_0]);
        }
        else
        {
            GUI.DrawTexture(new Rect(Screen.width - 200, Screen.height - 49 - 20, 49, 49), allSprites [Sprite.PICKAXE_1]);
        }
        GUI.Label(new Rect(Screen.width - 200 + 15, Screen.height - 20, 49, 49), "LMB");


        GUI.DrawTexture(new Rect(Screen.width - 100, Screen.height - 49 - 20, 49, 49), allSprites [Sprite.SPEAR]);
        GUI.Label(new Rect(Screen.width - 100 + 50, Screen.height - 50 - 20, 49, 49), player.SpearCount+"");
        GUI.Label(new Rect(Screen.width - 100 + 15, Screen.height - 20, 49, 49), "RMB");


        Rect playerLifeRect = new Rect(8, Screen.height - 8 - 32, 400, 32);

        //background
        GUI.DrawTexture(new Rect(playerLifeRect.x, playerLifeRect.y, 4, playerLifeRect.height), allSprites [Sprite.LIFE_BAR_LARGE_0]);
        GUI.DrawTexture(new Rect(playerLifeRect.x + 4 , playerLifeRect.y, playerLifeRect.width - 8 , playerLifeRect.height), allSprites [Sprite.LIFE_BAR_LARGE_1]);
        GUI.DrawTexture(new Rect(playerLifeRect.x + playerLifeRect.width - 4 , playerLifeRect.y, 4 , playerLifeRect.height), allSprites [Sprite.LIFE_BAR_LARGE_2]);
        
        //life
        float playerLifePercent = (float)player.hp / BaseStats.BASEPLAYERHP;
        playerLifePercent = playerLifePercent < 0 ? 0 : playerLifePercent;
        GUI.DrawTexture(new Rect(playerLifeRect.x + 2, playerLifeRect.y, (playerLifeRect.width - 4)*playerLifePercent, playerLifeRect.height), allSprites [Sprite.LIFE_BAR_LARGE_FILL]);
    }
	
	// Update is called once per frame
	void Update () {
        level.update();
	}

    public static void loadResources()
    {
        Tile.loadResources();
        Block.loadResources();
        Item.loadResources();

        allSprites = new Dictionary<Sprite, Texture2D>();

        allSprites.Add(Sprite.SPEAR, (Texture2D)Resources.Load("icon_spear", typeof(Texture2D)));
        allSprites.Add(Sprite.MEAT, (Texture2D)Resources.Load("icon_meat", typeof(Texture2D)));
        allSprites.Add(Sprite.PICKAXE_0, (Texture2D)Resources.Load("icon_pickaxe_0", typeof(Texture2D)));
        allSprites.Add(Sprite.PICKAXE_1, (Texture2D)Resources.Load("icon_pickaxe_1", typeof(Texture2D)));

        allSprites.Add(Sprite.LIFE_BAR_LARGE_0, (Texture2D)Resources.Load("life_bar_large_0", typeof(Texture2D)));
        allSprites.Add(Sprite.LIFE_BAR_LARGE_1, (Texture2D)Resources.Load("life_bar_large_1", typeof(Texture2D)));
        allSprites.Add(Sprite.LIFE_BAR_LARGE_2, (Texture2D)Resources.Load("life_bar_large_2", typeof(Texture2D)));
        allSprites.Add(Sprite.LIFE_BAR_LARGE_FILL, (Texture2D)Resources.Load("life_bar_large_life", typeof(Texture2D)));

        Mob.spearProjectile = (GameObject)Resources.Load("projectile_spear", typeof(GameObject));
    }
}
