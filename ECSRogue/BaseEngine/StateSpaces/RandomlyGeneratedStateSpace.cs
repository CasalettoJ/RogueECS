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
            List<Vector2> freeTiles = new List<Vector2>();
            dungeonDimensions = dungeonGeneration.GenerateDungeon(ref dungeonGrid, worldMin, worldMax, stateSpaceComponents.random, freeTiles);
            cellSize = dungeonGeneration.GetCellsize();
            dungeonSpriteFile = dungeonGeneration.GetDungeonSpritesheetFileName();
            dungeonColorInfo = dungeonGeneration.GetColorInfo();
            dungeonGeneration.GenerateDungeonEntities(stateSpaceComponents, dungeonGrid, dungeonDimensions, cellSize, freeTiles);
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
            stateSpaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.Player | Component.COMPONENT_INPUTMOVEMENT;
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
                stateSpaceComponents.GameplayInfoComponent = info;
                stateSpaceComponents.SkillLevelsComponents[id] = stateComponents.PlayerSkillLevels;
            }
            else
            {

                //Set GameplayInfo
                stateSpaceComponents.GameplayInfoComponent = new GameplayInfoComponent() { Kills = 0, StepsTaken = 0, FloorsReached = 0 };
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
            stateSpaceComponents.DisplayComponents[id] = new DisplayComponent() { Color = Color.White, SpriteSource = new Rectangle(2 * cellSize, 0 * cellSize, cellSize, cellSize), Origin = Vector2.Zero, SpriteEffect = SpriteEffects.None, Scale = 1f, Rotation = 0f };
            //Set Sightradius
            stateSpaceComponents.SightRadiusComponents[id] = new SightRadiusComponent() { Radius = 8 };
            //Set first turn
            stateSpaceComponents.PlayerComponent = new PlayerComponent() { PlayerJustLoaded = true };
            //Collision information
            stateSpaceComponents.CollisionComponents[id] = new CollisionComponent() { CollidedObjects = new List<Guid>(), Solid = true };
            //Set name of player
            stateSpaceComponents.NameComponents[id] = new NameComponent() { Name = "You" };

        }

        private void CreateMessageLog()
        {
            Guid id = stateSpaceComponents.CreateEntity();
            stateSpaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = Component.COMPONENT_GAMEMESSAGE;
            stateSpaceComponents.GameMessageComponent = new GameMessageComponent() { GlobalColor = Color.White, GlobalMessage = string.Empty,
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
                stateSpaceComponents.EntitiesToDelete.Clear();
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

            #region Debug
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && prevKeyboardState.IsKeyUp(Keys.LeftShift))
            {
                nextStateSpace = new RandomlyGeneratedStateSpace(new CaveGeneration(), 75, 125);
                LevelChangeSystem.RetainPlayerStatistics(stateComponents, stateSpaceComponents);
            }
            #endregion

            InputMovementSystem.HandleDungeonMovement(stateSpaceComponents, graphics, gameTime, prevKeyboardState, prevMouseState, prevGamepadState, camera, dungeonGrid, gameSettings);
            TileRevealSystem.RevealTiles(ref dungeonGrid, dungeonDimensions, stateSpaceComponents);
            TileRevealSystem.IncreaseTileOpacity(ref dungeonGrid, dungeonDimensions, gameTime, stateSpaceComponents);
            UpdateCamera(camera, gameTime);
            DestructionSystem.UpdateDestructionTimes(stateSpaceComponents, gameTime);
            MessageDisplaySystem.ScrollMessage(prevKeyboardState, Keyboard.GetState(), stateSpaceComponents);
            MovementSystem.UpdateMovingEntities(stateSpaceComponents, gameTime);
            MovementSystem.UpdateIndefinitelyMovingEntities(stateSpaceComponents, gameTime);
            CombatSystem.HandleMeleeCombat(stateSpaceComponents, cellSize);
            if(!stateSpaceComponents.PlayerComponent.PlayerJustLoaded)
            {
                PlayerComponent player = stateSpaceComponents.PlayerComponent;
                player.PlayerJustLoaded = false;
                stateSpaceComponents.PlayerComponent = player;
            }
            CollisionSystem.ResetCollision(stateSpaceComponents);
            stateSpaceComponents.InvokeDelayedActions();
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
            DisplaySystem.DrawDungeonEntities(stateSpaceComponents, camera, spriteBatch, sprites, cellSize, dungeonGrid);
            LabelDisplaySystem.DrawString(spriteBatch, stateSpaceComponents, messageFont, camera);
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


        #region Debugging
        //public void CreateDamageEntity(Camera camera)
        //{
        //    Guid id = stateSpaceComponents.CreateEntity();
        //    stateSpaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.DrawableLabel | ComponentMasks.MovingEntity;
        //    stateSpaceComponents.PositionComponents[id] = new PositionComponent() { Position = Vector2.Transform(Mouse.GetState().Position.ToVector2(), camera.GetInverseMatrix()) };
        //    stateSpaceComponents.LabelComponents[id] = new LabelComponent()
        //    {
        //        Color = Color.LightSalmon,
        //        Origin = Vector2.Zero,
        //        Rotation = 0f,
        //        Scale = 1.75f,
        //        SpriteEffect = SpriteEffects.None,
        //        Text = "-72"
        //    };
        //    stateSpaceComponents.VelocityComponents[id] = new VelocityComponent() { Velocity = new Vector2(stateSpaceComponents.random.Next(200,300), stateSpaceComponents.random.Next(200,300)) };
        //    stateSpaceComponents.TargetPositionComponents[id] = new TargetPositionComponent() { DestroyWhenReached = true, TargetPosition = new Vector2(stateSpaceComponents.PositionComponents[id].Position.X+ stateSpaceComponents.random.Next(-200,200), stateSpaceComponents.PositionComponents[id].Position.Y - 200) };
        //}
        #endregion
    }
}
