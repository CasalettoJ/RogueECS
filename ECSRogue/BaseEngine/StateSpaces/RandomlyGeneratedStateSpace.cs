using ECSRogue.BaseEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ECSRogue.ECS;
using ECSRogue.ProceduralGeneration;
using ECSRogue.ProceduralGeneration.Interfaces;
using ECSRogue.ECS.Systems;
using ECSRogue.ECS.Components;
using ECSRogue.BaseEngine.IO.Objects;

namespace ECSRogue.BaseEngine.StateSpaces
{
    public class RandomlyGeneratedStateSpace : IStateSpace
    {
        #region Components
        private StateSpaceComponents stateSpaceComponents;
        private StateComponents stateComponents;
        #endregion

        #region Dungeon Environment Variables
        private Texture2D sprites;
        private SpriteFont messageFont;
        private Vector2 dungeonDimensions;
        private DungeonTile[,] dungeonGrid = null;
        private int cellSize;
        private Texture2D dungeonSprites;
        private Texture2D UI;
        private string dungeonSpriteFile;
        private DungeonColorInfo dungeonColorInfo;
        #endregion

        public RandomlyGeneratedStateSpace(IGenerationAlgorithm dungeonGeneration, int worldMin, int worldMax)
        {
            stateSpaceComponents = new StateSpaceComponents();
            dungeonDimensions = dungeonGeneration.GenerateDungeon(ref dungeonGrid, worldMin, worldMax, stateSpaceComponents.random);
            cellSize = dungeonGeneration.GetCellsize();
            dungeonSpriteFile = dungeonGeneration.GetDungeonSpritesheetFileName();
            dungeonColorInfo = dungeonGeneration.GetColorInfo();
        }

        public RandomlyGeneratedStateSpace(DungeonInfo data)
        {
            stateSpaceComponents = data.stateSpaceComponents;
            dungeonSpriteFile = data.dungeonSpriteFile;
            dungeonGrid = data.dungeonGrid;
            cellSize = data.cellSize;
            dungeonColorInfo = data.dungeonColorInfo;
            dungeonDimensions = data.dungeonDimensions;
        }

        #region Load Logic
        public void LoadLevel(ContentManager content, GraphicsDeviceManager graphics, Camera camera, StateComponents stateComponents, bool createEntities = true)
        {
            this.stateComponents = stateComponents;
            sprites = content.Load<Texture2D>("Sprites/anonsheet");
            dungeonSprites = content.Load<Texture2D>(dungeonSpriteFile);
            messageFont = content.Load<SpriteFont>("Fonts/InfoText");
            UI = content.Load<Texture2D>("Sprites/ball");
            if (createEntities)
            {
                CreatePlayer();
                CreateMessageLog();
            }
            camera.AttachedToPlayer = true;
        }

        private void CreatePlayer()
        {
            Guid id = stateSpaceComponents.CreateEntity();
            stateSpaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.Player;
            //Set Position
            int X = 0;
            int Y = 0;
            do
            {
                X = stateSpaceComponents.random.Next(0, (int)dungeonDimensions.X);
                Y = stateSpaceComponents.random.Next(0, (int)dungeonDimensions.Y);
            } while (dungeonGrid[X, Y].Type != TileType.TILE_FLOOR);
            stateSpaceComponents.PositionComponents[id] = new PositionComponent() { Position = new Vector2(X, Y) };
            dungeonGrid[X, Y].Occupiable = true;
            if(stateComponents != null)
            {
                GameplayInfoComponent info = stateComponents.GameplayInfo;
                info.FloorsReached += 1;
                stateSpaceComponents.GameplayInfoComponents[id] = info;
                stateSpaceComponents.SkillLevelsComponents[id] = stateComponents.PlayerSkillLevels;
            }
            else
            {

                //Set GameplayInfo
                stateSpaceComponents.GameplayInfoComponents[id] = new GameplayInfoComponent() { Kills = 0, StepsTaken = 0, FloorsReached = 0 };
                //Set Skills Level
                stateSpaceComponents.SkillLevelsComponents[id] = new SkillLevelsComponent()
                {
                    CurrentHealth = 100,
                    Health = 100,
                    MagicAttack = 10,
                    MagicDefense = 3,
                    PhysicalAttack = 20,
                    PhysicalDefense = 10,
                    Wealth = 100
                };
            }
            //Set Display
            stateSpaceComponents.DisplayComponents[id] = new DisplayComponent() { Color = Color.White, SpriteSource = new Rectangle(2 * cellSize, 0 * cellSize, cellSize, cellSize) };
            //Set Sightradius
            stateSpaceComponents.SightRadiusComponents[id] = new SightRadiusComponent() { Radius = 8 };
        }

