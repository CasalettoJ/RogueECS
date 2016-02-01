using ECSRogue.ECS.Components;
using ECSRogue.ECS.Components.AIComponents;
using ECSRogue.ProceduralGeneration;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class AISystem
    {
        public static void AICheckDetection(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid)
        {
            foreach(Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_AI_STATE) == Component.COMPONENT_AI_STATE).Select(x => x.Id))
            {
                PositionComponent position = spaceComponents.PositionComponents[id];
                if(dungeonGrid[(int)position.Position.X, (int)position.Position.Y].InRange)
                {
                    AIState state = spaceComponents.AIStateComponents[id];
                    state.State = AIStates.STATE_ATTACKING;
                    spaceComponents.AIStateComponents[id] = state;
                }
            }
        }

        public static void AIMovement(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, DijkstraMapTile[,] mapToPlayer)
        {
            if (spaceComponents.PlayerComponent.PlayerTookTurn)
            {
                // Handle Combat ready AI entities
                foreach (Guid id in spaceComponents.Entities.Where(c => ((c.ComponentFlags & ComponentMasks.CombatReadyAI) == ComponentMasks.CombatReadyAI)
                     && ((c.ComponentFlags & ComponentMasks.MovingAI) != ComponentMasks.MovingAI)).Select(c => c.Id))
                {
                    AIAlignment alignment = spaceComponents.AIAlignmentComponents[id];
                    AIState state = spaceComponents.AIStateComponents[id];
                    PositionComponent position = spaceComponents.PositionComponents[id];
                    switch(state.State)
                    {
                        case AIStates.STATE_SLEEPING:
                            break;
                        case AIStates.STATE_ROAMING:
                            position = AISystem.AIRoam(id, position, dungeonGrid, dungeonDimensions, spaceComponents.random);
                            break;
                        case AIStates.STATE_ATTACKING:
                            position = AISystem.AIAttack(id, position, dungeonDimensions, mapToPlayer, spaceComponents.random);
                            break;
                        case AIStates.STATE_FLEEING:
                            break;
                    }
                    CollisionSystem.TryToMove(spaceComponents, dungeonGrid, position, id);
                }

                //Handle AIs that have a direct target to get to
                foreach (Guid id in spaceComponents.Entities.Where(c => (c.ComponentFlags & ComponentMasks.MovingAI) == ComponentMasks.MovingAI).Select(c => c.Id))
                {

                }
            }
        }

        private static PositionComponent AIAttack(Guid entity, PositionComponent position, Vector2 dungeonDimensions, DijkstraMapTile[,] mapToPlayer, Random random)
        {
            int lowestGridTile = 1000000;
            List<Vector2> validSpots = new List<Vector2>();
            for (int i = (int)position.Position.X - 1; i <= (int)position.Position.X + 1; i++)
            {
                for (int j = (int)position.Position.Y - 1; j <= (int)position.Position.Y + 1; j++)
                {
                    if (i >= 0 && j >= 0 && i < (int)dungeonDimensions.X && j < (int)dungeonDimensions.Y)
                    {
                        lowestGridTile = (mapToPlayer[i, j].Weight < lowestGridTile) ? mapToPlayer[i, j].Weight : lowestGridTile;
                    }
                }
            }
            for (int i = (int)position.Position.X - 1; i <= (int)position.Position.X + 1; i++)
            {
                for (int j = (int)position.Position.Y - 1; j <= (int)position.Position.Y + 1; j++)
                {
                    if (i >= 0 && j >= 0 && i < (int)dungeonDimensions.X && j < (int)dungeonDimensions.Y && mapToPlayer[i,j].Weight == lowestGridTile)
                    {
                        validSpots.Add(new Vector2(i, j));
                    }
                }
            }
            return new PositionComponent() { Position = validSpots[random.Next(0, validSpots.Count)] };
        }


        private static PositionComponent AIRoam(Guid entity, PositionComponent position, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, Random random)
        {
            List<Vector2> validSpots = new List<Vector2>();
            //Select a random traversible direction and move in that direction
            for(int i = (int)position.Position.X-1; i <= (int)position.Position.X+1; i++)
            {
                for(int j = (int)position.Position.Y-1; j <= (int)position.Position.Y+1; j++)
                {
                    if(i >=0 && j >= 0 && i < (int)dungeonDimensions.X && j < (int)dungeonDimensions.Y &&  dungeonGrid[i,j].Occupiable)
                    {
                        validSpots.Add(new Vector2(i, j));
                    }
                }
            }

            //Choose a new spot
            return new PositionComponent() { Position = validSpots[random.Next(0, validSpots.Count)] };

        }
        

    }
}
