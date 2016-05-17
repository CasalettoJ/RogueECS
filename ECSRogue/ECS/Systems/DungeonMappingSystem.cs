using ECSRogue.BaseEngine;
using ECSRogue.ECS.Components.AIComponents;
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

        public static void ShouldPlayerMapRecalc(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, ref DijkstraMapTile[,] playerMap)
        {
            if(spaceComponents.PlayerComponent.PlayerTookTurn || spaceComponents.PlayerComponent.PlayerJustLoaded)
            {
                CreateDijkstraMapToPlayers(dungeonGrid, dungeonDimensions, spaceComponents, ref playerMap);
            }
        }

        private static void CreateDijkstraMapToPlayers(DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, StateSpaceComponents spaceComponents, ref DijkstraMapTile[,] dijkstraMap)
        {
            List<Vector2> targets = new List<Vector2>();
            //targets are any entities with friendly AI and players
            foreach(Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER).Select(x => x.Id))
            {
                targets.Add(spaceComponents.PositionComponents[id].Position);
            }
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_AI_ALIGNMENT) == Component.COMPONENT_AI_ALIGNMENT).Select(x => x.Id))
            {
                if(spaceComponents.AIAlignmentComponents[id].Alignment == AIAlignments.ALIGNMENT_FRIENDLY)
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
                        dijkstraMap[i, j].Weight = DevConstants.Grid.WallWeight -1;
                    }
                    else
                    {
                        dijkstraMap[i, j].Weight = DevConstants.Grid.WallWeight;
                    }
                    if (targets.Contains(new Vector2(i, j)))
                    {
                        dijkstraMap[i, j].Weight = 0;
                    }
                    dijkstraMap[i, j].Checked = false;
                }
            }

            Queue<Vector2> floodQueue = new Queue<Vector2>();
            foreach(Vector2 item in targets)
            {
                floodQueue.Enqueue(item);
            }

            while (floodQueue.Count > 0)
            {
                Vector2 pos = floodQueue.Dequeue();

                int x = (int)pos.X;
                int y = (int)pos.Y;
                int lowestNeighbor = DevConstants.Grid.WallWeight;
                HashSet<Vector2> toAdd = new HashSet<Vector2>();
                for (int k = x - 1; k <= x + 1; k++)
                {
                    for (int l = y - 1; l <= y + 1; l++)
                    {
                        if (l >= 0 && k >= 0 && l < (int)dungeonDimensions.Y && k < (int)dungeonDimensions.X && dijkstraMap[(int)pos.X, (int)pos.Y].Weight != DevConstants.Grid.WallWeight)
                        {
                            lowestNeighbor = (lowestNeighbor < dijkstraMap[k, l].Weight) ? lowestNeighbor : dijkstraMap[k, l].Weight;
                            if(!dijkstraMap[k,l].Checked)
                            {
                                dijkstraMap[k, l].Checked = true;
                                floodQueue.Enqueue(new Vector2(k, l));
                            }
                        }
                    }
                }
                if (dijkstraMap[x,y].Weight >= lowestNeighbor + 2)
                {
                    dijkstraMap[x,y].Weight = lowestNeighbor + 1;
                }
            }




        }

    }
}