        private void CreateMessageLog()
        {
            Guid id = stateSpaceComponents.CreateEntity();
            stateSpaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = Component.COMPONENT_GAMEMESSAGE;
            stateSpaceComponents.GameMessageComponents[id] = new GameMessageComponent() { GlobalColor = Color.White, GlobalMessage = string.Empty,
                 MaxMessages = 100, IndexBegin = 0, GameMessages = new List<Tuple<Color,string>>()};
            MessageDisplaySystem.GenerateRandomGameMessage(stateSpaceComponents, Messages.CaveEntranceMessages, MessageColors.SpecialAction);
        }
        #endregion

        #region Update Logic
        public IStateSpace UpdateSpace(GameTime gameTime, ContentManager content, GraphicsDeviceManager graphics, KeyboardState prevKeyboardState, MouseState prevMouseState, GamePadState prevGamepadState, Camera camera, ref GameSettings gameSettings)
        {
            if(stateSpaceComponents.EntitiesToDelete.Count > 0)
            {
                foreach(Guid entity in stateSpaceComponents.EntitiesToDelete)
                {
                    stateSpaceComponents.DestroyEntity(entity);
                }
            }
            IStateSpace nextStateSpace = this;
            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                camera.Target = Vector2.Transform(Mouse.GetState().Position.ToVector2(), camera.GetInverseMatrix());
                camera.AttachedToPlayer = false;
                MessageDisplaySystem.SetRandomGlobalMessage(stateSpaceComponents, Messages.CameraDetatchedMessage);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                camera.AttachedToPlayer = true;
                MessageDisplaySystem.SetRandomGlobalMessage(stateSpaceComponents, new string[] { string.Empty });
            }

            #region Debug changing levels
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && prevKeyboardState.IsKeyUp(Keys.LeftShift))
            {
                nextStateSpace = new RandomlyGeneratedStateSpace(new CaveGeneration(), 75, 125);
                LevelChangeSystem.RetainPlayerStatistics(stateComponents, stateSpaceComponents);
            }
            #endregion

            InputMovementSystem.HandleDungeonMovement(stateSpaceComponents, graphics, gameTime, prevKeyboardState, prevMouseState, prevGamepadState, camera, dungeonGrid, gameSettings);
            TileRevealSystem.RevealTiles(ref dungeonGrid, dungeonDimensions, stateSpaceComponents);
            TileRevealSystem.IncreaseTileOpacity(ref dungeonGrid, dungeonDimensions, gameTime);
            UpdateCamera(camera, gameTime);
            MessageDisplaySystem.ScrollMessage(prevKeyboardState, Keyboard.GetState(), stateSpaceComponents);
            stateSpaceComponents.EntitiesToDelete.Clear();
            return nextStateSpace;
        }

        private void UpdateCamera(Camera camera, GameTime gameTime)
        {
            if(camera.AttachedToPlayer)
            {
                Entity playerId = stateSpaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).FirstOrDefault();
                if(playerId != null)
                {
                    camera.Target = new Vector2((int)stateSpaceComponents.PositionComponents[playerId.Id].Position.X * cellSize + stateSpaceComponents.DisplayComponents[playerId.Id].SpriteSource.Width/2,
                    (int)stateSpaceComponents.PositionComponents[playerId.Id].Position.Y * cellSize + stateSpaceComponents.DisplayComponents[playerId.Id].SpriteSource.Height/2);
                }
            }
            if (Vector2.Distance(camera.Position, camera.Target) > 0)
            {
                float distance = Vector2.Distance(camera.Position, camera.Target);
                Vector2 direction = Vector2.Normalize(camera.Target - camera.Position);
                float velocity = distance * 2.5f;
                camera.Position += direction * velocity * (camera.Scale >= 1 ? camera.Scale : 1) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (distance < 2.5)
                {
                    camera.Position = camera.Target;
                }
            }
        }
        #endregion

        #region Draw Logic
        public void DrawLevel(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, Camera camera)
        {
            DisplaySystem.DrawTiles(camera, spriteBatch, dungeonGrid, dungeonDimensions, cellSize, dungeonSprites, dungeonColorInfo);
            DisplaySystem.DrawDungeonEntities(stateSpaceComponents, camera, spriteBatch, sprites, cellSize);
        }

        public void DrawUserInterface(SpriteBatch spriteBatch, Camera camera)
        {
            spriteBatch.Draw(UI, camera.DungeonUIViewport.Bounds, Color.DarkSlateBlue);
            MessageDisplaySystem.WriteMessages(stateSpaceComponents, spriteBatch, camera, messageFont);
        }
        #endregion

        #region Save Logic
        public DungeonInfo GetSaveData()
        {
            return new DungeonInfo()
            {
                cellSize = this.cellSize,
                dungeonColorInfo = this.dungeonColorInfo,
                dungeonDimensions = this.dungeonDimensions,
                dungeonGrid = this.dungeonGrid,
                stateComponents = this.stateComponents,
                dungeonSpriteFile = this.dungeonSpriteFile,
                stateSpaceComponents = this.stateSpaceComponents
            };
        }
        #endregion
    }
}
