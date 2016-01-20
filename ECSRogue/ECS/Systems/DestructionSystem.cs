using ECSRogue.ECS.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class DestructionSystem
    {
        public static void UpdateDestructionTimes(StateSpaceComponents spaceComponents, GameTime gameTime)
        {
            foreach(Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_TIME_TO_LIVE) == Component.COMPONENT_TIME_TO_LIVE).Select(x => x.Id))
            {
                TimeToLiveComponent timeToLive = spaceComponents.TimeToLiveComponents[id];
                timeToLive.CurrentSecondsAlive += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if(timeToLive.SecondsToLive < timeToLive.CurrentSecondsAlive)
                {
                    spaceComponents.EntitiesToDelete.Add(id);
                }
                else
                {
                    spaceComponents.TimeToLiveComponents[id] = timeToLive;
                }
            }
        }
    }
}
