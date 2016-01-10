using ECSRogue.BaseEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class DisplaySystem
    {
        public static void DrawDungeonEntities(StateSpaceComponents spaceComponents, Camera camera, SpriteBatch spriteBatch, int cellSize)
        {
            IEnumerable<Guid> drawableEntities = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Drawable) == ComponentMasks.Drawable).Select(x => x.Id);
            foreach(Guid id in drawableEntities)
            {
                spriteBatch.Draw(spaceComponents.DisplayComponents[id].Texture, new Vector2(spaceComponents.PositionComponents[id].Position.X * cellSize, spaceComponents.PositionComponents[id].Position.Y * cellSize), spaceComponents.DisplayComponents[id].Color);
            }
        }
    }
}
