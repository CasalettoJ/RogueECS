using ECSRogue.ProceduralGeneration.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ECSRogue.BaseEngine;

namespace ECSRogue.ProceduralGeneration
{
    public class CaveGeneration : IGenerationAlgorithm
    {
        public DungeonColorInfo GetColorInfo()
        {
            return new DungeonColorInfo()
            {
                Floor = Colors.Caves.Floor,
                Wall = Colors.Caves.Wall,
                FloorInRange = Colors.Caves.FloorInRange,
                WallInRange = Colors.Caves.WallInRange,
                //Test Colors
                DeepWater = Colors.Caves.DeepWater,
                DeepWaterInRange = Colors.Caves.DeepWaterInRange,
                Spikes = Colors.Caves.Spikes,
                SpikesInRange = Colors.Caves.SpikesInRange,
                TallGrass = Colors.Caves.TallGrass,
                TallGrassInRange = Colors.Caves.TallGrassInRange,
                Water = Colors.Caves.Water,
                WaterInRange = Colors.Caves.WaterInRange
            };
        }

        #region TEST TILE TYPE GENERATIONS

        public void TallGrassGeneration(ref DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, Random random, List<Vector2> freeTiles)
        {
            int numberOfGrassPlumes = random.Next(10, 20); //Must change to a formula
            for(int i = 0; i < numberOfGrassPlumes; i++)
            {
                Vector2 tile = freeTiles[random.Next(0, freeTiles.Count)];

                int radius = random.Next(3,25); //Must change to formula
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

        public void WaterGeneration(ref DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, Random random, List<Vector2> freeTiles)
        {
            int numberOfLakes = random.Next(0, 11);
            int maxWidth = 30;
            int minWidth = 10;
            int maxHeight = 40;
            int minHeight = 10;

            for(int i = 0; i < numberOfLakes; i++)
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
                            else if (cellularLake[j + 1,k])
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

                            if(!cellularLake[j, k] && numAlive >= 5)
                            {
                                newMap[j, k] = true;
                            }
                            else if(cellularLake[j, k] && numAlive >= 4)
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
                        if (cellularLake[xStart+l, yStart+m] && dungeonGrid[l,m].Occupiable)
                        {
                            dungeonGrid[xStart+l, yStart+m].Type = TileType.TILE_WATER;
                            dungeonGrid[xStart+l, yStart+m].Symbol = Tiles.WaterSymbol;
                            dungeonGrid[xStart+l, yStart+m].SymbolColor = Tiles.WaterSymbolColor;
                            dungeonGrid[xStart + l, yStart + m].ChanceToIgnite = Tiles.WaterIgniteChance;
                        }
                    }
                }

            }

            
        }




        #endregion

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



            for (int i = 0; i < worldI; i++)
            {
                for(int j = 0; j < worldJ; j++)
                {
                    if(dungeonGrid[i,j].Type == TileType.TILE_FLOOR)
                    {
                        dungeonGrid[i, j].Occupiable = true;
                        dungeonGrid[i, j].ChanceToIgnite = Tiles.FloorIgniteChance;
                        freeTiles.Add(new Vector2(i, j));
                    }
                }
            }


            #region TEST GENERATION
            TallGrassGeneration(ref dungeonGrid, new Vector2(worldI, worldJ), random, freeTiles);
            WaterGeneration(ref dungeonGrid, new Vector2(worldI, worldJ), random, freeTiles);
            #endregion

            return new Vector2(worldI, worldJ);
        }

        public string GetDungeonSpritesheetFileName()
        {
            return DevConstants.Graphics.CavesSpriteSheet;
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
