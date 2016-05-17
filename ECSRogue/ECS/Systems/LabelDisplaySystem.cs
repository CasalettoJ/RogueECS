using ECSRogue.BaseEngine;
using ECSRogue.ECS.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class LabelDisplaySystem
    {
        public static void DrawString(SpriteBatch spriteBatch, StateSpaceComponents spaceComponents, SpriteFont font, Camera camera)
        {
            Matrix cameraMatrix = camera.GetMatrix();
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.DrawableLabel) == ComponentMasks.DrawableLabel).Select(x => x.Id))
            {
                LabelComponent label = spaceComponents.LabelComponents[id];
                Vector2 position = new Vector2(spaceComponents.PositionComponents[id].Position.X, spaceComponents.PositionComponents[id].Position.Y);
                Vector2 stringSize = font.MeasureString(label.Text);
                Vector2 bottomRight = Vector2.Transform(new Vector2(position.X + stringSize.X, position.Y + stringSize.Y), cameraMatrix);
                Vector2 topLeft = Vector2.Transform(new Vector2(position.X, position.Y), cameraMatrix);

                Rectangle cameraBounds = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)bottomRight.X - (int)topLeft.X, (int)bottomRight.Y - (int)topLeft.Y);

                if (camera.IsInView(cameraMatrix, cameraBounds))
                {
                    spriteBatch.DrawString(font, label.Text, position,
                        label.Color, label.Rotation, label.Origin, label.Scale, label.SpriteEffect, 0f);
                }
            }
        }
    }
}
