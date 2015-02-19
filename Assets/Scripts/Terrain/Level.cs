using UnityEngine;
using System.Collections.Generic;
using System;

public class Level{

    public int width, height;
    public Tile[,] tiles;
    public Block[,] block;
    public List<Mob> allMobs;

    public Level(Player player)
    {
        width = 32;
        height = 128;


        allMobs = new List<Mob>();
        allMobs.Add(player);
        player.level = this;

        LevelGenerator.generate(this, width, height);
    }

    public void update()
    {

    }

    public void attack(Mob attacker)
    {
        GameObject attackCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        attackCube.transform.position = attacker.gameObject.transform.position + attacker.gameObject.transform.forward * 2 / 2;
        attackCube.transform.rotation = attacker.gameObject.transform.rotation;
        attackCube.transform.localScale = new Vector3(1, 100, 2);
        attackCube.collider.isTrigger = true;
        attackCube.renderer.enabled = false;
        
        for (int i = 0; i < allMobs.Count; i++)
        {
            Mob mob = allMobs [i];

            if (!mob.isAlive)
                continue;

            if (attacker.isPlayer)
            {
                if (attackCube.collider.bounds.Intersects(mob.gameObject.collider.bounds))
                {
                    if (attacker is Player && !(mob is Player))
                    {
                        mob.Damage(BaseStats.BASEPLAYERATK);
                    }
                }
            }
            else
            {
                float distance = (attacker.gameObject.transform.position - mob.gameObject.transform.position).magnitude;
                
                if ((mob is Player) && distance < 2)
                {
                    
                    mob.Damage(15);
                }
            }
        }
        
        GameObject.Destroy(attackCube);
    }
    
    protected void removeSingleEdge(int i, int j, bool west, bool south)
    {
        
        
        int addX = west ? -1 : 1;
        int addZ = south ? -1 : 1;
        String eastWest = west ? "west" : "east";
        String northSouth = south ? "south" : "north";

        if (i + addX < 0 || i + addX >= width)
        	addX = 0;
        if (j + addZ < 0 || j + addZ >= height)
        	addZ = 0;

        bool leftRight = block[i + addX, j] == null;
        bool upDown = block [i, j + addZ] == null;
        bool diag = block [i + addX, j + addZ] == null;

        if (leftRight && upDown && diag)
        {
            if (block[i,j].type != Block.Type.BORDER)
            {
                GameObject go = block[i,j].gameObject.transform.FindChild("block_stone_" + northSouth + "_" + eastWest).gameObject;
                go.SetActive(false);
            }
        }
    }

    public void removeEdges(int i, int j)
    {
        if(i >=0 && i < width && j >=0 && j < height 
           && block[i,j] != null)
        {
            for (int k = 0; k < 4; k++) {
                bool west = k == 0 || k == 3;
                bool south = k > 1;
                removeSingleEdge(i, j, west, south);
            }
        }
    }

    public void destroyBlock(int i, int j)
    {
        block[i, j] = null;

        for (int k = 0; k < 9; k++)
        {
            int neighbourI = (i-1) + k % 3;
            int neighbourJ = (j-1) + k / 3;
            removeEdges(neighbourI, neighbourJ);
        }
    }
}
