using ECSRogue.BaseEngine;
using ECSRogue.BaseEngine.Interfaces;
using ECSRogue.BaseEngine.IO;
using ECSRogue.BaseEngine.IO.Objects;
using ECSRogue.BaseEngine.States;
using ECSRogue.BaseEngine.StateSpaces;
using ECSRogue.ProceduralGeneration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace ECSRogue
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class ECSRogue : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private IState currentState;
        private Camera gameCamera;
        private KeyboardState prevKey;
        private SpriteFont debugText;
        private GameSettings gameSettings;

        public ECSRogue()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            FileIO.LoadGameSettings(ref gameSettings);
            this.Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            gameCamera = new Camera(Vector2.Zero, Vector2.Zero, 0.0f, gameSettings.Scale, graphics);
            this.IsMouseVisible = true;
            this.Window.IsBorderless = gameSettings.Borderless;
            Window.AllowUserResizing = false;

            this.ResetGameSettings();
            currentState = new TitleState(gameCamera, Content, graphics);

            debugText = Content.Load<SpriteFont>("Fonts/InfoText");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            try
            {
                currentState = currentState.UpdateContent(gameTime, gameCamera, ref gameSettings);
            }
            catch
            {
                Environment.Exit(0); // When laptop is unplugged game.exit() doesn't work...
            }
            prevKey = Keyboard.GetState();
            if(currentState == null)
            {
                Exit();
            }
            if(gameSettings.HasChanges)
            {
                ResetGameSettings();
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            //Draw entities
            spriteBatch.Begin(transformMatrix: gameCamera.GetMatrix(), samplerState: SamplerState.PointClamp);
            currentState.DrawContent(spriteBatch, gameCamera);
            spriteBatch.End();
            //Draw UI
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            currentState.DrawUserInterface(spriteBatch, gameCamera);
            //spriteBatch.DrawString(debugText, (1 / (float)gameTime.ElapsedGameTime.TotalSeconds).ToString(), gameCamera.Bounds.Center.ToVector2(), Color.Yellow);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void ResetGameSettings()
        {
            graphics.PreferredBackBufferWidth = (int)gameSettings.Resolution.X;
            graphics.PreferredBackBufferHeight = (int)gameSettings.Resolution.Y;
            graphics.SynchronizeWithVerticalRetrace = gameSettings.Vsync;
            this.IsFixedTimeStep = gameSettings.Vsync;
            graphics.ApplyChanges();
            gameCamera.ResetCamera(gameCamera.Position, Vector2.Zero, 0f, gameSettings.Scale, graphics);
            gameSettings.HasChanges = false;
            this.Window.Position = new Point((int)graphics.GraphicsDevice.DisplayMode.Width/2 - (int)gameSettings.Resolution.X/2, (int)graphics.GraphicsDevice.DisplayMode.Height / 2 - (int)gameSettings.Resolution.Y / 2);
            this.Window.IsBorderless = gameSettings.Borderless;
        }

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            this.ResetGameSettings();
        }
    }
}
