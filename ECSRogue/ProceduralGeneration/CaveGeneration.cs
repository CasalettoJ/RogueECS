using ECSRogue.ProceduralGeneration.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ECSRogue.BaseEngine;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ECSRogue.ECS;
using ECSRogue.ECS.Systems;
using ECSRogue.ECS.Components;

namespace ECSRogue.ProceduralGeneration
{
    public class CaveGeneration : IGenerationAlgorithm
    {
        private const int cellSize = 32;

        public int GetCellsize()
        {
            return cellSize;
        }

        public DungeonColorInfo GetColorInfo()
        {
            return new DungeonColorInfo()
            {
                Floor = Color.DarkGreen,
                Wall = Color.DarkViolet,
                FloorInRange = Color.Green,
                WallInRange = Color.Violet
            };
        }

        public Vector2 GenerateDungeon(ref DungeonTile[,] dungeonGrid, int worldMin, int worldMax, Random random, List<Vector2> freeTiles)
        {
            int worldI = random.Next(worldMin, worldMax);
            int worldJ = random.Next(worldMin, worldMax);

            dungeonGrid = new DungeonTile[worldI, worldJ];

            bool acceptable = false;

            while (!acceptable)
            {
                //Cellular Automata
                for (int i = 0; i < worldI; i++)
                {
                    for (int j = 0; j < worldJ; j++)
                    {
                        int choice = random.Next(0, 101);
                        dungeonGrid[i, j].Type = (choice <= 41) ? TileType.TILE_ROCK : TileType.TILE_FLOOR;
                    }
                }

                int iterations = 8;
                for (int z = 0; z <= iterations; z++)
                {
                    DungeonTile[,] newMap = new DungeonTile[worldI, worldJ];
                    for (int i = 0; i < worldI; i++)
                    {
                        for (int j = 0; j < worldJ; j++)
                        {
                            int numRocks = 0;
                            int farRocks = 0;
                            //Check 8 directions and self
                            //Self:
                            if (dungeonGrid[i, j].Type == TileType.TILE_ROCK)
                            {
                                numRocks += 1;
                            }
                            //Topleft
                            if (i - 1 < 0 || j - 1 < 0)
                            {
                                numRocks += 1;
                            }
                            else if (dungeonGrid[i - 1, j - 1].Type == TileType.TILE_ROCK)
                            {
                                numRocks += 1;
                            }
                            //Top
                            if (j - 1 < 0)
                            {
                                numRocks += 1;
                            }
                            else if (dungeonGrid[i, j - 1].Type == TileType.TILE_ROCK)
                            {
                                numRocks += 1;
                            }
                            //Topright
                            if (i + 1 > worldI - 1 || j - 1 < 0)
                            {
                                numRocks += 1;
                            }
                            else if (dungeonGrid[i + 1, j - 1].Type == TileType.TILE_ROCK)
                            {
                                numRocks += 1;
                            }
                            //Left
                            if (i - 1 < 0)
                            {
                                numRocks += 1;
                            }
                            else if (dungeonGrid[i - 1, j].Type == TileType.TILE_ROCK)
                            {
                                numRocks += 1;
                            }
                            //Right
                            if (i + 1 > worldI - 1)
                            {
                                numRocks += 1;
                            }
                            else if (dungeonGrid[i + 1, j].Type == TileType.TILE_ROCK)
                            {
                                numRocks += 1;
                            }
                            //Bottomleft
                            if (i - 1 < 0 || j + 1 > worldJ - 1)
                            {
                                numRocks += 1;
                            }
                            else if (dungeonGrid[i - 1, j + 1].Type == TileType.TILE_ROCK)
                            {
                                numRocks += 1;
                            }
                            //Bottom
                            if (j + 1 > worldJ - 1)
                            {
                                numRocks += 1;
                            }
                            else if (dungeonGrid[i, j + 1].Type == TileType.TILE_ROCK)
                            {
                                numRocks += 1;
                            }
                            //BottomRight
                            if (i + 1 > worldI - 1 || j + 1 > worldJ - 1)
                            {
                                numRocks += 1;
                            }
                            else if (dungeonGrid[i + 1, j + 1].Type == TileType.TILE_ROCK)
                            {
                                numRocks += 1;
                            }

                            //Check 8 directions for far rocks

                            //Topleft
                            if (i - 2 < 0 || j - 2 < 0)
                            {
                                farRocks += 1;
                            }
                            else if (dungeonGrid[i - 2, j - 2].Type == TileType.TILE_ROCK)
                            {
                                farRocks += 1;
                            }
                            //Top
                            if (j - 2 < 0)
                            {
                                farRocks += 1;
                            }
                            else if (dungeonGrid[i, j - 2].Type == TileType.TILE_ROCK)
                            {
                                farRocks += 1;
                            }
                            //Topright
                            if (i + 2 > worldI - 2 || j - 2 < 0)
                            {
                                farRocks += 1;
                            }
                            else if (dungeonGrid[i + 2, j - 2].Type == TileType.TILE_ROCK)
                            {
                                farRocks += 1;
                            }
                            //Left
                            if (i - 2 < 0)
                            {
                                farRocks += 1;
                            }
                            else if (dungeonGrid[i - 2, j].Type == TileType.TILE_ROCK)
                            {
                                farRocks += 1;
                            }
                            //Right
                            if (i + 2 > worldI - 2)
                            {
                                farRocks += 1;
                            }
                            else if (dungeonGrid[i + 2, j].Type == TileType.TILE_ROCK)
                            {
                                farRocks += 1;
                            }
                            //Bottomleft
                            if (i - 2 < 0 || j + 2 > worldJ - 2)
                            {
                                farRocks += 1;
                            }
                            else if (dungeonGrid[i - 2, j + 2].Type == TileType.TILE_ROCK)
                            {
                                farRocks += 1;
                            }
                            //Bottom
                            if (j + 2 > worldJ - 2)
                            {
                                farRocks += 1;
                            }
                            else if (dungeonGrid[i, j + 2].Type == TileType.TILE_ROCK)
                            {
                                farRocks += 1;
                            }
                            //BottomRight
                            if (i + 2 > worldI - 2 || j + 2 > worldJ - 2 || dungeonGrid[i + 2, j + 2].Type == TileType.TILE_ROCK)
                            {
                                farRocks += 1;
                            }
                            else if (dungeonGrid[i + 2, j + 2].Type == TileType.TILE_ROCK)
                            {
                                farRocks += 1;
                            }


                            if (numRocks >= 5 || i == 0 || j == 0 || i == worldI - 1 || j == worldJ - 1 || (z <= 3 && numRocks + farRocks <= 2))
                            {
                                newMap[i, j].Type = TileType.TILE_ROCK;
                            }
                            else
                            {
                                newMap[i, j].Type = TileType.TILE_FLOOR;
                            }
                        }
                    }
                    Array.Copy(newMap, dungeonGrid, worldJ * worldI);
                }

                int fillX = 0;
                int fillY = 0;
                do
                {
                    fillX = random.Next(0, worldI);
                    fillY = random.Next(0, worldJ);
                } while (dungeonGrid[fillX, fillY].Type != TileType.TILE_FLOOR);

                this.FloodFill(fillX, fillY, worldI, worldJ, ref dungeonGrid);

                double connectedTiles = 0.0;
                double totalTiles = 0.0;
                for (int i = 0; i < worldI; i++)
                {
                    for (int j = 0; j < worldJ; j++)
                    {
                        if (dungeonGrid[fillX, fillY].Type == TileType.TILE_FLOOR)
                        {
                            totalTiles += 1.0;
                            if (dungeonGrid[i, j].Reached)
                            {
                                connectedTiles += 1.0;
                            }
                        }
                    }
                }

                if (connectedTiles / totalTiles >= .50)
                {
                    for (int i = 0; i < worldI; i++)
                    {
                        for (int j = 0; j < worldJ; j++)
                        {
                            if (!dungeonGrid[i, j].Reached)
                            {
                                dungeonGrid[i, j].Type = TileType.TILE_ROCK;
                            }
                        }
                    }
                    acceptable = true;
                }

            }

            //Mark Walls
            for (int i = 0; i < worldI; i++)
            {
                for (int j = 0; j < worldJ; j++)
                {
                    int numFloor = 0;
                    //Check 8 directions and self
                    //Self:
                    if (dungeonGrid[i, j].Type == TileType.TILE_ROCK)
                    {
                        //Topleft
                        if (!(i - 1 < 0 || j - 1 < 0) && dungeonGrid[i - 1, j - 1].Type == TileType.TILE_FLOOR)
                        {
                            numFloor += 1;
                        }
                        //Top
                        if (!(j - 1 < 0) && dungeonGrid[i, j - 1].Type == TileType.TILE_FLOOR)
                        {
                            numFloor += 1;
                        }
                        //Topright
                        if (!(i + 1 > worldI - 1 || j - 1 < 0) && dungeonGrid[i + 1, j - 1].Type == TileType.TILE_FLOOR)
                        {
                            numFloor += 1;
                        }
                        //Left
                        if (!(i - 1 < 0) && dungeonGrid[i - 1, j].Type == TileType.TILE_FLOOR)
                        {
                            numFloor += 1;
                        }
                        //Right
                        if (!(i + 1 > worldI - 1) && dungeonGrid[i + 1, j].Type == TileType.TILE_FLOOR)
                        {
                            numFloor += 1;
                        }
                        //Bottomleft
                        if (!(i - 1 < 0 || j + 1 > worldJ - 1) && dungeonGrid[i - 1, j + 1].Type == TileType.TILE_FLOOR)
                        {
                            numFloor += 1;
                        }
                        //Bottom
                        if (!(j + 1 > worldJ - 1) && dungeonGrid[i, j + 1].Type == TileType.TILE_FLOOR)
                        {
                            numFloor += 1;
                        }
                        //BottomRight
                        if (!(i + 1 > worldI - 1 || j + 1 > worldJ - 1) && dungeonGrid[i + 1, j + 1].Type == TileType.TILE_FLOOR)
                        {
                            numFloor += 1;
                        }

                        if (numFloor > 0)
                        {
                            dungeonGrid[i, j].Type = TileType.TILE_WALL;
                        }
                    }

                }
            }

            for(int i = 0; i < worldI; i++)
            {
                for(int j = 0; j < worldJ; j++)
                {
                    if(dungeonGrid[i,j].Type == TileType.TILE_FLOOR)
                    {
                        dungeonGrid[i, j].Occupiable = true;
                        freeTiles.Add(new Vector2(i, j));
                    }
                }
            }

            return new Vector2(worldI, worldJ);
        }

