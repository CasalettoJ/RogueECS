using ECSRogue.BaseEngine;
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
        public static void AICheckDetection(StateSpaceComponents spaceComponents)
        {
            if(spaceComponents.PlayerComponent.PlayerTookTurn)
            {
                foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.CombatReadyAI) == ComponentMasks.CombatReadyAI).Select(x => x.Id))
                {
                    AIState state = spaceComponents.AIStateComponents[id];
                    switch (state.State)
                    {
                        case AIStates.STATE_SLEEPING:
                            AISystem.AITryToWake(id, spaceComponents);
                            break;
                        case AIStates.STATE_ROAMING:
                            AISystem.AITryToFind(id, spaceComponents);
                            break;
                    }
                }
            }
        }

        public static void AICheckFleeing(StateSpaceComponents spaceComponents)
        {
            if (spaceComponents.PlayerComponent.PlayerTookTurn)
            {
                foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.CombatReadyAI) == ComponentMasks.CombatReadyAI).Select(x => x.Id))
                {
                    AIState state = spaceComponents.AIStateComponents[id];
                    if (state.State == AIStates.STATE_ATTACKING)
                    {
                        AIFlee flee = spaceComponents.AIFleeComponents[id];
                        if (flee.DoesFlee)
                        {
                            AIFieldOfView FOV = spaceComponents.AIFieldOfViewComponents[id];
                            SkillLevelsComponent skills = spaceComponents.SkillLevelsComponents[id];
                            AIAlignment alignment = spaceComponents.AIAlignmentComponents[id];
                            double healthPercent = ((double)skills.CurrentHealth / (double)skills.Health) * 100;
                            if (healthPercent <= flee.FleeAtHealthPercent)
                            {
                                state.State = AIStates.STATE_FLEEING;
                                FOV.Color = FOVColors.Fleeing;
                                spaceComponents.AIStateComponents[id] = state;
                                spaceComponents.AIFieldOfViewComponents[id] = FOV;
                                spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.StatusChange,
                                    string.Format("[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + Messages.Flee[spaceComponents.random.Next(0, Messages.Flee.Count())], spaceComponents.NameComponents[id].Name)));
                            }
                        }
                    }
                    else if (state.State == AIStates.STATE_FLEEING)
                    {
                        AIFlee flee = spaceComponents.AIFleeComponents[id];
                        AIFieldOfView FOV = spaceComponents.AIFieldOfViewComponents[id];
                        SkillLevelsComponent skills = spaceComponents.SkillLevelsComponents[id];
                        double healthPercent = ((double)skills.CurrentHealth / (double)skills.Health) * 100;
                        AIAlignment alignment = spaceComponents.AIAlignmentComponents[id];
                        if (healthPercent >= flee.FleeUntilHealthPercent)
                        {
                            state.State = AIStates.STATE_ATTACKING;
                            spaceComponents.AIStateComponents[id] = state;
                            FOV.Color = FOVColors.Fleeing;
                            spaceComponents.AIStateComponents[id] = state;
                            spaceComponents.AIFieldOfViewComponents[id] = FOV;
                            spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.StatusChange,
                                string.Format("[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + Messages.Flee[spaceComponents.random.Next(0, Messages.Flee.Count())], spaceComponents.NameComponents[id].Name)));
                        }
                    }
                }
            }
        }

        public static void AIMovement(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, DijkstraMapTile[,] mapToPlayer)
        {
            if (spaceComponents.PlayerComponent.PlayerTookTurn)
            {
                // Handle Combat ready AI entities
                foreach (Guid id in spaceComponents.Entities.Where(c => ((c.ComponentFlags & ComponentMasks.CombatReadyAI) == ComponentMasks.CombatReadyAI)).Select(c => c.Id))
                {
                    AIAlignment alignment = spaceComponents.AIAlignmentComponents[id];
                    AIState state = spaceComponents.AIStateComponents[id];
                    PositionComponent position = spaceComponents.PositionComponents[id];
                    switch(state.State)
                    {
                        case AIStates.STATE_ROAMING:
                            position = AISystem.AIRoam(id, position, dungeonGrid, dungeonDimensions, spaceComponents.random);
                            break;
                        case AIStates.STATE_ATTACKING:
                            position = AISystem.AIAttack(id, position, dungeonDimensions, mapToPlayer, spaceComponents.random);
                            break;
                        case AIStates.STATE_FLEEING:
                            position = AISystem.AIFlee(id, position, dungeonDimensions, mapToPlayer, spaceComponents.random);
                            break;
                    }
                    CollisionSystem.TryToMove(spaceComponents, dungeonGrid, position, id);
                }
            }
        }

        public static void AIUpdateVision(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions)
        {
            foreach(Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.AIView) == ComponentMasks.AIView).Select(x => x.Id))
            {

                AIFieldOfView fieldOfViewInfo = spaceComponents.AIFieldOfViewComponents[id];

                //Reset seen tiles
                fieldOfViewInfo.SeenTiles = new List<Vector2>();

                if(fieldOfViewInfo.radius >= 0)
                {
                    Vector2 position = spaceComponents.PositionComponents[id].Position;
                    int radius = fieldOfViewInfo.radius;
                    int initialX, x0, initialY, y0;
                    initialX = x0 = (int)position.X;
                    initialY = y0 = (int)position.Y;

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

                                fieldOfViewInfo.SeenTiles.Add(new Vector2(x0, y0));
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
                    fieldOfViewInfo.SeenTiles = fieldOfViewInfo.SeenTiles.Distinct().ToList();
                    spaceComponents.AIFieldOfViewComponents[id] = fieldOfViewInfo;
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

        private static PositionComponent AIFlee(Guid entity, PositionComponent position, Vector2 dungeonDimensions, DijkstraMapTile[,] mapToPlayer, Random random)
        {
            int lowestGridTile = 1;
            List<Vector2> validSpots = new List<Vector2>();
            for (int i = (int)position.Position.X - 1; i <= (int)position.Position.X + 1; i++)
            {
                for (int j = (int)position.Position.Y - 1; j <= (int)position.Position.Y + 1; j++)
                {
                    if (i >= 0 && j >= 0 && i < (int)dungeonDimensions.X && j < (int)dungeonDimensions.Y)
                    {
                        if (mapToPlayer[i, j].Weight < DevConstants.Grid.WallWeight)
                        {
                            lowestGridTile = (mapToPlayer[i, j].Weight * -1 < lowestGridTile * -1) ? mapToPlayer[i, j].Weight : lowestGridTile;
                        }
                    }
                }
            }
            for (int i = (int)position.Position.X - 1; i <= (int)position.Position.X + 1; i++)
            {
                for (int j = (int)position.Position.Y - 1; j <= (int)position.Position.Y + 1; j++)
                {
                    if (i >= 0 && j >= 0 && i < (int)dungeonDimensions.X && j < (int)dungeonDimensions.Y && mapToPlayer[i, j].Weight == lowestGridTile)
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

        private static void AITryToWake(Guid entity, StateSpaceComponents spaceComponents)
        {
            AIFieldOfView entityFOV = spaceComponents.AIFieldOfViewComponents[entity];
            AIAlignment entityAlignment = spaceComponents.AIAlignmentComponents[entity];
            AISleep entitySleep = spaceComponents.AISleepComponents[entity];
            AIState entityState = spaceComponents.AIStateComponents[entity];
            foreach (Guid id in spaceComponents.Entities.Where(x => ((x.ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER) || 
                ((x.ComponentFlags & ComponentMasks.CombatReadyAI) == ComponentMasks.CombatReadyAI)).Select(x => x.Id))
            {
                PositionComponent position = spaceComponents.PositionComponents[id];
                AIAlignment alignment = spaceComponents.AIAlignmentComponents[id];
                if(entityFOV.SeenTiles.Contains(position.Position) && entityAlignment.Alignment != alignment.Alignment && spaceComponents.random.Next(1,101) <= entitySleep.ChanceToWake)
                {
                    entityState.State = AIStates.STATE_ROAMING;
                    entityFOV.radius += entitySleep.FOVRadiusChangeOnWake;
                    entityFOV.Color = FOVColors.Roaming;
                    spaceComponents.AIStateComponents[entity] = entityState;
                    spaceComponents.AIFieldOfViewComponents[entity] = entityFOV;
                    spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.StatusChange,
                        string.Format("[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + Messages.AwokenBySight[spaceComponents.random.Next(0, Messages.AwokenBySight.Count())], spaceComponents.NameComponents[entity].Name)));
                    return;
                }
            }
        }

        private static void AITryToFind(Guid entity, StateSpaceComponents spaceComponents)
        {
            AIFieldOfView entityFOV = spaceComponents.AIFieldOfViewComponents[entity];
            AIAlignment entityAlignment = spaceComponents.AIAlignmentComponents[entity];
            AIRoam entityRoam = spaceComponents.AIRoamComponents[entity];
            AIState entityState = spaceComponents.AIStateComponents[entity];
            foreach (Guid id in spaceComponents.Entities.Where(x => ((x.ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER) ||
                ((x.ComponentFlags & ComponentMasks.CombatReadyAI) == ComponentMasks.CombatReadyAI)).Select(x => x.Id))
            {
                PositionComponent position = spaceComponents.PositionComponents[id];
                AIAlignment alignment = spaceComponents.AIAlignmentComponents[id];
                if (entityFOV.SeenTiles.Contains(position.Position) && entityAlignment.Alignment != alignment.Alignment && spaceComponents.random.Next(1, 101) <= entityRoam.ChanceToDetect)
                {
                    entityState.State = AIStates.STATE_ATTACKING;
                    entityFOV.Color = FOVColors.Attacking;
                    spaceComponents.AIStateComponents[entity] = entityState;
                    spaceComponents.AIFieldOfViewComponents[entity] = entityFOV;
                    spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.StatusChange,
                        string.Format("[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + Messages.FoundBySight[spaceComponents.random.Next(0, Messages.FoundBySight.Count())], spaceComponents.NameComponents[entity].Name)));
                    return;
                }
            }
        }


    }
}
