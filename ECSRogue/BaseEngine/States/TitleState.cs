using ECSRogue.BaseEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ECSRogue.BaseEngine.IO.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using ECSRogue.ECS;
using ECSRogue.BaseEngine.StateSpaces;
using ECSRogue.ProceduralGeneration;

namespace ECSRogue.BaseEngine.States
{
    public class TitleState : IState
    {
        private enum Options
        {
            NEW_GAME = 0,
            LOAD_GAME = 1,
            OPTIONS = 2,
            QUIT_GAME = 3
        }
        protected struct Option
        {
            public bool Enabled;
            public string Message;
        }
        private const int optionsAmount = 4;
        private int optionSelection;
        private SpriteFont titleText;
        private SpriteFont optionText;
        private Option[] menuOptions;
        private string Title;

        #region State Private Variables
        private ContentManager Content;
        private MouseState PrevMouseState;
        private GamePadState PrevGamepadState;
        private KeyboardState PrevKeyboardState;
        private GraphicsDeviceManager Graphics;
        private StateComponents StateComponents;
        #endregion

        public TitleState(Camera camera, ContentManager content, GraphicsDeviceManager graphics, MouseState mouseState = new MouseState(), GamePadState gamePadState = new GamePadState(), KeyboardState keyboardState = new KeyboardState())
        {
            this.Content = new ContentManager(content.ServiceProvider, "Content");
            Graphics = graphics;
            PrevMouseState = mouseState;
            PrevGamepadState = gamePadState;
            PrevKeyboardState = keyboardState;
            StateComponents = new StateComponents();
            titleText = content.Load<SpriteFont>("Fonts/TitleText");
            optionText = content.Load<SpriteFont>("Fonts/OptionText");
            optionSelection = 0;
            menuOptions = new Option[optionsAmount];
            Title = "PLACEHOLDER TITLE";
            menuOptions[0] = new Option() { Enabled = true, Message = "NEW GAME" };
            menuOptions[1] = new Option() { Enabled = false, Message = "CONTINUE" };
            menuOptions[2] = new Option() { Enabled = false, Message = "OPTIONS" };
            menuOptions[3] = new Option() { Enabled = true, Message = "QUIT GAME" };
        }

        public IState UpdateContent(GameTime gameTime, Camera camera, ref GameSettings gameSettings)
        {
            IState nextState = this;
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Up) && !PrevKeyboardState.IsKeyDown(Keys.Up))
            {
                optionSelection -= 1;
                if (optionSelection < 0)
                {
                    optionSelection = optionsAmount - 1;
                }
                if (optionSelection >= optionsAmount)
                {
                    optionSelection = 0;
                }
                while (!menuOptions[optionSelection].Enabled)
                {
                    optionSelection -= 1;
                }
            }
            if (keyState.IsKeyDown(Keys.Down) && !PrevKeyboardState.IsKeyDown(Keys.Down))
            {
                optionSelection += 1;
                if (optionSelection < 0)
                {
                    optionSelection = optionsAmount - 1;
                }
                if (optionSelection >= optionsAmount)
                {
                    optionSelection = 0;
                }
                while (!menuOptions[optionSelection].Enabled)
                {
                    optionSelection += 1;
                }
            }

            if (keyState.IsKeyDown(Keys.Enter))
            {
                switch (optionSelection)
                {
                    case (int)Options.NEW_GAME:
                        RandomlyGeneratedStateSpace nextStateSpace = new RandomlyGeneratedStateSpace(new CaveGeneration(), 75, 125);
                        nextState = new PlayingState(nextStateSpace, camera, Content, Graphics, keyboardState: PrevKeyboardState);
                        break;
                    case (int)Options.LOAD_GAME:
                        break;
                    case (int)Options.OPTIONS:
                        break;
                    case (int)Options.QUIT_GAME:
                        nextState = null;
                        break;
                }
            }

            PrevKeyboardState = Keyboard.GetState();
            PrevMouseState = Mouse.GetState();
            PrevGamepadState = GamePad.GetState(PlayerIndex.One);
            return nextState;
        }

        public void DrawContent(SpriteBatch spriteBatch, Camera camera)
        {
            //Ain't nothing here
        }

        public void DrawUserInterface(SpriteBatch spriteBatch, Camera camera)
        {
            int messageCount = 0;
            int messageSpacing = 50;
            Vector2 titleLength = titleText.MeasureString(Title);
            spriteBatch.DrawString(titleText, Title, new Vector2((camera.Viewport.Width / 2) - titleLength.X / 2, messageSpacing), Color.Goldenrod);
            foreach (Option option in menuOptions)
            {
                int stringLength = (int)optionText.MeasureString(option.Message).X;
                spriteBatch.DrawString(optionText, option.Message, new Vector2((camera.Viewport.Width / 2) - stringLength / 2, (camera.Viewport.Height / 2) + (messageCount * messageSpacing)),
                    option.Enabled ? messageCount == optionSelection ? Color.MediumPurple : Color.Goldenrod : Color.Gray);
                messageCount += 1;
            }
        }

        public void SetStateSpace(IStateSpace stateSpace, Camera camera)
        {
            //Nothing here.
        }
    }
}
