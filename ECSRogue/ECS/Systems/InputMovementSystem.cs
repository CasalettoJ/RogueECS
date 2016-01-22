using ECSRogue.BaseEngine;
using ECSRogue.BaseEngine.IO.Objects;
using ECSRogue.ECS.Components;
using ECSRogue.ProceduralGeneration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class InputMovementSystem
    {
        public static void HandleDungeonMovement(StateSpaceComponents spaceComponents, GraphicsDeviceManager graphics, GameTime gameTime,
            KeyboardState prevKeyboardState, MouseState prevMouseState, GamePadState prevGamepadState, Camera camera, DungeonTile[,] dungeonGrid, GameSettings gameSettings)
        {
            IEnumerable<Guid> movableEntities = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.InputMoveable) == ComponentMasks.InputMoveable).Select(x => x.Id);
            foreach(Guid id in movableEntities)
            {
                bool hitWall = false;
                bool movement = false;
                KeyboardState keyState = Keyboard.GetState();
                PositionComponent pos = spaceComponents.PositionComponents[id];
                GameplayInfoComponent gameInfo = spaceComponents.GameplayInfoComponents[id];
                if (keyState.IsKeyDown(Keys.NumPad8) && !prevKeyboardState.IsKeyDown(Keys.NumPad8))
                {
                    pos.Position.Y -= 1;
                    movement = true;
                }
                else if (keyState.IsKeyDown(Keys.NumPad2) && !prevKeyboardState.IsKeyDown(Keys.NumPad2))
                {
                    pos.Position.Y += 1;
                    movement = true;
                }
                else if (keyState.IsKeyDown(Keys.NumPad6) && !prevKeyboardState.IsKeyDown(Keys.NumPad6))
                {
                    pos.Position.X += 1;
                    movement = true;
                }
                else if (keyState.IsKeyDown(Keys.NumPad4) && !prevKeyboardState.IsKeyDown(Keys.NumPad4))
                {
                    pos.Position.X -= 1;
                    movement = true;
                }
                else if (keyState.IsKeyDown(Keys.NumPad7) && !prevKeyboardState.IsKeyDown(Keys.NumPad7))
                {
                    pos.Position.X -= 1;
                    pos.Position.Y -= 1;
                    movement = true;
                }
                else if (keyState.IsKeyDown(Keys.NumPad9) && !prevKeyboardState.IsKeyDown(Keys.NumPad9))
                {
                    pos.Position.X += 1;
                    pos.Position.Y -= 1;
                    movement = true;
                }
                else if (keyState.IsKeyDown(Keys.NumPad1) && !prevKeyboardState.IsKeyDown(Keys.NumPad1))
                {
                    pos.Position.X -= 1;
                    pos.Position.Y += 1;
                    movement = true;
                }
                else if (keyState.IsKeyDown(Keys.NumPad3) && !prevKeyboardState.IsKeyDown(Keys.NumPad3))
                {
                    pos.Position.X += 1;
                    pos.Position.Y += 1;
                    movement = true;
                }

                hitWall = !dungeonGrid[(int)pos.Position.X, (int)pos.Position.Y].Occupiable;
                if (!hitWall && movement)
                {
                    //Check collisions.  If no collisions, move into spot.
                    spaceComponents.PositionComponents[id] = pos;
                    gameInfo.StepsTaken += 1;
                    spaceComponents.GameplayInfoComponents[id] = gameInfo;
                    PlayerComponent player = spaceComponents.PlayerComponent;
                    player.PlayerTookTurn = true;
                    spaceComponents.PlayerComponent = player;
                }
                if(hitWall)
                {
                    MessageDisplaySystem.GenerateRandomGameMessage(spaceComponents, Messages.WallCollisionMessages, MessageColors.Normal, gameSettings);
                }

            }
        }
    }


}
