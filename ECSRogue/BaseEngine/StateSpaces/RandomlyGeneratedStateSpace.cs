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
using ECSRogue.ECS.Components.GraphicalEffectsComponents;
using ECSRogue.ECS.Components.ItemizationComponents;

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
        private Texture2D dungeonSprites;
        private Texture2D UI;
        private SpriteFont messageFont;
        private SpriteFont asciiDisplay;
        private SpriteFont optionFont;
        private Vector2 dungeonDimensions;
        private DungeonTile[,] dungeonGrid = null;
        private List<Vector2> freeTiles;
        private string dungeonSpriteFile;
        private DungeonColorInfo dungeonColorInfo;
        private DijkstraMapTile[,] mapToPlayer;
        private bool showInventory = false;
        private bool showObserver = false;
        #endregion

        #region Constructors
        public RandomlyGeneratedStateSpace(IGenerationAlgorithm dungeonGeneration, int worldMin, int worldMax)
        {
            stateSpaceComponents = new StateSpaceComponents();
            freeTiles = new List<Vector2>();
            dungeonDimensions = dungeonGeneration.GenerateDungeon(ref dungeonGrid, worldMin, worldMax, stateSpaceComponents.random, freeTiles);
            dungeonSpriteFile = dungeonGeneration.GetDungeonSpritesheetFileName();
            dungeonColorInfo = dungeonGeneration.GetColorInfo();
        }

        public RandomlyGeneratedStateSpace(DungeonInfo data)
        {
            stateSpaceComponents = data.stateSpaceComponents;
            dungeonSpriteFile = data.dungeonSpriteFile;
            dungeonGrid = data.dungeonGrid;
            dungeonColorInfo = data.dungeonColorInfo;
            dungeonDimensions = data.dungeonDimensions;
            freeTiles = data.freeTiles;
            PlayerComponent player = stateSpaceComponents.PlayerComponent;
            player.PlayerJustLoaded = true;
            stateSpaceComponents.PlayerComponent = player;
        }
        #endregion

        #region Load Logic
        public void LoadLevel(ContentManager content, GraphicsDeviceManager graphics, Camera camera, StateComponents stateComponents, bool createEntities = true)
        {
            this.stateComponents = stateComponents;
            sprites = content.Load<Texture2D>(DevConstants.Graphics.SpriteSheet);
            dungeonSprites = content.Load<Texture2D>(dungeonSpriteFile);
            messageFont = content.Load<SpriteFont>(DevConstants.Graphics.MessageFont);
            asciiDisplay = content.Load<SpriteFont>(DevConstants.Graphics.AsciiFont);
            optionFont = content.Load<SpriteFont>(DevConstants.Graphics.OptionFont);
            UI = content.Load<Texture2D>(DevConstants.Graphics.UISheet);
            camera.AttachedToPlayer = true;
            if(createEntities)
            {
                LevelChangeSystem.CreateGameplayInfo(stateComponents, stateSpaceComponents);
                DungeonCreationSystem.CreateDungeonDrops(stateSpaceComponents, dungeonGrid, dungeonDimensions, freeTiles);
                DungeonCreationSystem.CreateDungeonMonsters(stateSpaceComponents, dungeonGrid, dungeonDimensions, DevConstants.Grid.CellSize, freeTiles);
                LevelChangeSystem.LoadPlayerSkillset(stateComponents, stateSpaceComponents);
                LevelChangeSystem.CreateMessageLog(stateSpaceComponents);
            }
            mapToPlayer = new DijkstraMapTile[(int)dungeonDimensions.X, (int)dungeonDimensions.Y];
        }
        #endregion

        #region Update Logic
        public IStateSpace UpdateSpace(GameTime gameTime, ContentManager content, GraphicsDeviceManager graphics, KeyboardState prevKeyboardState, MouseState prevMouseState, GamePadState prevGamepadState, Camera camera, ref GameSettings gameSettings)
        {
            IStateSpace nextStateSpace = this;
            #region Debug
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && prevKeyboardState.IsKeyUp(Keys.LeftShift))
            {
                nextStateSpace = new RandomlyGeneratedStateSpace(new CaveGeneration(), 75, 125);
                LevelChangeSystem.RetainPlayerStatistics(stateComponents, stateSpaceComponents);
            }
            #endregion
            //Check to see if the next level needs to be loaded
            if(stateSpaceComponents.PlayerComponent.GoToNextFloor)
            {
                nextStateSpace = new RandomlyGeneratedStateSpace(new CaveGeneration(), 75, 125);
                LevelChangeSystem.RetainPlayerStatistics(stateComponents, stateSpaceComponents);
            }
            //Toggle Inventory Menu
            if (Keyboard.GetState().IsKeyDown(Keys.I) && prevKeyboardState.IsKeyUp(Keys.I) && !showObserver)
            {
                showInventory = !showInventory;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Enter) && prevKeyboardState.IsKeyUp(Keys.Enter) && !showInventory)
            {
                //If observer exists, remove it and add input component to player(s), otherwise, remove input component from all players and create an observer.
                if (ObserverSystem.CreateOrDestroyObserver(stateSpaceComponents))
                {
                    showObserver = true;
                }
                else
                {
                    showObserver = false;
                }
            }

            //Actions to complete if the inventory is open
            if (showInventory)
            {
                //Deletion and Cleanup
                if (stateSpaceComponents.EntitiesToDelete.Count > 0)
                {
                    foreach (Guid entity in stateSpaceComponents.EntitiesToDelete)
                    {
                        stateSpaceComponents.DestroyEntity(entity);
                    }
                    stateSpaceComponents.EntitiesToDelete.Clear();
                }
                showInventory = InventorySystem.HandleInventoryInput(stateSpaceComponents, gameTime, prevKeyboardState, Keyboard.GetState());
            }
            //Actions to complete if inventory is not open
            if (showObserver)
            {
                ObserverComponent observer = stateSpaceComponents.ObserverComponent;
                observer.Observed = new List<Guid>();
                stateSpaceComponents.ObserverComponent = observer;
                InputMovementSystem.HandleDungeonMovement(stateSpaceComponents, graphics, gameTime, prevKeyboardState, prevMouseState, prevGamepadState, camera, dungeonGrid, dungeonDimensions);
                CameraSystem.UpdateCamera(camera, gameTime, stateSpaceComponents, DevConstants.Grid.CellSize, prevKeyboardState);
                ObserverSystem.HandleObserverFindings(stateSpaceComponents, Keyboard.GetState(), prevKeyboardState, dungeonGrid);
                stateSpaceComponents.InvokeDelayedActions();
            }
            else if (!showInventory && !showObserver)
            {
                //Deletion and Cleanup
                if (stateSpaceComponents.EntitiesToDelete.Count > 0)
                {
                    foreach (Guid entity in stateSpaceComponents.EntitiesToDelete)
                    {
                        stateSpaceComponents.DestroyEntity(entity);
                    }
                    stateSpaceComponents.EntitiesToDelete.Clear();
                }
                DestructionSystem.UpdateDestructionTimes(stateSpaceComponents, gameTime);

                //Non-turn-based
                AnimationSystem.UpdateFovColors(stateSpaceComponents, gameTime);
                AnimationSystem.UpdateOutlineColors(stateSpaceComponents, gameTime);
                MovementSystem.UpdateMovingEntities(stateSpaceComponents, gameTime);
                MovementSystem.UpdateIndefinitelyMovingEntities(stateSpaceComponents, gameTime);

                //Movement and Reaction
                InputMovementSystem.HandleDungeonMovement(stateSpaceComponents, graphics, gameTime, prevKeyboardState, prevMouseState, prevGamepadState, camera, dungeonGrid, dungeonDimensions);
                CameraSystem.UpdateCamera(camera, gameTime, stateSpaceComponents, DevConstants.Grid.CellSize, prevKeyboardState);
                TileRevealSystem.RevealTiles(ref dungeonGrid, dungeonDimensions, stateSpaceComponents);
                TileRevealSystem.IncreaseTileOpacity(ref dungeonGrid, dungeonDimensions, gameTime, stateSpaceComponents);
                MessageDisplaySystem.ScrollMessage(prevKeyboardState, Keyboard.GetState(), stateSpaceComponents);
                DungeonMappingSystem.ShouldPlayerMapRecalc(stateSpaceComponents, dungeonGrid, dungeonDimensions, ref mapToPlayer);

                //AI and Combat
                AISystem.AICheckDetection(stateSpaceComponents);
                AISystem.AIMovement(stateSpaceComponents, dungeonGrid, dungeonDimensions, mapToPlayer);
                InventorySystem.TryPickupItems(stateSpaceComponents, dungeonGrid);
                AISystem.AIUpdateVision(stateSpaceComponents, dungeonGrid, dungeonDimensions);
                CombatSystem.HandleMeleeCombat(stateSpaceComponents, DevConstants.Grid.CellSize);
                AISystem.AICheckFleeing(stateSpaceComponents);
                CombatSystem.RegenerateHealth(stateSpaceComponents);

                //Resetting Systems
                if (stateSpaceComponents.PlayerComponent.PlayerJustLoaded || stateSpaceComponents.PlayerComponent.PlayerTookTurn)
                {
                    PlayerComponent player = stateSpaceComponents.PlayerComponent;
                    player.PlayerJustLoaded = false;
                    player.PlayerTookTurn = false;
                    stateSpaceComponents.PlayerComponent = player;
                }
                CollisionSystem.ResetCollision(stateSpaceComponents);
                stateSpaceComponents.InvokeDelayedActions();
            }
            return nextStateSpace;
        }  
        #endregion

        #region Draw Logic
        public void DrawLevel(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, Camera camera, ref GameSettings gameSettings)
        {
            DisplaySystem.DrawTiles(camera, spriteBatch, dungeonGrid, dungeonDimensions, DevConstants.Grid.CellSize, dungeonSprites, dungeonColorInfo);
            DisplaySystem.DrawAIFieldOfViews(stateSpaceComponents, camera, spriteBatch, UI, DevConstants.Grid.CellSize, dungeonGrid);
            if(gameSettings.ShowGlow)
            {
                DisplaySystem.DrawOutlines(stateSpaceComponents, camera, spriteBatch, UI, dungeonGrid);
            }
            DisplaySystem.DrawDungeonEntities(stateSpaceComponents, camera, spriteBatch, sprites, DevConstants.Grid.CellSize, dungeonGrid, asciiDisplay);
            LabelDisplaySystem.DrawString(spriteBatch, stateSpaceComponents, messageFont, camera);
        }

        public void DrawUserInterface(SpriteBatch spriteBatch, Camera camera)
        {
            if(showInventory)
            {
                InventorySystem.ShowInventoryMenu(stateSpaceComponents, spriteBatch, camera, messageFont, optionFont, UI);
            }
            //Don't show observer findings if the inventory screen is open
            else
            {
                ObserverSystem.PrintObserver(stateSpaceComponents, messageFont, spriteBatch, dungeonGrid, camera, UI);
                spriteBatch.Draw(UI, camera.DungeonUIViewport.Bounds, Color.Black);
                //spriteBatch.Draw(UI, camera.DungeonUIViewport.Bounds, Color.DarkSlateBlue * .3f);
                //spriteBatch.Draw(UI, new Rectangle(new Point(camera.DungeonUIViewport.Bounds.X, camera.DungeonUIViewport.Bounds.Y), new Point(camera.DungeonUIViewport.Bounds.Width, 3)), Color.DarkSlateBlue);
                //spriteBatch.Draw(UI, new Rectangle(new Point(camera.DungeonUIViewport.Bounds.X, camera.DungeonUIViewport.Bounds.Y+5), new Point(camera.DungeonUIViewport.Bounds.Width, camera.DungeonUIViewport.Bounds.Height - 5)), Color.DarkSlateBlue * .3f);
                spriteBatch.Draw(UI, camera.DungeonUIViewportLeft.Bounds, Color.Black);
                //spriteBatch.Draw(UI, camera.DungeonUIViewportLeft.Bounds, Color.DarkOrange * .15f);
                //spriteBatch.Draw(UI, new Rectangle(new Point(camera.DungeonUIViewportLeft.Bounds.X, camera.DungeonUIViewportLeft.Bounds.Y), new Point(3, camera.DungeonUIViewportLeft.Bounds.Height - camera.DungeonUIViewport.Bounds.Height + 3)), Color.DarkSlateBlue);
                //spriteBatch.Draw(UI, new Rectangle(new Point(camera.DungeonUIViewportLeft.Bounds.X, camera.DungeonUIViewportLeft.Bounds.Y), new Point(camera.DungeonUIViewportLeft.Bounds.Width, camera.DungeonUIViewportLeft.Bounds.Height)), Color.DarkSlateBlue * .3f);
                MessageDisplaySystem.WriteMessages(stateSpaceComponents, spriteBatch, camera, messageFont);
            }
        }
        #endregion

        #region Save Logic
        public DungeonInfo GetSaveData()
        {
            return new DungeonInfo()
            {
                dungeonColorInfo = this.dungeonColorInfo,
                dungeonDimensions = this.dungeonDimensions,
                dungeonGrid = this.dungeonGrid,
                stateComponents = this.stateComponents,
                dungeonSpriteFile = this.dungeonSpriteFile,
                stateSpaceComponents = this.stateSpaceComponents,
                freeTiles = this.freeTiles
            };
        }
        #endregion
    }
}
