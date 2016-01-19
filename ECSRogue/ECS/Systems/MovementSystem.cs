using ECSRogue.ECS.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class MovementSystem
    {
        public static void UpdateMovingEntities(StateSpaceComponents spaceComponents, GameTime gameTime)
        {
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.MovingEntity) == ComponentMasks.MovingEntity).Select(x => x.Id))
            {
                PositionComponent position = spaceComponents.PositionComponents[id];
                Vector2 Direction = Vector2.Normalize(spaceComponents.TargetPositionComponents[id].TargetPosition - position.Position);
                float distance = Math.Abs(Vector2.Distance(position.Position, spaceComponents.TargetPositionComponents[id].TargetPosition));
                if (distance > 10)
                {
                    position.Position += Direction * spaceComponents.VelocityComponents[id].Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    spaceComponents.PositionComponents[id] = position;
                }
                else if (spaceComponents.TargetPositionComponents[id].DestroyWhenReached)
                {
                    spaceComponents.EntitiesToDelete.Add(id);
                }
            }
        }

        public static void UpdateIndefinitelyMovingEntities(StateSpaceComponents spaceComponents, GameTime gameTime)
        {
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.IndefiniteMovingEntity) == ComponentMasks.IndefiniteMovingEntity).Select(x => x.Id))
            {
                PositionComponent position = spaceComponents.PositionComponents[id];
                position.Position += spaceComponents.DirectionComponents[id].Direction * spaceComponents.VelocityComponents[id].Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                spaceComponents.PositionComponents[id] = position;
            }
        }
    }
}
