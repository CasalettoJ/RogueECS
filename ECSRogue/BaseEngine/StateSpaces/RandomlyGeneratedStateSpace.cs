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
        private List<Vector2> waterTiles;
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
            waterTiles = new List<Vector2>();
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
            if(stateComponents.StateSpaceComponents != null)
            {
                this.stateSpaceComponents = stateComponents.StateSpaceComponents;
            }
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
                DungeonCreationSystem.TallGrassGeneration(ref dungeonGrid, dungeonDimensions, stateSpaceComponents.random, freeTiles, stateSpaceComponents);
                DungeonCreationSystem.WaterGeneration(ref dungeonGrid, dungeonDimensions, stateSpaceComponents.random, freeTiles, stateSpaceComponents, waterTiles);
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


            //Check to see if the player has died
            if(stateSpaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).Count() == 0)
            {
                //Game End, High Score, and Save Data handling
            }
            else
            {//Check to see if the next level needs to be loaded
                if (stateSpaceComponents.PlayerComponent.GoToNextFloor || Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                {
                    nextStateSpace = new RandomlyGeneratedStateSpace(new CaveGeneration(), 75, 125);
                    PlayerComponent player = stateSpaceComponents.PlayerComponent;
                    player.GoToNextFloor = false;
                    player.PlayerJustLoaded = true;
                    stateSpaceComponents.PlayerComponent = player;
                    LevelChangeSystem.RetainPlayerStatistics(stateComponents, stateSpaceComponents);
                    LevelChangeSystem.RetainNecessaryComponents(stateComponents, stateSpaceComponents);
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
                    DestructionSystem.UpdateDestructionTimes(stateSpaceComponents, gameTime);

                    //Non-turn-based
                    AnimationSystem.UpdateFovColors(stateSpaceComponents, gameTime);
                    AnimationSystem.UpdateOutlineColors(stateSpaceComponents, gameTime);
                    MovementSystem.UpdateMovingEntities(stateSpaceComponents, gameTime);
                    MovementSystem.UpdateIndefinitelyMovingEntities(stateSpaceComponents, gameTime);

                    //Movement and Reaction
                    InputMovementSystem.HandleDungeonMovement(stateSpaceComponents, graphics, gameTime, prevKeyboardState, prevMouseState, prevGamepadState, camera, dungeonGrid, dungeonDimensions);
                    CameraSystem.UpdateCamera(camera, gameTime, stateSpaceComponents, DevConstants.Grid.CellSize, prevKeyboardState);
                    TileSystem.RevealTiles(ref dungeonGrid, dungeonDimensions, stateSpaceComponents);
                    TileSystem.IncreaseTileOpacity(ref dungeonGrid, dungeonDimensions, gameTime, stateSpaceComponents);
                    MessageDisplaySystem.ScrollMessage(prevKeyboardState, Keyboard.GetState(), stateSpaceComponents);
                    DungeonMappingSystem.ShouldPlayerMapRecalc(stateSpaceComponents, dungeonGrid, dungeonDimensions, ref mapToPlayer);

                    //AI and Combat
                    AISystem.AICheckDetection(stateSpaceComponents);
                    AISystem.AIMovement(stateSpaceComponents, dungeonGrid, dungeonDimensions, mapToPlayer);
                    InventorySystem.TryPickupItems(stateSpaceComponents, dungeonGrid);
                    AISystem.AIUpdateVision(stateSpaceComponents, dungeonGrid, dungeonDimensions);
                    CombatSystem.HandleMeleeCombat(stateSpaceComponents, DevConstants.Grid.CellSize);
                    AISystem.AICheckFleeing(stateSpaceComponents);

                    //End-Of-Turn Status Effects
                    StatusSystem.RegenerateHealth(stateSpaceComponents);
                    StatusSystem.ApplyBurnDamage(stateSpaceComponents, dungeonGrid);
                    TileSystem.SpreadFire(ref dungeonGrid, dungeonDimensions, stateSpaceComponents);

                    //Resetting Systems
                    if (stateSpaceComponents.PlayerComponent.PlayerJustLoaded || stateSpaceComponents.PlayerComponent.PlayerTookTurn)
                    {
                        PlayerComponent player = stateSpaceComponents.PlayerComponent;
                        player.PlayerJustLoaded = false;
                        player.PlayerTookTurn = false;
                        stateSpaceComponents.PlayerComponent = player;
                    }
                    CollisionSystem.ResetCollision(stateSpaceComponents);
                    if (stateSpaceComponents.EntitiesToDelete.Count > 0)
                    {
                        foreach (Guid entity in stateSpaceComponents.EntitiesToDelete)
                        {
                            stateSpaceComponents.DestroyEntity(entity);
                        }
                        stateSpaceComponents.EntitiesToDelete.Clear();
                    }
                    stateSpaceComponents.InvokeDelayedActions();
                }
            }
            
            return nextStateSpace;
        }  
        #endregion

        #region Draw Logic
        public void DrawLevel(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, Camera camera, ref GameSettings gameSettings)
        {
            DisplaySystem.DrawTiles(camera, spriteBatch, dungeonGrid, dungeonDimensions, DevConstants.Grid.CellSize, dungeonSprites, dungeonColorInfo, mapToPlayer, asciiDisplay, stateSpaceComponents);
            DisplaySystem.DrawAIFieldOfViews(stateSpaceComponents, camera, spriteBatch, UI, DevConstants.Grid.CellSize, dungeonGrid);
            if(gameSettings.ShowGlow)
            {
                DisplaySystem.DrawOutlines(stateSpaceComponents, camera, spriteBatch, UI, dungeonGrid);
            }
            DisplaySystem.DrawDungeonEntities(stateSpaceComponents, camera, spriteBatch, sprites, DevConstants.Grid.CellSize, dungeonGrid, asciiDisplay, dungeonColorInfo);
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
                spriteBatch.Draw(UI, camera.DungeonUIViewportLeft.Bounds, Color.Black);
                MessageDisplaySystem.WriteMessages(stateSpaceComponents, spriteBatch, camera, messageFont, dungeonGrid);
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
                freeTiles = this.freeTiles,
                waterTiles = this.waterTiles
            };
        }
        #endregion
    }
}
