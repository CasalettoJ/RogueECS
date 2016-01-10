using ECSRogue.BaseEngine;
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
            KeyboardState prevKeyboardState, MouseState prevMouseState, GamePadState prevGamepadState, Camera camera, DungeonTile[,] dungeonGrid)
        {
            IEnumerable<Guid> movableEntities = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.InputMoveable) == ComponentMasks.InputMoveable).Select(x => x.Id);
            foreach(Guid id in movableEntities)
            {
                KeyboardState keyState = Keyboard.GetState();
                if (keyState.IsKeyDown(Keys.NumPad8) && !prevKeyboardState.IsKeyDown(Keys.NumPad8))
                {
                    PositionComponent pos = spaceComponents.PositionComponents[id];
                    pos.Position.Y -= 1;
                    if (dungeonGrid[(int)pos.Position.X, (int)pos.Position.Y].Type == TileType.TILE_FLOOR)
                    {
                        spaceComponents.PositionComponents[id] = pos;
                    }
                }
                if (keyState.IsKeyDown(Keys.NumPad2) && !prevKeyboardState.IsKeyDown(Keys.NumPad2))
                {
                    PositionComponent pos = spaceComponents.PositionComponents[id];
                    pos.Position.Y += 1;
                    if (dungeonGrid[(int)pos.Position.X, (int)pos.Position.Y].Type == TileType.TILE_FLOOR)
                    {
                        spaceComponents.PositionComponents[id] = pos;
                    }
                }
                if (keyState.IsKeyDown(Keys.NumPad6) && !prevKeyboardState.IsKeyDown(Keys.NumPad6))
                {
                    PositionComponent pos = spaceComponents.PositionComponents[id];
                    pos.Position.X += 1;
                    if (dungeonGrid[(int)pos.Position.X, (int)pos.Position.Y].Type == TileType.TILE_FLOOR)
                    {
                        spaceComponents.PositionComponents[id] = pos;
                    }
                }
                if (keyState.IsKeyDown(Keys.NumPad4) && !prevKeyboardState.IsKeyDown(Keys.NumPad4))
                {
                    PositionComponent pos = spaceComponents.PositionComponents[id];
                    pos.Position.X -= 1;
                    if (dungeonGrid[(int)pos.Position.X, (int)pos.Position.Y].Type == TileType.TILE_FLOOR)
                    {
                        spaceComponents.PositionComponents[id] = pos;
                    }
                }
                if (keyState.IsKeyDown(Keys.NumPad7) && !prevKeyboardState.IsKeyDown(Keys.NumPad7))
                {
                    PositionComponent pos = spaceComponents.PositionComponents[id];
                    pos.Position.X -= 1;
                    pos.Position.Y -= 1;
                    if (dungeonGrid[(int)pos.Position.X, (int)pos.Position.Y].Type == TileType.TILE_FLOOR)
                    {
                        spaceComponents.PositionComponents[id] = pos;
                    }
                }
                if (keyState.IsKeyDown(Keys.NumPad9) && !prevKeyboardState.IsKeyDown(Keys.NumPad9))
                {
                    PositionComponent pos = spaceComponents.PositionComponents[id];
                    pos.Position.X += 1;
                    pos.Position.Y -= 1;
                    if (dungeonGrid[(int)pos.Position.X, (int)pos.Position.Y].Type == TileType.TILE_FLOOR)
                    {
                        spaceComponents.PositionComponents[id] = pos;
                    }
                }
                if (keyState.IsKeyDown(Keys.NumPad1) && !prevKeyboardState.IsKeyDown(Keys.NumPad1))
                {
                    PositionComponent pos = spaceComponents.PositionComponents[id];
                    pos.Position.X -= 1;
                    pos.Position.Y += 1;
                    if (dungeonGrid[(int)pos.Position.X, (int)pos.Position.Y].Type == TileType.TILE_FLOOR)
                    {
                        spaceComponents.PositionComponents[id] = pos;
                    }
                }
                if (keyState.IsKeyDown(Keys.NumPad3) && !prevKeyboardState.IsKeyDown(Keys.NumPad3))
                {
                    PositionComponent pos = spaceComponents.PositionComponents[id];
                    pos.Position.X += 1;
                    pos.Position.Y += 1;
                    if (dungeonGrid[(int)pos.Position.X, (int)pos.Position.Y].Type == TileType.TILE_FLOOR)
                    {
                        spaceComponents.PositionComponents[id] = pos;
                    }
                }
            }
        }
    }


}
