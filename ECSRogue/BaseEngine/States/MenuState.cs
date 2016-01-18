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
    public class MenuState : IState
    {
        #region State Management Property
        private static IState previousState;
        #endregion

        #region State Private Variables
        private ContentManager Content;
        private MouseState PrevMouseState;
        private GamePadState PrevGamepadState;
        private KeyboardState PrevKeyboardState;
        private IStateSpace CurrentStateSpace;
        private GraphicsDeviceManager Graphics;
        private StateComponents StateComponents;
        #endregion

        public MenuState(IStateSpace space, Camera camera, ContentManager content, GraphicsDeviceManager graphics,
            IState prevState = null, MouseState mouseState = new MouseState(), GamePadState gamePadState = new GamePadState(), KeyboardState keyboardState = new KeyboardState(), DungeonInfo saveInfo = null)
        {
            this.Content = new ContentManager(content.ServiceProvider, "Content");
            Graphics = graphics;
            PrevMouseState = mouseState;
            PrevGamepadState = gamePadState;
            PrevKeyboardState = keyboardState;
            StateComponents = saveInfo == null ? new StateComponents() : saveInfo.stateComponents;
            SetStateSpace(space, camera, saveInfo == null);
            previousState = prevState;
        }

        public IState UpdateContent(GameTime gameTime, Camera camera, ref GameSettings gameSettings)
        {
            IStateSpace nextLevel = CurrentStateSpace;
            nextLevel = CurrentStateSpace.UpdateSpace(gameTime, Content, Graphics, PrevKeyboardState, PrevMouseState, PrevGamepadState, camera, ref gameSettings);
            if (nextLevel != CurrentStateSpace && nextLevel != null)
            {
                SetStateSpace(nextLevel, camera);
            }
            if (nextLevel == null || (Keyboard.GetState().IsKeyDown(Keys.Escape) && PrevKeyboardState.IsKeyUp(Keys.Escape)))
            {
                previousState.SetPrevInput(Keyboard.GetState(), Mouse.GetState(), GamePad.GetState(PlayerIndex.One));
                if(previousState.GetType().Name == "TitleState")
                {
                    return new TitleState(camera, Content, Graphics, keyboardState: Keyboard.GetState()); //Fixes bug about title not rendering when you go back for some reason..
                }
                return previousState;
            }
            PrevKeyboardState = Keyboard.GetState();
            PrevMouseState = Mouse.GetState();
            PrevGamepadState = GamePad.GetState(PlayerIndex.One);
            return this;
        }

        public void DrawContent(SpriteBatch spriteBatch, Camera camera)
        {
            CurrentStateSpace.DrawLevel(spriteBatch, Graphics, camera);
        }

        public void DrawUserInterface(SpriteBatch spriteBatch, Camera camera)
        {
            CurrentStateSpace.DrawUserInterface(spriteBatch, camera);
        }

        public void SetPrevInput(KeyboardState prevKey, MouseState prevMouse, GamePadState prevPad)
        {
            //Nothing here.
        }

        public void SetStateSpace(IStateSpace stateSpace, Camera camera, bool createEntities = true)
        {
            if (Content != null && stateSpace != null)
            {
                Content.Unload();

                CurrentStateSpace = stateSpace;
                stateSpace.LoadLevel(Content, Graphics, camera, StateComponents);
            }
        }
    }
}
