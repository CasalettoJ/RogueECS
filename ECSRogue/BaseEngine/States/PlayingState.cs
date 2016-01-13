using ECSRogue.BaseEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace ECSRogue.BaseEngine.States
{
    public class PlayingState : IState
    {
        #region State Private Variables
        private ContentManager Content;
        private MouseState PrevMouseState;
        private GamePadState PrevGamepadState;
        private KeyboardState PrevKeyboardState;
        private IStateSpace CurrentLevel;
        private GraphicsDeviceManager Graphics;
        #endregion

        public PlayingState(IStateSpace space, Camera camera, ContentManager content, GraphicsDeviceManager graphics, MouseState mouseState = new MouseState(), GamePadState gamePadState = new GamePadState(), KeyboardState keyboardState = new KeyboardState())
        {
            this.Content = new ContentManager(content.ServiceProvider, "Content");
            Graphics = graphics;
            PrevMouseState = mouseState;
            PrevGamepadState = gamePadState;
            PrevKeyboardState = keyboardState;
            SetStateSpace(space, camera);
        }

        ~PlayingState()
        {
            if (Content != null) { Content.Unload(); }
        }

        public IState UpdateContent(GameTime gameTime, Camera camera)
        {
            IStateSpace nextLevel = CurrentLevel;
            nextLevel = CurrentLevel.UpdateLevel(gameTime, Content, Graphics, PrevKeyboardState, PrevMouseState, PrevGamepadState, camera);
            if (nextLevel != CurrentLevel && nextLevel != null)
            {
                SetStateSpace(nextLevel, camera);
            }
            if (nextLevel == null)
            {
                return null;
            }

            PrevKeyboardState = Keyboard.GetState();
            PrevMouseState = Mouse.GetState();
            PrevGamepadState = GamePad.GetState(PlayerIndex.One);
            return this;
        }

        public void DrawContent(SpriteBatch spriteBatch, Camera camera)
        {
            CurrentLevel.DrawLevel(spriteBatch, Graphics, camera);
        }

        public void SetStateSpace(IStateSpace stateSpace, Camera camera)
        {
            if (Content != null && stateSpace != null)
            {
                Content.Unload();

                CurrentLevel = stateSpace;
                stateSpace.LoadLevel(Content, Graphics, camera);
            }
        }

        public void DrawUserInterface(SpriteBatch spriteBatch, Camera camera)
        {
            CurrentLevel.DrawUserInterface(spriteBatch, camera);
        }
    }
}
