using UnityEngine;
using System.Collections;
using System;
using Pathfinding;
public class LevelGenerator
{
    //public static int currentSeed = UnityEngine.Random.Range(0,100000);
    public static int defaultSeed = UnityEngine.Random.Range(0,100000);
    public static int currentSeed = defaultSeed;

    public static void generate(Level level, int width, int height)
    {
        GameObject sapien = (GameObject)Resources.Load("mob_homo_sapien", typeof(GameObject));
        GameObject flores = (GameObject)Resources.Load("mob_homo_floresiensis", typeof(GameObject));

        level.tiles = new Tile[width, height];
        level.block = new Block[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float perlinX, perlinY;
                perlinX = i * 0.7f;
                perlinY = j * 0.7f;

                GameObject go = (GameObject)GameObject.Instantiate(Tile.allTileModels[Tile.Type.STONE], new Vector3(i * 2, 0, j * 2), Quaternion.Euler(new Vector3(0, 0, 0)));
                Bounds btile = go.collider.bounds;
                //Pathfinding.Console.Write ("// Placing Object\n");
                GraphUpdateObject guo = new GraphUpdateObject(btile);
                AstarPath.active.UpdateGraphs(guo);
                level.tiles[i, j] = go.GetComponent<Tile>();

                Block.Type blockType;

                //GENERATE END GAME
                if (j == height-1 && i == width/2)
                {
                    for (int x = -width/2; x < width/2; x++)
                    {
                        for (int y = 0; y < 14; y++)
                        {
                            GameObject tile = (GameObject)GameObject.Instantiate(Tile.allTileModels[Tile.Type.STONE], new Vector3((x+i) * 2, 0, (y+j) * 2), Quaternion.Euler(new Vector3(0, 0, 0)));

                        }
                    }

                    continue;
                }

                if (j > 0 && j < 4 && i > width/2 - 2 && i < width/2 + 2)
                    continue;

                
                if (i == 0 || i == width-1 || j == 0 || j == height-1)
                    blockType = Block.Type.BORDER;
                else
                    blockType = Block.Type.STONE;

                currentSeed = defaultSeed;
                if (blockType == Block.Type.BORDER || perlinNoise(perlinX,perlinY,0.6f) > -0.2f)
                {
                    GameObject block = (GameObject)GameObject.Instantiate(Block.allBlockModels[blockType], new Vector3(i * 2, 0, j * 2), Quaternion.Euler(new Vector3(0, 0, 0)));
                    
                    Bounds b = block.collider.bounds;
                    //Pathfinding.Console.Write ("// Placing Object\n");
                    GraphUpdateObject guo1 = new GraphUpdateObject(b);
                    AstarPath.active.UpdateGraphs(guo1);
                    level.block[i, j] = block.GetComponent<Block>();
                    level.block[i, j].Init(blockType, level, i, j);
                }
                else
                {
                    if (UnityEngine.Random.Range(0,1f) < 0.04f)
                    {
                        currentSeed = defaultSeed + 123;

                        if (perlinNoise(perlinX,perlinY,0.2f) > 0.0f)
                        {
                            GameObject npc = (GameObject)GameObject.Instantiate(sapien, new Vector3(i * 2, 0, j * 2),Quaternion.identity);
                            Sapien mob = npc.GetComponent<Sapien>();
                            mob.type = 0;
                            mob.level = level;
                            level.allMobs.Add(mob);
                        }
                        else
                        {
                            GameObject npc = (GameObject)GameObject.Instantiate(flores, new Vector3(i * 2, 0, j * 2),Quaternion.identity);
                            Sapien mob = npc.GetComponent<Sapien>();
                            mob.type = 1;
                            mob.level = level;
                            level.allMobs.Add(mob);
                        }
                    }
                }
            }
        }

        OnGenerateMap(level,width,height);
    }


    public static void OnGenerateMap(Level level, int width, int height)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                level.removeEdges(i, j);
            }
        }
    }
    /**
     * Returns a pseudo random number between -1.0 to 1.0
     * Has 6 levels of octaves which generate different numbers
     */ 
    public static float noise (int x, int y, int octave)
    {
        int n = x + y * (57+currentSeed);
        n = (n << 13) ^ n;
        
        if (octave == 0)
            return (float)(1.0 - ((n * ((n * n * 19571) + 576377) + 1376312589) & 0x7fffffff) / 1073741824.0);
        else if (octave == 1)
            return (float)(1.0 - ((n * ((n * n * 22123) + 746939) + 1841957833) & 0x7fffffff) / 1073741824.0);
        else if (octave == 2)
            return (float)(1.0 - ((n * ((n * n * 23623) + 365147) + 1419576371) & 0x7fffffff) / 1073741824.0);
        else if (octave == 3)
            return (float)(1.0 - ((n * ((n * n * 25031) + 856343) + 1519576381) & 0x7fffffff) / 1073741824.0);
        else if (octave == 4)
            return (float)(1.0 - ((n * ((n * n * 20269) + 277427) + 1119515899) & 0x7fffffff) / 1073741824.0);
        else 
            return (float)(1.0 - ((n * ((n * n * 32609) + 793379) + 1000085861) & 0x7fffffff) / 1073741824.0);      
    }   
    
    /**
     * Cosine interpolation that smoothes the point x inbetween point a and b
     */ 
    public static float interpolate (float a, float b, float x)
    {
        float f = (float)(1 - Math.Cos (x * Math.PI)) * 0.5f;
        return (a * (1 - f)) + (b * f);
    }
    
    /**
     * Smoothes the noise in 2D
     */ 
    public static float smoothNoise2D (int a, int b, int octave)
    {
        
        float corners = (noise (a - 1, b - 1, octave) + noise (a + 1, b - 1, octave) + noise (a - 1, b + 1, octave) + noise (a + 1, b + 1, octave))/16;
        float sides = (noise (a - 1, b, octave) + noise (a + 1, b, octave) + noise (a, b - 1, octave) + noise (a, b + 1, octave))/8;
        float center = noise (a, b, octave)/4;
        return corners + sides + center;
        //return noise (a,b,octave);
    }
    
    public static float interpolatedNoise(float x, float y, int octave)
    {
        int intX = (int)x;
        //int intX = (int)Math.Round(x);
        float fractionalX;
        if (x > 0)
            fractionalX = x - intX;
        else
            fractionalX = 1-(x - intX);
        
        int intY = (int)y;
        //int intY = (int)Math.Round(y);
        float fractionalY;
        if (y > 0)
            fractionalY = y - intY;
        else
            fractionalY = 1-(y - intY);
        
        float v1 = smoothNoise2D (intX, intY, octave);
        float v2 = smoothNoise2D (intX + 1, intY, octave);
        float v3 = smoothNoise2D (intX, intY + 1, octave);
        float v4 = smoothNoise2D (intX + 1, intY + 1, octave);
        
        return interpolate (interpolate(v1, v2, fractionalX), interpolate(v3, v4, fractionalX), fractionalY);
    }
    
    /**
     * A 2D perlin noise function at (x,y), with a persistence variable, p.
     * p -> 1 results in very spiky noise, while p -> 0 results in flat noise
     */ 
    public static float perlinNoise(float x, float y, float p)
    {
        float total = 0;
        
        //6 octaves
        for (int i = 0; i < 6; i++)
        {
            float f = (float)Math.Pow(1.1f,i);
            float a = (float)Math.Pow(p,i);
            
            total += interpolatedNoise(x * f, y * f, i) * a;
        }
        return total;
        //return (total + 1f) / 2;
    }
}
