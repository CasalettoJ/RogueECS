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
                GameplayInfoComponent gameInfo = spaceComponents.GameplayInfoComponent;
                InputMovementComponent movementComponent = spaceComponents.InputMovementComponents[id];
                if (keyState.IsKeyDown(Keys.NumPad8))
                {
                    movement = InputMovementSystem.CalculateMovement(ref pos, 0, -1, ref movementComponent, gameTime, Keys.NumPad8);
                }
                else if (keyState.IsKeyDown(Keys.NumPad2))
                {
                    movement = InputMovementSystem.CalculateMovement(ref pos, 0, 1, ref movementComponent, gameTime, Keys.NumPad2);
                }
                else if (keyState.IsKeyDown(Keys.NumPad6))
                {
                    movement = InputMovementSystem.CalculateMovement(ref pos, 1, 0, ref movementComponent, gameTime, Keys.NumPad6);
                }
                else if (keyState.IsKeyDown(Keys.NumPad4))
                {
                    movement = InputMovementSystem.CalculateMovement(ref pos, -1, 0, ref movementComponent, gameTime, Keys.NumPad4);
                }
                else if (keyState.IsKeyDown(Keys.NumPad7))
                {
                    movement = InputMovementSystem.CalculateMovement(ref pos, -1, -1, ref movementComponent, gameTime, Keys.NumPad7);
                }
                else if (keyState.IsKeyDown(Keys.NumPad9))
                {
                    movement = InputMovementSystem.CalculateMovement(ref pos, 1, -1, ref movementComponent, gameTime, Keys.NumPad9);
                }
                else if (keyState.IsKeyDown(Keys.NumPad1))
                {
                    movement = InputMovementSystem.CalculateMovement(ref pos, -1, 1, ref movementComponent, gameTime, Keys.NumPad1);
                }
                else if (keyState.IsKeyDown(Keys.NumPad3))
                {
                    movement = InputMovementSystem.CalculateMovement(ref pos, 1, 1, ref movementComponent, gameTime, Keys.NumPad3);
                }

                else if (keyState.IsKeyDown(Keys.Z) && prevKeyboardState.IsKeyUp(Keys.Z))
                {
                    SightRadiusComponent radius = spaceComponents.SightRadiusComponents[id];
                    radius.CurrentRadius -= 1;
                    spaceComponents.SightRadiusComponents[id] = (radius.CurrentRadius <= 0) ? spaceComponents.SightRadiusComponents[id] : radius;
                    movement = true;
                }
                else if (keyState.IsKeyDown(Keys.X) && prevKeyboardState.IsKeyUp(Keys.X))
                {
                    SightRadiusComponent radius = spaceComponents.SightRadiusComponents[id];
                    radius.CurrentRadius += 1;
                    spaceComponents.SightRadiusComponents[id] = (radius.CurrentRadius > spaceComponents.SightRadiusComponents[id].MaxRadius) ? spaceComponents.SightRadiusComponents[id] : radius;
                    movement = true;
                }
                else
                {
                    movementComponent.IsButtonDown = false;
                    movementComponent.TotalTimeButtonDown = 0f;
                    movementComponent.LastKeyPressed = Keys.None;
                }

                hitWall = !dungeonGrid[(int)pos.Position.X, (int)pos.Position.Y].Occupiable;
                spaceComponents.InputMovementComponents[id] = movementComponent;
                if (!hitWall && movement)
                {
                    //Check collisions.  If no collisions, move into spot.
                    CollisionSystem.TryToMove(spaceComponents, dungeonGrid, pos, id);
                    gameInfo.StepsTaken += 1;
                    spaceComponents.GameplayInfoComponent = gameInfo;
                    PlayerComponent player = spaceComponents.PlayerComponent;
                    player.PlayerTookTurn = true;
                    spaceComponents.PlayerComponent = player;
                }
                if(hitWall)
                {
                    MessageDisplaySystem.GenerateRandomGameMessage(spaceComponents, Messages.WallCollisionMessages, Colors.Messages.Normal, gameSettings);
                }

            }
        }


        private static bool CalculateMovement(ref PositionComponent pos, int xChange, int yChange, ref InputMovementComponent movementComponent, GameTime gameTime, Keys keyPressed)
        {
            bool movement = false;
            if (movementComponent.IsButtonDown)
            {
                if(keyPressed == movementComponent.LastKeyPressed)
                {
                    movementComponent.TimeSinceLastMovement += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    movementComponent.TotalTimeButtonDown += (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    movementComponent.IsButtonDown = false;
                    movementComponent.TotalTimeButtonDown = 0f;
                }
            }
            if (!movementComponent.IsButtonDown || (movementComponent.TimeIntervalBetweenMovements < movementComponent.TimeSinceLastMovement 
                && movementComponent.InitialWait < movementComponent.TotalTimeButtonDown))
            {
                movementComponent.IsButtonDown = true;
                movementComponent.LastKeyPressed = keyPressed;
                movementComponent.TimeSinceLastMovement = 0f;
                movement = true;
                pos.Position.X += xChange;
                pos.Position.Y += yChange;

            }


            return movement;
        }


    }
}
