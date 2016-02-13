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
using ECSRogue.ECS.Components.AIComponents;
using ECSRogue.ECS.Components.MeleeMessageComponents;

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
        private Vector2 dungeonDimensions;
        private DungeonTile[,] dungeonGrid = null;
        private List<Vector2> freeTiles;
        private int cellSize;
        private string dungeonSpriteFile;
        private DungeonColorInfo dungeonColorInfo;
        private DijkstraMapTile[,] mapToPlayer;
        #endregion

        #region Constructors
        public RandomlyGeneratedStateSpace(IGenerationAlgorithm dungeonGeneration, int worldMin, int worldMax)
        {
            stateSpaceComponents = new StateSpaceComponents();
            freeTiles = new List<Vector2>();
            dungeonDimensions = dungeonGeneration.GenerateDungeon(ref dungeonGrid, worldMin, worldMax, stateSpaceComponents.random, freeTiles);
            cellSize = dungeonGeneration.GetCellsize();
            dungeonSpriteFile = dungeonGeneration.GetDungeonSpritesheetFileName();
            dungeonColorInfo = dungeonGeneration.GetColorInfo();
            CreateGameplayInfo();
            CreateMessageLog();
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
            freeTiles = data.freeTiles;
        }
        #endregion

        #region Load Logic
        public void LoadLevel(ContentManager content, GraphicsDeviceManager graphics, Camera camera, StateComponents stateComponents, bool createEntities = true)
        {
            this.stateComponents = stateComponents;
            sprites = content.Load<Texture2D>("Sprites/Ball");
            dungeonSprites = content.Load<Texture2D>(dungeonSpriteFile);
            messageFont = content.Load<SpriteFont>("Fonts/InfoText");
            asciiDisplay = content.Load<SpriteFont>("Fonts/DisplayText");
            UI = content.Load<Texture2D>("Sprites/ball");
            if (createEntities)
            {
                CreatePlayer();
            }
            camera.AttachedToPlayer = true;
            mapToPlayer = new DijkstraMapTile[(int)dungeonDimensions.X, (int)dungeonDimensions.Y];
        }

        private void CreateGameplayInfo()
        {
            if (stateComponents != null)
            {
                GameplayInfoComponent info = stateComponents.GameplayInfo;
                info.FloorsReached += 1;
                stateSpaceComponents.GameplayInfoComponent = info;
            }
            else
            {
                //Set GameplayInfo
                stateSpaceComponents.GameplayInfoComponent = new GameplayInfoComponent() { Kills = 0, StepsTaken = 0, FloorsReached = 1, Madness = 0 };
            }
        }

        private void CreatePlayer()
        {
            Guid id = stateSpaceComponents.CreateEntity();
            stateSpaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.Player | Component.COMPONENT_INPUTMOVEMENT | Component.COMPONENT_HEALTH_REGENERATION;
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
                stateSpaceComponents.SkillLevelsComponents[id] = stateComponents.PlayerSkillLevels;
            }
            else
            {
                stateSpaceComponents.SkillLevelsComponents[id] = new SkillLevelsComponent()
                {
                    CurrentHealth = 100,
                    Health = 100,
                    Power = 10,
                    Defense = 50,
                    Accuracy = 100,
                    Wealth = 0
                };
            }
            //Set Display
            stateSpaceComponents.DisplayComponents[id] = new DisplayComponent() { Color = Color.Wheat, SpriteSource = new Rectangle(0 * cellSize, 0 * cellSize, cellSize, cellSize),
                Origin = Vector2.Zero, SpriteEffect = SpriteEffects.None, Scale = 1f, Rotation = 0f };
            //Set Sightradius
            stateSpaceComponents.SightRadiusComponents[id] = new SightRadiusComponent() { CurrentRadius = 15, MaxRadius = 15, DrawRadius = true };
            //Set first turn
            stateSpaceComponents.PlayerComponent = new PlayerComponent() { PlayerJustLoaded = true };
            //Collision information
            stateSpaceComponents.CollisionComponents[id] = new CollisionComponent() { CollidedObjects = new List<Guid>(), Solid = true };
            //Set name of player
            stateSpaceComponents.NameComponents[id] = new NameComponent() { Name = "You" };
            //Set Input of the player
            stateSpaceComponents.InputMovementComponents[id] = new InputMovementComponent() { TimeIntervalBetweenMovements = .09f, TimeSinceLastMovement = 0f, InitialWait = .5f, TotalTimeButtonDown = 0f, LastKeyPressed = Keys.None };
            //Set an alignment for AI to communicate with
            stateSpaceComponents.AIAlignmentComponents[id] = new AIAlignment() { Alignment = AIAlignments.ALIGNMENT_FRIENDLY };
            //Set health regeneration
            stateSpaceComponents.HealthRegenerationComponents[id] = new HealthRegenerationComponent() { HealthRegain = 1, RegenerateTurnRate = 1, TurnsSinceLastHeal = 0 };
            //Set combat messages
            stateSpaceComponents.DodgeMeleeMessageComponents[id] = new DodgeMeleeMessageComponent()
            {
                NormalDodgeMessages = new string[]
                {
                    " and the attack misses you!",
                    " but nothing happened.",
                    " ... but it failed!",
                    " and your defense protects you.",
                    " but it fails to connect."
                },
                StreakDodgeMessages = new string[]
                {
                    " but you don't even notice.",
                    " and you laugh at the attempt.",
                    " but you easily dodge it again.",
                    " and misses you. Again!"
                }
            };
            stateSpaceComponents.MeleeAttackNPCMessageComponents[id] = new MeleeAttackNPCMessageComponent()
            {
                AttackNPCMessages = new string[]
                {
                    "{0} attack the {1}",
                    "{0} take a swing at the {1}",
                    "{0} swipe at {1}",
                    "{0} try to damage the {1}",
                    "{0} slash viciously at the {1}"
                }
            };
            stateSpaceComponents.TakeMeleeDamageMesageComponents[id] = new TakeMeleeDamageMesageComponent()
            {
                NormalTakeDamageMessages = new string[]
                {
                    " and you take {0} damage.",
                    " and it hurts! You take {0} damage.",
                    "! Ow. You lose {0} health.",
                    " and hits you for {0} damage."
                },
                BrokenDodgeStreakTakeDamageMessages = new string[]
                {
                    " and you finally take {0} damage.",
                    " and this time you lose {0} health! Ow!",
                    " and hits you for {0} THIS time.",
                    "! {0} damage taken! Don't get cocky..."
                }
            };
        }

        private void CreateMessageLog()
        {
            Guid id = stateSpaceComponents.CreateEntity();
            stateSpaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = Component.COMPONENT_GAMEMESSAGE;
            stateSpaceComponents.GameMessageComponent = new GameMessageComponent() { GlobalColor = Color.White, GlobalMessage = string.Empty,
                 MaxMessages = 100, IndexBegin = 0, GameMessages = new List<Tuple<Color,string>>()};
            if (stateSpaceComponents.GameplayInfoComponent.FloorsReached <= 1)
            {
                MessageDisplaySystem.GenerateRandomGameMessage(stateSpaceComponents, Messages.GameEntranceMessages, Colors.Messages.Special);
            }
            else
            {
                MessageDisplaySystem.GenerateRandomGameMessage(stateSpaceComponents, Messages.CaveEntranceMessages, Colors.Messages.Special);
            }

            stateSpaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.Normal, "This is a normal message."));
            stateSpaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.Special, "This is a SPECIAL message."));
            stateSpaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.StatusChange, "A status change has happened!"));
            stateSpaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.Good, "Something good happened."));
            stateSpaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.Bad, "Something awful happened!"));
            stateSpaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.LootPickup, "You picked up cool loot!"));
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

            //Deletion and Cleanup
            if (stateSpaceComponents.EntitiesToDelete.Count > 0)
            {
                foreach(Guid entity in stateSpaceComponents.EntitiesToDelete)
                {
                    stateSpaceComponents.DestroyEntity(entity);
                }
                stateSpaceComponents.EntitiesToDelete.Clear();
            }
            DestructionSystem.UpdateDestructionTimes(stateSpaceComponents, gameTime);

            //Non-turn-based
            AnimationSystem.UpdateFovColors(stateSpaceComponents, gameTime);
            MovementSystem.UpdateMovingEntities(stateSpaceComponents, gameTime);
            MovementSystem.UpdateIndefinitelyMovingEntities(stateSpaceComponents, gameTime);

            //Movement and Reaction
            InputMovementSystem.HandleDungeonMovement(stateSpaceComponents, graphics, gameTime, prevKeyboardState, prevMouseState, prevGamepadState, camera, dungeonGrid, gameSettings);
            CameraSystem.UpdateCamera(camera, gameTime, stateSpaceComponents, cellSize, prevKeyboardState);
            TileRevealSystem.RevealTiles(ref dungeonGrid, dungeonDimensions, stateSpaceComponents);
            TileRevealSystem.IncreaseTileOpacity(ref dungeonGrid, dungeonDimensions, gameTime, stateSpaceComponents);
            MessageDisplaySystem.ScrollMessage(prevKeyboardState, Keyboard.GetState(), stateSpaceComponents);
            DungeonMappingSystem.ShouldPlayerMapRecalc(stateSpaceComponents, dungeonGrid, dungeonDimensions, ref mapToPlayer);

            //AI and Combat
            AISystem.AICheckDetection(stateSpaceComponents);
            AISystem.AIMovement(stateSpaceComponents, dungeonGrid, dungeonDimensions, mapToPlayer);
            AISystem.AIUpdateVision(stateSpaceComponents, dungeonGrid, dungeonDimensions);
            CombatSystem.HandleMeleeCombat(stateSpaceComponents, cellSize);
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
            return nextStateSpace;
        }  
        #endregion

        #region Draw Logic
        public void DrawLevel(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, Camera camera)
        {
            DisplaySystem.DrawTiles(camera, spriteBatch, dungeonGrid, dungeonDimensions, cellSize, dungeonSprites, dungeonColorInfo);
            DisplaySystem.DrawAIFieldOfViews(stateSpaceComponents, camera, spriteBatch, UI, cellSize, dungeonGrid);
            DisplaySystem.DrawDungeonEntities(stateSpaceComponents, camera, spriteBatch, sprites, cellSize, dungeonGrid, asciiDisplay);
            LabelDisplaySystem.DrawString(spriteBatch, stateSpaceComponents, messageFont, camera);
        }

        public void DrawUserInterface(SpriteBatch spriteBatch, Camera camera)
        {
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
                stateSpaceComponents = this.stateSpaceComponents,
                freeTiles = this.freeTiles
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
