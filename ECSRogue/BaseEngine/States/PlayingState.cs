﻿using ECSRogue.BaseEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using ECSRogue.ECS;
using ECSRogue.BaseEngine.IO.Objects;

namespace ECSRogue.BaseEngine.States
{
    public class PlayingState : IState
    {
        #region State Management Property
        private static IState previousState;
        #endregion

        #region State Private Variables
        private ContentManager Content;
        private MouseState PrevMouseState;
        private GamePadState PrevGamepadState;
        private KeyboardState PrevKeyboardState;
        private IStateSpace CurrentLevel;
        private GraphicsDeviceManager Graphics;
        private StateComponents StateComponents;
        #endregion

        public PlayingState(IStateSpace space, Camera camera, ContentManager content, GraphicsDeviceManager graphics, 
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

        ~PlayingState()
        {
            if (Content != null) { Content.Unload(); }
        }

        public IState UpdateContent(GameTime gameTime, Camera camera, ref GameSettings gameSettings)
        {
            IStateSpace nextLevel = CurrentLevel;
            nextLevel = CurrentLevel.UpdateSpace(gameTime, Content, Graphics, PrevKeyboardState, PrevMouseState, PrevGamepadState, camera, ref gameSettings);
            if (nextLevel != CurrentLevel && nextLevel != null)
            {
                SetStateSpace(nextLevel, camera);
            }
            if (nextLevel == null)
            {
                return previousState;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && PrevKeyboardState.IsKeyUp(Keys.Escape))
            {
                return new PauseState(camera, Content, Graphics, this, keyboardState: Keyboard.GetState());
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

        public void SetStateSpace(IStateSpace stateSpace, Camera camera, bool createEntities = true)
        {
            if (Content != null && stateSpace != null)
            {
                Content.Unload();

                CurrentLevel = stateSpace;
                stateSpace.LoadLevel(Content, Graphics, camera, StateComponents, createEntities);
            }
        }

        public void DrawUserInterface(SpriteBatch spriteBatch, Camera camera)
        {
            CurrentLevel.DrawUserInterface(spriteBatch, camera);
        }

        public void SetPrevInput(KeyboardState prevKey, MouseState prevMouse, GamePadState prevPad)
        {
            PrevGamepadState = prevPad;
            PrevKeyboardState = prevKey;
            PrevMouseState = prevMouse;
        }

        public DungeonInfo GetSaveData()
        {
            return CurrentLevel.GetSaveData();
        }
    }
}
