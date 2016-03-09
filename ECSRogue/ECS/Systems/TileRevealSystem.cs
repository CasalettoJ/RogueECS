using ECSRogue.ProceduralGeneration;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class TileRevealSystem
    {
        public static void RevealTiles(ref DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, StateSpaceComponents spaceComponents)
        {
            Entity player = spaceComponents.Entities.Where(z => (z.ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER).FirstOrDefault();
            if (player != null && (spaceComponents.PlayerComponent.PlayerTookTurn || spaceComponents.PlayerComponent.PlayerJustLoaded)) 
            {

                //Reset Range
                for (int i = 0; i < dungeonDimensions.X; i++)
                {
                    for (int j = 0; j < dungeonDimensions.Y; j++)
                    {
                        dungeonGrid[i, j].InRange = false;
                    }
                }

                foreach (Guid entity in spaceComponents.Entities.Where(c => (c.ComponentFlags & Component.COMPONENT_SIGHTRADIUS) == Component.COMPONENT_SIGHTRADIUS).Select(c => c.Id))
                {
                    if(!spaceComponents.SightRadiusComponents[entity].DrawRadius || spaceComponents.SightRadiusComponents[entity].CurrentRadius == 0)
                    {
                        continue;
                    }
                    //Guid entity = player.Id;
                    Vector2 position = spaceComponents.PositionComponents[entity].Position;
                    int radius =  dungeonGrid[(int)position.X, (int)position.Y].Type == TileType.TILE_TALLGRASS ? (int)spaceComponents.SightRadiusComponents[entity].CurrentRadius / 2 : spaceComponents.SightRadiusComponents[entity].CurrentRadius;
                    int initialX, x0, initialY, y0;
                    initialX = x0 = (int)position.X;
                    initialY = y0 = (int)position.Y;

                    List<Vector2> visionRange = new List<Vector2>();
                    /*
                                 Shared
                                 edge by
                      Shared     1 & 2      Shared
                      edge by\      |      /edge by
                      1 & 8   \     |     / 2 & 3
                               \1111|2222/
                               8\111|222/3
                               88\11|22/33
                               888\1|2/333
                      Shared   8888\|/3333  Shared
                      edge by-------@-------edge by
                      7 & 8    7777/|\4444  3 & 4
                               777/6|5\444
                               77/66|55\44
                               7/666|555\4
                               /6666|5555\
                      Shared  /     |     \ Shared
                      edge by/      |      \edge by
                      6 & 7      Shared     4 & 5
                                 edge by 
                                 5 & 6
                     */
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
                            if (!dungeonGrid[x0, y0].Found)
                            {
                                dungeonGrid[x0, y0].NewlyFound = true;
                            }
                            dungeonGrid[x0, y0].Found = dungeonGrid[x0, y0].InRange = dungeonGrid[x0, y0].Occupiable = true;
                            if (dungeonGrid[x0, y0].Type == TileType.TILE_WALL || dungeonGrid[x0, y0].Type == TileType.TILE_ROCK)
                            {
                                dungeonGrid[x0, y0].Occupiable = false;
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

        public static void IncreaseTileOpacity(ref DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, GameTime gameTime, StateSpaceComponents spaceComponents)
        {
                for (int i = 0; i < dungeonDimensions.X; i++)
                {
                    for (int j = 0; j < dungeonDimensions.Y; j++)
                    {
                        if (dungeonGrid[i, j].NewlyFound)
                        {
                            if (dungeonGrid[i, j].Occupiable)
                            {
                                dungeonGrid[i, j].Opacity += (float)gameTime.ElapsedGameTime.TotalSeconds * 6;
                            }
                            else
                            {
                                dungeonGrid[i, j].Opacity += (float)gameTime.ElapsedGameTime.TotalSeconds * 4;
                            }
                        }
                    }
                }
            if(spaceComponents.PlayerComponent.PlayerJustLoaded)
            {
                for (int i = 0; i < dungeonDimensions.X; i++)
                {
                    for (int j = 0; j < dungeonDimensions.Y; j++)
                    {
                        if (dungeonGrid[i, j].NewlyFound)
                        {
                            dungeonGrid[i, j].Opacity = 1;
                        }
                    }
                }
            }
            
        }
    }
}
