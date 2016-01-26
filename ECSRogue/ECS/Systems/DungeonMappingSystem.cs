using ECSRogue.ProceduralGeneration;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class DungeonMappingSystem
    {
        private const int WallValue = 10000;

        public static int[,] CreateDijkstraMap(DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, Vector2 target)
        {
            int[,] dijkstraMap = new int[(int)dungeonDimensions.X, (int)dungeonDimensions.Y];

            //Set up map
            for(int i = 0; i < dungeonDimensions.X; i++)
            {
                for(int j = 0; j < dungeonDimensions.Y; j++)
                {
                    if(dungeonGrid[i,j].Occupiable)
                    {
                        dijkstraMap[i, j] = 100;
                    }
                    else
                    {
                        dijkstraMap[i, j] = WallValue;
                    }
                    if(i == (int)target.X && j == (int)target.Y)
                    {
                        dijkstraMap[i, j] = 0;
                    }
                }
            }

            //Set map values
            bool changes = true;
            while(changes)
            {
                changes = false;
                for (int i = 0; i < dungeonDimensions.X; i++)
                {
                    for (int j = 0; j < dungeonDimensions.Y; j++)
                    {
                        //Get lowest neighbor
                        int lowestNeighbor = WallValue + 1;
                        for( int k = i - 1; k <= i + 1; k++)
                        {
                            for (int l = j - 1; l <= j + 1; l++)
                            {
                                if (l >= 0 && k >= 0 && l < (int)dungeonDimensions.Y && k < (int)dungeonDimensions.X)
                                {
                                    lowestNeighbor = (lowestNeighbor < dijkstraMap[k, l]) ? lowestNeighbor : dijkstraMap[k, l];
                                }
                            }
                        }
                        //If the tile is at least 2 greater than the lowest neighbor, set value to lowestneighbor+1
                        if(dijkstraMap[i,j] >= lowestNeighbor + 2)
                        {
                            dijkstraMap[i, j] = lowestNeighbor + 1;
                            changes = true;
                        }
                    }
                }
            }

            return dijkstraMap;
        }

        

        public static void ShouldPlayerMapRecalc(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, ref int[,] playerMap)
        {
            if(spaceComponents.PlayerComponent.PlayerTookTurn || spaceComponents.PlayerComponent.PlayerJustLoaded)
            {
                CreateDijkstraMapToPlayers(dungeonGrid, dungeonDimensions, spaceComponents, ref playerMap);
            }
        }

        private static void CreateDijkstraMapToPlayers(DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, StateSpaceComponents spaceComponents, ref int[,] dijkstraMap)
        {
            List<Vector2> targets = new List<Vector2>();
            //targets are any entities with friendly AI and players
            foreach(Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).Select(x => x.Id))
            {
                targets.Add(spaceComponents.PositionComponents[id].Position);
            }
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.NPC) == ComponentMasks.NPC).Select(x => x.Id))
            {
                if(spaceComponents.AIComponents[id].Alignment == Components.AIAlignment.ALIGNMENT_FRIENDLY)
                {
                    targets.Add(spaceComponents.PositionComponents[id].Position);
                }
            }

            //Set up map
            for (int i = 0; i < dungeonDimensions.X; i++)
            {
                for (int j = 0; j < dungeonDimensions.Y; j++)
                {
                    if (dungeonGrid[i, j].Occupiable)
                    {
                        dijkstraMap[i, j] = WallValue-1;
                    }
                    else
                    {
                        dijkstraMap[i, j] = WallValue;
                    }
                    if (targets.Contains(new Vector2(i, j)))
                    {
                        dijkstraMap[i, j] = 0;
                    }
                }
            }

            //Set map values
            bool changes = true;
            while (changes)
            {
                changes = false;

                for (int i = 0; i < dungeonDimensions.X; i++)
                {
                    for (int j = 0; j < dungeonDimensions.Y; j++)
                    {
                        //Get lowest neighbor
                        if (dijkstraMap[i, j] != WallValue)
                        {
                            int lowestNeighbor = WallValue + 1;
                            //for (int k = i - 1; k <= i + 1; k++)
                            //{
                            //    for (int l = j - 1; l <= j + 1; l++)
                            //    {
                            //        if (l >= 0 && k >= 0 && l < (int)dungeonDimensions.Y && k < (int)dungeonDimensions.X)
                            //        {
                            //            lowestNeighbor = (lowestNeighbor < dijkstraMap[k, l]) ? lowestNeighbor : dijkstraMap[k, l];
                            //        }
                            //    }
                            //}
                            if(j-1 >= 0 && i-1 >= 0)
                            {
                                lowestNeighbor = (lowestNeighbor < dijkstraMap[i-1,j-1]) ? lowestNeighbor : dijkstraMap[i-1,j-1];
                            }
                            if(j-1 >= 0)
                            {
                                lowestNeighbor = (lowestNeighbor < dijkstraMap[i, j-1]) ? lowestNeighbor : dijkstraMap[i, j-1];
                            }
                            if(j-1 >= 0 && i+1 < (int)dungeonDimensions.X)
                            {
                                lowestNeighbor = (lowestNeighbor < dijkstraMap[i+1, j-1]) ? lowestNeighbor : dijkstraMap[i+1, j-1];
                            }
                            if(i-1 >= 0)
                            {
                                lowestNeighbor = (lowestNeighbor < dijkstraMap[i-1, j]) ? lowestNeighbor : dijkstraMap[i-1, j];
                            }
                            if(i+1 < (int)dungeonDimensions.X)
                            {
                                lowestNeighbor = (lowestNeighbor < dijkstraMap[i+1, j]) ? lowestNeighbor : dijkstraMap[i+1, j];
                            }
                            if (j + 1 < (int)dungeonDimensions.Y && i-1 >= 0)
                            {
                                lowestNeighbor = (lowestNeighbor < dijkstraMap[i-1, j + 1]) ? lowestNeighbor : dijkstraMap[i-1, j + 1];
                            }
                            if (j+1 < (int)dungeonDimensions.Y)
                            {
                                lowestNeighbor = (lowestNeighbor < dijkstraMap[i, j+1]) ? lowestNeighbor : dijkstraMap[i, j+1];
                            }
                            if (j + 1 < (int)dungeonDimensions.Y && i + 1 < (int)dungeonDimensions.Y)
                            {
                                lowestNeighbor = (lowestNeighbor < dijkstraMap[i + 1, j + 1]) ? lowestNeighbor : dijkstraMap[i + 1, j + 1];
                            }
                            //If the tile is at least 2 greater than the lowest neighbor, set value to lowestneighbor+1
                            if (dijkstraMap[i, j] >= lowestNeighbor + 2)
                            {
                                dijkstraMap[i, j] = lowestNeighbor + 1;
                                changes = true;
                            }
                        }
                    }
                }
            }
        }

    }
}
