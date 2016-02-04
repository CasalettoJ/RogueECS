using ECSRogue.BaseEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class CameraSystem
    {

        public static void UpdateCamera(Camera camera, GameTime gameTime, StateSpaceComponents stateSpaceComponents, int cellSize, KeyboardState prevKey)
        {
            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                MessageDisplaySystem.SetRandomGlobalMessage(stateSpaceComponents, Messages.CameraDetatchedMessage);
                camera.Target = Vector2.Transform(Mouse.GetState().Position.ToVector2(), camera.GetInverseMatrix());
                camera.AttachedToPlayer = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                camera.AttachedToPlayer = false;
                camera.Position.Y -= camera.Velocity.Y * (float)gameTime.ElapsedGameTime.TotalSeconds;
                camera.Target = camera.Position;
                MessageDisplaySystem.SetRandomGlobalMessage(stateSpaceComponents, Messages.CameraDetatchedMessage);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                camera.AttachedToPlayer = false;
                camera.Position.X -= camera.Velocity.X * (float)gameTime.ElapsedGameTime.TotalSeconds;
                camera.Target = camera.Position;
                MessageDisplaySystem.SetRandomGlobalMessage(stateSpaceComponents, Messages.CameraDetatchedMessage);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                camera.AttachedToPlayer = false;
                camera.Position.Y += camera.Velocity.Y * (float)gameTime.ElapsedGameTime.TotalSeconds;
                camera.Target = camera.Position;
                MessageDisplaySystem.SetRandomGlobalMessage(stateSpaceComponents, Messages.CameraDetatchedMessage);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                camera.AttachedToPlayer = false;
                camera.Position.X += camera.Velocity.X * (float)gameTime.ElapsedGameTime.TotalSeconds;
                camera.Target = camera.Position;
                MessageDisplaySystem.SetRandomGlobalMessage(stateSpaceComponents, Messages.CameraDetatchedMessage);
            }
            if(Keyboard.GetState().IsKeyDown(Keys.OemPlus) && prevKey.IsKeyUp(Keys.OemPlus))
            {
                if(camera.Scale + .25f < 4.25f)
                {
                    camera.Scale += .25f;
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus) && prevKey.IsKeyUp(Keys.OemMinus))
            {
                if (camera.Scale - .25f > 0f)
                {
                    camera.Scale -= .25f;
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                camera.AttachedToPlayer = true;
                MessageDisplaySystem.SetRandomGlobalMessage(stateSpaceComponents, new string[] { string.Empty });
            }

            if (camera.AttachedToPlayer)
            {
                Entity playerId = stateSpaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).FirstOrDefault();
                if (playerId != null)
                {
                    camera.Target = new Vector2((int)stateSpaceComponents.PositionComponents[playerId.Id].Position.X * cellSize + stateSpaceComponents.DisplayComponents[playerId.Id].SpriteSource.Width / 2,
                    (int)stateSpaceComponents.PositionComponents[playerId.Id].Position.Y * cellSize + stateSpaceComponents.DisplayComponents[playerId.Id].SpriteSource.Height / 2);
                }
                if (stateSpaceComponents.PlayerComponent.PlayerJustLoaded)
                {
                    camera.Position = new Vector2((int)stateSpaceComponents.PositionComponents[playerId.Id].Position.X * cellSize + stateSpaceComponents.DisplayComponents[playerId.Id].SpriteSource.Width / 2,
                    (int)stateSpaceComponents.PositionComponents[playerId.Id].Position.Y * cellSize + stateSpaceComponents.DisplayComponents[playerId.Id].SpriteSource.Height / 2);
                }
            }
            if (Vector2.Distance(camera.Position, camera.Target) > 0)
            {
                float distance = Vector2.Distance(camera.Position, camera.Target);
                Vector2 direction = Vector2.Normalize(camera.Target - camera.Position);
                float velocity = distance * 2.5f;
                if (distance > 10f)
                {
                    camera.Position += direction * velocity * (camera.Scale >= 1 ? camera.Scale : 1) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
            }
        }
    }
}