        public string GetDungeonSpritesheetFileName()
        {
            return "Sprites/Ball";
        }
        
        public void GenerateDungeonEntities(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, int cellsize, List<Vector2> freeTiles)
        {
            //Generate Monsters
            int numberOfMonsters = spaceComponents.random.Next(1, 100);
            for(int i = 0; i < numberOfMonsters; i++)
            {
                Guid id = spaceComponents.CreateEntity();
                spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.NPC;

                int tileIndex = spaceComponents.random.Next(0, freeTiles.Count);
                spaceComponents.PositionComponents[id] = new PositionComponent() { Position = freeTiles[tileIndex] };
                freeTiles.RemoveAt(tileIndex);
                spaceComponents.DisplayComponents[id] = new DisplayComponent()
                {
                    Color = Color.DarkRed,
                    Origin = Vector2.Zero,
                    Rotation = 0f,
                    Scale = 1f,
                    SpriteEffect = SpriteEffects.None,
                    SpriteSource = new Rectangle(0 * cellSize, 0 * cellSize, cellSize, cellSize),
                    Symbol = "t",
                    SymbolColor = Color.White
                };
                spaceComponents.SightRadiusComponents[id] = new SightRadiusComponent() { Radius = 3 };
                spaceComponents.SkillLevelsComponents[id] = new SkillLevelsComponent() { CurrentHealth = 10, DieNumber = 1, Health = 10, Power = 5, Defense = 1, Accuracy = 100, Wealth = 25, MinimumDamage = 1, MaximumDamage = 2 };
                spaceComponents.CollisionComponents[id] = new CollisionComponent() { Solid = true, CollidedObjects = new List<Guid>() };
                spaceComponents.NameComponents[id] = new NameComponent() { Name = "TEST ENEMY NPC" };
                spaceComponents.AIComponents[id] = new AIComponent()
                {
                    Alignment = AIAlignment.ALIGNMENT_HOSTILE,
                    AttackType = AIAttackType.ATTACK_TYPE_NORMAL,
                    FleesWhenLowHealth = true,
                    State = AIState.STATE_SLEEPING
                };
            }

            numberOfMonsters = spaceComponents.random.Next(1, 100);
            for (int i = 0; i < numberOfMonsters; i++)
            {
                Guid id = spaceComponents.CreateEntity();
                spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.NPC;

                int tileIndex = spaceComponents.random.Next(0, freeTiles.Count);
                spaceComponents.PositionComponents[id] = new PositionComponent() { Position = freeTiles[tileIndex] };
                freeTiles.RemoveAt(tileIndex);
                spaceComponents.DisplayComponents[id] = new DisplayComponent()
                {
                    Color = Color.DarkRed,
                    Origin = Vector2.Zero,
                    Rotation = 0f,
                    Scale = 1f,
                    SpriteEffect = SpriteEffects.None,
                    SpriteSource = new Rectangle(0 * cellSize, 0 * cellSize, cellSize, cellSize),
                    Symbol = "W",
                    SymbolColor = Color.White
                };
                spaceComponents.SightRadiusComponents[id] = new SightRadiusComponent() { Radius = 3 };
                spaceComponents.SkillLevelsComponents[id] = new SkillLevelsComponent() { CurrentHealth = 45, DieNumber = 2, Health = 45, Power = 5, Defense = 10, Accuracy = 135, Wealth = 25, MinimumDamage = 5, MaximumDamage = 14 };
                spaceComponents.CollisionComponents[id] = new CollisionComponent() { Solid = true, CollidedObjects = new List<Guid>() };
                spaceComponents.NameComponents[id] = new NameComponent() { Name = "WILD ROOTS" };
                spaceComponents.AIComponents[id] = new AIComponent()
                {
                    Alignment = AIAlignment.ALIGNMENT_HOSTILE,
                    AttackType = AIAttackType.ATTACK_TYPE_NORMAL,
                    FleesWhenLowHealth = true,
                    State = AIState.STATE_SLEEPING
                };
            }
        }

