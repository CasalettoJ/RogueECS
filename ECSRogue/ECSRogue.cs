using ECSRogue.BaseEngine;
using ECSRogue.BaseEngine.Interfaces;
using ECSRogue.BaseEngine.States;
using ECSRogue.BaseEngine.StateSpaces;
using ECSRogue.ProceduralGeneration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        private Stack<IState> stateStack;
        private IState currentState;
        private Camera gameCamera;
        private static readonly Vector2 _initialScale = new Vector2(1920, 1080);
        private static readonly Vector2 _initialSize = new Vector2(1024, 576);

        public ECSRogue()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            stateStack = new Stack<IState>();
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

            this.IsMouseVisible = true;
            Window.AllowUserResizing = true;
            graphics.PreferredBackBufferWidth = (int)_initialSize.X;
            graphics.PreferredBackBufferHeight = (int)_initialSize.Y;
            graphics.ApplyChanges();
            
            gameCamera = new Camera(Vector2.Zero, Vector2.Zero, 0.0f, _initialScale, graphics);
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
            RandomlyGeneratedStateSpace firstStateSpace = new RandomlyGeneratedStateSpace(new CaveGeneration(), 75, 125);
            PlayingState firstState = new PlayingState(firstStateSpace, gameCamera, Content, graphics);
            stateStack.Push(firstState);
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
            currentState = stateStack.Peek();
            IState nextState = currentState.UpdateContent(gameTime, gameCamera);
            if (nextState != currentState && nextState != null)
            {
                stateStack.Push(nextState);
            }
            else if (nextState == null)
            {
                stateStack.Pop();
            }
            if (stateStack.Count == 0)
            {
                Exit();
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
            spriteBatch.Begin(transformMatrix: gameCamera.GetMatrix(), samplerState: SamplerState.PointClamp);
            currentState.DrawContent(spriteBatch, gameCamera);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
