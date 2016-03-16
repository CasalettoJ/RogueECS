using ECSRogue.BaseEngine;
using ECSRogue.ECS.Components.ItemizationComponents;
using ECSRogue.ProceduralGeneration;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class DungeonCreationSystem
    {
        public static void CreateDungeonMonsters(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, int cellsize, List<Vector2> freeTiles)
        {
            int numberOfSpawns = 15; //Should be a formula based on depth level once a formula has been decided
            List<MonsterInfo> monsterPossibilities = new List<MonsterInfo>();
            //populate the monster possibility array based on how many slots a monster gets
            foreach (MonsterInfo monster in Monsters.MonsterCatalog.Where(x => x.SpawnDepthsAndChances.ContainsKey(spaceComponents.GameplayInfoComponent.FloorsReached)))
            {
                for (int i = 0; i < monster.SpawnDepthsAndChances[spaceComponents.GameplayInfoComponent.FloorsReached]; i++)
                {
                    monsterPossibilities.Add(monster);
                }
            }
            //Roll randomly in the index and add whatever monster it lands on
            if(monsterPossibilities.Count > 0)
            {
                for (int i = 0; i < numberOfSpawns; i++)
                {
                    monsterPossibilities[spaceComponents.random.Next(0, monsterPossibilities.Count)].SpawnFunction(spaceComponents, dungeonGrid, dungeonDimensions, cellsize, freeTiles);
                }
            }

            foreach (MonsterInfo monster in Monsters.MonsterCatalog.Where(x => x.IsRequiredSpawn))
            {
                monster.SpawnFunction(spaceComponents, dungeonGrid, dungeonDimensions, cellsize, freeTiles);
            }
        }

        public static void CreateDungeonDrops(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, List<Vector2> freeTiles)
        {
            //These should be a formula based on level once a formula has been decided.
            int numberOfArtifacts = 5;
            int numberOfConsumablesAndGold = 25;
            List<ItemInfo> artifactPossibilities = new List<ItemInfo>();
            List<ItemInfo> consumableAndGoldPossibilities = new List<ItemInfo>();

            foreach (ItemInfo item in Items.ItemCatalog.Where(x => x.SpawnDepthsAndChances.ContainsKey(spaceComponents.GameplayInfoComponent.FloorsReached)))
            {
                for (int i = 0; i < item.SpawnDepthsAndChances[spaceComponents.GameplayInfoComponent.FloorsReached]; i++)
                {
                    switch(item.DropType)
                    {
                        case ItemType.ARTIFACT:
                            artifactPossibilities.Add(item);
                            break;
                        case ItemType.CONSUMABLE:
                        case ItemType.GOLD:
                            consumableAndGoldPossibilities.Add(item);
                            break;
                    }
                }
            }

            //Spawn Artifacts
            if(artifactPossibilities.Count > 0)
            {
                for(int i = 0; i <= numberOfArtifacts; i++)
                {
                    artifactPossibilities[spaceComponents.random.Next(0, artifactPossibilities.Count)].SpawnFunction(spaceComponents, dungeonGrid, dungeonDimensions, freeTiles);
                }
            }

            //Spawn gold and consumables
            if(consumableAndGoldPossibilities.Count > 0)
            {
                for(int i = 0; i <= numberOfConsumablesAndGold; i++)
                {
                    consumableAndGoldPossibilities[spaceComponents.random.Next(0, consumableAndGoldPossibilities.Count)].SpawnFunction(spaceComponents, dungeonGrid, dungeonDimensions, freeTiles);
                }
            }

            foreach (ItemInfo item in Items.ItemCatalog.Where(x => x.IsRequiredSpawn))
            {
                item.SpawnFunction(spaceComponents, dungeonGrid, dungeonDimensions, freeTiles);
            }

        }

        public static void WaterGeneration(ref DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, Random random, List<Vector2> freeTiles, StateSpaceComponents spaceComponents)
        {
            int floorNumber = spaceComponents.GameplayInfoComponent.FloorsReached;
            floorNumber = floorNumber > 5 ? 5 : floorNumber;
            int numberOfLakes = random.Next(0, floorNumber +1);
            int maxWidth = 40;
            int minWidth = 25;
            int maxHeight = 35;
            int minHeight = 20;

            for (int i = 0; i < numberOfLakes; i++)
            {
                int xStart = random.Next(0, (int)dungeonDimensions.X - maxWidth);
                int yStart = random.Next(0, (int)dungeonDimensions.Y - maxHeight);
                int width = random.Next(minWidth, maxWidth);
                int height = random.Next(minHeight, maxHeight);

                bool[,] cellularLake = new bool[(int)dungeonDimensions.X, (int)dungeonDimensions.Y];
                //Initial Seed
                for (int j = 0; j < (int)dungeonDimensions.X; j++)
                {
                    for (int k = 0; k < (int)dungeonDimensions.Y; k++)
                    {
                        cellularLake[j, k] = random.Next(0, 101) <= 55;
                    }
                }

                for (int z = 0; z < 5; z++)
                {
                    bool[,] newMap = new bool[(int)dungeonDimensions.X, (int)dungeonDimensions.Y];

                    for (int j = 0; j < (int)dungeonDimensions.X; j++)
                    {
                        for (int k = 0; k < (int)dungeonDimensions.Y; k++)
                        {
                            int numAlive = 0;
                            int worldI = (int)dungeonDimensions.X;
                            int worldJ = (int)dungeonDimensions.Y;
                            //Check 8 directions and self
                            //Self:
                            //if (cellularLake[j,k])
                            //{
                            //    numAlive += 1;
                            //}
                            //Topleft
                            if (j - 1 < 0 || k - 1 < 0)
                            {
                                numAlive += 1;
                            }
                            else if (cellularLake[j - 1, k - 1])
                            {
                                numAlive += 1;
                            }
                            //Top
                            if (k - 1 < 0)
                            {
                                numAlive += 1;
                            }
                            else if (cellularLake[j, k - 1])
                            {
                                numAlive += 1;
                            }
                            //Topright
                            if (j + 1 > worldI - 1 || k - 1 < 0)
                            {
                                numAlive += 1;
                            }
                            else if (cellularLake[j + 1, k - 1])
                            {
                                numAlive += 1;
                            }
                            //Left
                            if (j - 1 < 0)
                            {
                                numAlive += 1;
                            }
                            else if (cellularLake[j - 1, k])
                            {
                                numAlive += 1;
                            }
                            //Right
                            if (j + 1 > worldI - 1)
                            {
                                numAlive += 1;
                            }
                            else if (cellularLake[j + 1, k])
                            {
                                numAlive += 1;
                            }
                            //Bottomleft
                            if (j - 1 < 0 || k + 1 > worldJ - 1)
                            {
                                numAlive += 1;
                            }
                            else if (cellularLake[j - 1, k + 1])
                            {
                                numAlive += 1;
                            }
                            //Bottom
                            if (k + 1 > worldJ - 1)
                            {
                                numAlive += 1;
                            }
                            else if (cellularLake[j, k + 1])
                            {
                                numAlive += 1;
                            }
                            //BottomRight
                            if (j + 1 > worldI - 1 || k + 1 > worldJ - 1)
                            {
                                numAlive += 1;
                            }
                            else if (cellularLake[j + 1, k + 1])
                            {
                                numAlive += 1;
                            }

                            if (!cellularLake[j, k] && numAlive >= 5)
                            {
                                newMap[j, k] = true;
                            }
                            else if (cellularLake[j, k] && numAlive >= 4)
                            {
                                newMap[j, k] = true;
                            }
                            else
                            {
                                newMap[j, k] = false;
                            }
                        }
                    }

                    Array.Copy(newMap, cellularLake, (int)dungeonDimensions.X * (int)dungeonDimensions.Y);

                }

                for (int l = 0; l <= width; l++)
                {
                    for (int m = 0; m < height; m++)
                    {
                        if (cellularLake[xStart + l, yStart + m] && dungeonGrid[l, m].Occupiable)
                        {
                            dungeonGrid[xStart + l, yStart + m].Type = TileType.TILE_WATER;
                            dungeonGrid[xStart + l, yStart + m].Symbol = Tiles.WaterSymbol;
                            dungeonGrid[xStart + l, yStart + m].SymbolColor = Tiles.WaterSymbolColor;
                            dungeonGrid[xStart + l, yStart + m].ChanceToIgnite = Tiles.WaterIgniteChance;
                        }
                    }
                }

            }


        }

        public static void TallGrassGeneration(ref DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, Random random, List<Vector2> freeTiles, StateSpaceComponents spaceComponents)
        {
            int floorNumber = spaceComponents.GameplayInfoComponent.FloorsReached;
            floorNumber = floorNumber > 10 ? 10 : floorNumber;
            int numberOfGrassPlumes = random.Next(1, 5 + floorNumber); //Must change to a formula
            for (int i = 0; i < numberOfGrassPlumes; i++)
            {
                Vector2 tile = freeTiles[random.Next(0, freeTiles.Count)];

                int radius = random.Next(3, 6 + floorNumber); //Must change to formula
                int initialX, x0, initialY, y0;
                initialX = x0 = (int)tile.X;
                initialY = y0 = (int)tile.Y;

                List<Vector2> visionRange = new List<Vector2>();

                int x = radius;
                int y = 0;
                int decisionOver2 = 1 - x;   // Decision criterion divided by 2 evaluated at x=r, y=0

                while (y <= x)
                {
                    if (-x + x0 >= 0 && -y + y0 >= 0)
                    {
                        // Octant 5
                        visionRange.Add(new Vector2(-x + x0, -y + y0));
                    }
                    else
                    {
                        int newX = -x + x0 >= 0 ? -x + x0 : 0;
                        int newY = -y + y0 >= 0 ? -y + y0 : 0;
                        visionRange.Add(new Vector2(newX, newY));
                    }
                    if (-y + x0 >= 0 && -x + y0 >= 0)
                    {
                        // Octant 6
                        visionRange.Add(new Vector2(-y + x0, -x + y0));
                    }
                    else
                    {
                        int newX = -y + x0 >= 0 ? -y + x0 : 0;
                        int newY = -x + y0 >= 0 ? -x + y0 : 0;
                        visionRange.Add(new Vector2(newX, newY));
                    }

                    if (x + x0 < dungeonDimensions.X && -y + y0 >= 0)
                    {
                        // Octant 8
                        visionRange.Add(new Vector2(x + x0, -y + y0));
                    }
                    else
                    {
                        int newX = x + x0 < dungeonDimensions.X ? x + x0 : (int)dungeonDimensions.X - 1;
                        int newY = -y + y0 >= 0 ? -y + y0 : 0;
                        visionRange.Add(new Vector2(newX, newY));
                    }
                    if (y + x0 < dungeonDimensions.X && -x + y0 >= 0)
                    {
                        // Octant 7
                        visionRange.Add(new Vector2(y + x0, -x + y0));
                    }
                    else
                    {
                        int newX = y + x0 < dungeonDimensions.X ? y + x0 : (int)dungeonDimensions.X - 1;
                        int newY = -x + y0 >= 0 ? -x + y0 : 0;
                        visionRange.Add(new Vector2(newX, newY));
                    }

                    if (x + x0 < dungeonDimensions.X && y + y0 < dungeonDimensions.Y)
                    {
                        // Octant 1
                        visionRange.Add(new Vector2(x + x0, y + y0));
                    }
                    else
                    {
                        int newX = x + x0 < dungeonDimensions.X ? x + x0 : (int)dungeonDimensions.X - 1;
                        int newY = y + y0 < dungeonDimensions.Y ? y + y0 : (int)dungeonDimensions.Y - 1;
                        visionRange.Add(new Vector2(newX, newY));
                    }
                    if (y + x0 < dungeonDimensions.X && x + y0 < dungeonDimensions.Y)
                    {
                        // Octant 2
                        visionRange.Add(new Vector2(y + x0, x + y0));
                    }
                    else
                    {
                        int newX = y + x0 < dungeonDimensions.X ? y + x0 : (int)dungeonDimensions.X - 1;
                        int newY = x + y0 < dungeonDimensions.Y ? x + y0 : (int)dungeonDimensions.Y - 1;
                        visionRange.Add(new Vector2(newX, newY));
                    }

                    if (-y + x0 >= 0 && x + y0 < dungeonDimensions.Y)
                    {
                        // Octant 3
                        visionRange.Add(new Vector2(-y + x0, x + y0));
                    }
                    else
                    {
                        int newX = -y + x0 >= 0 ? -y + x0 : 0;
                        int newY = x + y0 < dungeonDimensions.Y ? x + y0 : (int)dungeonDimensions.Y - 1;
                        visionRange.Add(new Vector2(newX, newY));
                    }
                    if (-x + x0 >= 0 && y + y0 < dungeonDimensions.Y)
                    {
                        // Octant 4
                        visionRange.Add(new Vector2(-x + x0, y + y0));
                    }
                    else
                    {
                        int newX = -x + x0 >= 0 ? -x + x0 : 0;
                        int newY = y + y0 < dungeonDimensions.Y ? y + y0 : (int)dungeonDimensions.Y - 1;
                        visionRange.Add(new Vector2(newX, newY));
                    }

                    y++;

                    if (decisionOver2 <= 0)
                    {
                        decisionOver2 += 2 * y + 1;   // Change in decision criterion for y -> y+1
                    }
                    else
                    {
                        x--;
                        decisionOver2 += 2 * (y - x) + 1;   // Change for y -> y+1, x -> x-1
                    }
                }

                //Fill the circle
                foreach (var visionLine in visionRange.GroupBy(z => z.Y))
                {
                    int smallestX = -1;
                    int largestX = -1;
                    foreach (var point in visionLine)
                    {
                        smallestX = smallestX == -1 ? (int)point.X : smallestX;
                        largestX = largestX == -1 ? (int)point.X : largestX;
                        if ((int)point.X < smallestX)
                        {
                            smallestX = (int)point.X;
                        }
                        if ((int)point.X > largestX)
                        {
                            largestX = (int)point.X;
                        }
                    }
                    //Build a line of points from smallest to largest x
                    for (int z = smallestX; z <= largestX; z++)
                    {
                        visionRange.Add(new Vector2(z, visionLine.Key));
                    }
                }

                foreach (Vector2 point in visionRange)
                {
                    x0 = initialX;
                    y0 = initialY;

                    int dx = Math.Abs((int)point.X - x0), sx = x0 < (int)point.X ? 1 : -1;
                    int dy = -Math.Abs((int)point.Y - y0), sy = y0 < (int)point.Y ? 1 : -1;
                    int err = dx + dy, e2; /* error value e_xy */

                    for (;;)
                    {  /* loop */
                        if (dungeonGrid[x0, y0].Occupiable)
                        {
                            dungeonGrid[x0, y0].Type = TileType.TILE_TALLGRASS;
                            dungeonGrid[x0, y0].Symbol = Tiles.TallGrassSymbol;
                            dungeonGrid[x0, y0].SymbolColor = Tiles.TallGrassSymbolColor;
                            dungeonGrid[x0, y0].ChanceToIgnite = Tiles.TallGrassIgniteChange;
                        }
                        else
                        {
                            break;
                        }

                        if (x0 == (int)point.X && y0 == (int)point.Y) break;
                        e2 = 2 * err;
                        if (e2 >= dy) { err += dy; x0 += sx; } /* e_xy+e_x > 0 */
                        if (e2 <= dx) { err += dx; y0 += sy; } /* e_xy+e_y < 0 */
                    }


                }

            }
        }
    }
}