        private void FloodFill(int x, int y, int worldI, int worldJ, ref DungeonTile[,] dungeonGrid)
        {
            if (x < 0 || y < 0 || x >= worldI || y >= worldJ)
            {
                return;
            }

            Queue<Vector2> floodQueue = new Queue<Vector2>();
            floodQueue.Enqueue(new Vector2(x, y));

            while (floodQueue.Count > 0)
            {
                Vector2 pos = floodQueue.Dequeue();
                if (dungeonGrid[(int)pos.X, (int)pos.Y].Type == TileType.TILE_FLOOR && !dungeonGrid[(int)pos.X, (int)pos.Y].Reached)
                {
                    x = (int)pos.X;
                    y = (int)pos.Y;
                    dungeonGrid[(int)pos.X, (int)pos.Y].Reached = true;
                    HashSet<Vector2> toAdd = new HashSet<Vector2>();
                    toAdd.Add(new Vector2(x + 1, y + 1));
                    toAdd.Add(new Vector2(x, y + 1));
                    toAdd.Add(new Vector2(x + 1, y));
                    toAdd.Add(new Vector2(x - 1, y - 1));
                    toAdd.Add(new Vector2(x - 1, y));
                    toAdd.Add(new Vector2(x, y - 1));
                    toAdd.Add(new Vector2(x + 1, y - 1));
                    toAdd.Add(new Vector2(x - 1, y + 1));
                    foreach (var vector in toAdd)
                    {
                        if (!floodQueue.Contains(vector))
                        {
                            floodQueue.Enqueue(vector);
                        }
                    }
                    toAdd.Clear();
                }
            }

        }
    }
}
