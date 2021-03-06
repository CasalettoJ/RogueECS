﻿using ECSRogue.ECS.Components;
using ECSRogue.ProceduralGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ECSRogue.BaseEngine.DevConstants;

namespace ECSRogue.ECS.Systems
{
    public static class CollisionSystem
    {
        public static bool TryToMove(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, PositionComponent newPosition, Guid attemptingEntity)
        {
            bool canMove = true;
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Collidable) == ComponentMasks.Collidable).Select(x => x.Id))
            {
                if((int)spaceComponents.PositionComponents[id].Position.X == (int)newPosition.Position.X &&
                    (int)spaceComponents.PositionComponents[id].Position.Y == (int)newPosition.Position.Y &&
                    id != attemptingEntity)
                {
                    spaceComponents.CollisionComponents[attemptingEntity].CollidedObjects.Add(id);
                    spaceComponents.GlobalCollisionComponent.EntitiesThatCollided.Add(attemptingEntity);
                    if(spaceComponents.CollisionComponents[id].Solid && spaceComponents.CollisionComponents[attemptingEntity].Solid)
                    {
                        canMove = false;
                    }
                }
            }

            if(canMove)
            {
                spaceComponents.PositionComponents[attemptingEntity] = newPosition;
                if(dungeonGrid[(int)newPosition.Position.X, (int)newPosition.Position.Y].Type == TileType.TILE_TALLGRASS && 
                    (spaceComponents.Entities.Where(x => (x.Id == attemptingEntity)).First().ComponentFlags & ComponentMasks.Observer) != ComponentMasks.Observer)
                {
                    dungeonGrid[(int)newPosition.Position.X, (int)newPosition.Position.Y].Type = TileType.TILE_FLATTENEDGRASS;
                    dungeonGrid[(int)newPosition.Position.X, (int)newPosition.Position.Y].Symbol = Tiles.FlatGrassSymbol;
                    dungeonGrid[(int)newPosition.Position.X, (int)newPosition.Position.Y].SymbolColor = Tiles.FlatGrassSymbolColor;
                    dungeonGrid[(int)newPosition.Position.X, (int)newPosition.Position.Y].ChanceToIgnite = Tiles.FlatGrassIgniteChance;
                }
                if (dungeonGrid[(int)newPosition.Position.X, (int)newPosition.Position.Y].Type == TileType.TILE_FIRE &&
                    (spaceComponents.Entities.Where(x => (x.Id == attemptingEntity)).First().ComponentFlags & ComponentMasks.Observer) != ComponentMasks.Observer)
                {
                    spaceComponents.DelayedActions.Add(new Action(() =>
                    {
                        //Burn effect damage is hardcoded for now
                        StatusSystem.ApplyBurnEffect(spaceComponents, attemptingEntity, StatusEffects.Burning.Turns, StatusEffects.Burning.MinDamage, StatusEffects.Burning.MaxDamage);
                    }));
                }
            }

            return canMove;
        }

        public static void ResetCollision(StateSpaceComponents spaceComponents)
        {
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Collidable) == ComponentMasks.Collidable).Select(x => x.Id))
            {
                spaceComponents.CollisionComponents[id].CollidedObjects.Clear();
            }
            spaceComponents.GlobalCollisionComponent = new GlobalCollisionComponent() { EntitiesThatCollided = new List<Guid>() };
        }

    }
}
