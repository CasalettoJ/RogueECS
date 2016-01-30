using ECSRogue.ECS.Components;
using ECSRogue.ProceduralGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                    (int)spaceComponents.PositionComponents[id].Position.Y == (int)newPosition.Position.Y)
                {
                    spaceComponents.CollisionComponents[attemptingEntity].CollidedObjects.Add(id);
                    if(spaceComponents.CollisionComponents[id].Solid)
                    {
                        canMove = false;
                    }
                }
            }

            if(canMove)
            {
                spaceComponents.PositionComponents[attemptingEntity] = newPosition;
            }

            return canMove;
        }

        public static void ResetCollision(StateSpaceComponents spaceComponents)
        {
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Collidable) == ComponentMasks.Collidable).Select(x => x.Id))
            {
                spaceComponents.CollisionComponents[id].CollidedObjects.Clear();
            }
        }

    }
}
