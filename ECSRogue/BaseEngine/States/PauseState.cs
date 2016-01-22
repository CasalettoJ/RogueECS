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
using ECSRogue.BaseEngine.IO;

namespace ECSRogue.BaseEngine.States
{
    public class PauseState : IState
    {
        #region State Management Property
        private static IState previousState;
        #endregion

        private enum Options
        {
            OPTIONS = 0,
            SAVE_TITLE = 1,
            UNPAUSE = 2
        }
        protected struct Option
        {
            public bool Enabled;
            public string Message;
        }
        private const int optionsAmount = 3;
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

        public PauseState(Camera camera, ContentManager content, GraphicsDeviceManager graphics, IState prevState, MouseState mouseState = new MouseState(), GamePadState gamePadState = new GamePadState(), KeyboardState keyboardState = new KeyboardState())
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
            Title = "GAME PAUSED";
            menuOptions[0] = new Option() { Enabled = true, Message = "OPTIONS" };
            menuOptions[1] = new Option() { Enabled = true, Message = "SAVE AND RETURN TO TITLE" };
            menuOptions[2] = new Option() { Enabled = true, Message = "UNPAUSE" };
            previousState = prevState;
        }

        ~PauseState()
        {
            if (Content != null) { Content.Unload(); }
        }

        public IState UpdateContent(GameTime gameTime, Camera camera, ref GameSettings gameSettings)
        {
            IState nextState = this;
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Escape) && PrevKeyboardState.IsKeyUp(Keys.Escape))
            {
                nextState = previousState;
                nextState.SetPrevInput(Keyboard.GetState(), Mouse.GetState(), GamePad.GetState(PlayerIndex.One));
            }
            else if (keyState.IsKeyDown(Keys.Up) && PrevKeyboardState.IsKeyUp(Keys.Up))
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
            else if (keyState.IsKeyDown(Keys.Down) && PrevKeyboardState.IsKeyUp(Keys.Down))
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

            else if (keyState.IsKeyDown(Keys.Enter) && PrevKeyboardState.IsKeyUp(Keys.Enter))
            {
                switch (optionSelection)
                {
                    case (int)Options.OPTIONS:
                        GameSettingsMenuStateSpace nextStateSpace = new GameSettingsMenuStateSpace(ref gameSettings);
                        nextState = new MenuState(nextStateSpace, camera, Content, Graphics, this, keyboardState: Keyboard.GetState());
                        break;
                    case (int)Options.SAVE_TITLE:
                        FileIO.SaveDungeonData(((PlayingState)previousState).GetSaveData());
                        nextState = new TitleState(camera, Content, Graphics, Mouse.GetState(), GamePad.GetState(PlayerIndex.One), keyState);
                        break;
                    case (int)Options.UNPAUSE:
                        nextState = previousState;
                        nextState.SetPrevInput(Keyboard.GetState(), Mouse.GetState(), GamePad.GetState(PlayerIndex.One));
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
            spriteBatch.DrawString(titleText, Title, new Vector2((camera.FullViewport.Width / 2) - titleLength.X / 2, messageSpacing), Color.Goldenrod);
            foreach (Option option in menuOptions)
            {
                int stringLength = (int)optionText.MeasureString(option.Message).X;
                spriteBatch.DrawString(optionText, option.Message, new Vector2((camera.FullViewport.Width / 2) - stringLength / 2, (camera.FullViewport.Height / 2) + (messageCount * messageSpacing)),
                    option.Enabled ? messageCount == optionSelection ? Color.MediumPurple : Color.Goldenrod : Color.Gray);
                messageCount += 1;
            }
        }

        public void SetStateSpace(IStateSpace stateSpace, Camera camera, bool createEntities = true)
        {
            //Nothing here.
        }

        public void SetPrevInput(KeyboardState prevKey, MouseState prevMouse, GamePadState prevPad)
        {
            PrevGamepadState = prevPad;
            PrevKeyboardState = prevKey;
            PrevMouseState = prevMouse;
        }
    }
}
