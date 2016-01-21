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
using ECSRogue.BaseEngine.IO;

namespace ECSRogue.BaseEngine.StateSpaces
{
    public class GameSettingsMenuStateSpace : IStateSpace
    {
        private enum Options
        {
            RESOLUTION = 0,
            GRAPHICS_SCALE = 1,
            BORDERLESS = 2,
            VSYNC = 3,
            MESSAGE_FILTER = 4,
            SAVE_CHANGES = 5,
            RESTORE_DEFAULTS = 6,
            CANCEL = 7
        }
        private struct Option
        {
            public string Message;
            public List<object> OptionsCollection;
            public int Selection;
        }

        private const int optionsAmount = 8;
        private int optionSelection;
        private SpriteFont titleText;
        private SpriteFont optionText;
        private SpriteFont smallText;
        private Option[] menuOptions;
        private string Title;
        private GameSettings gameSettings;

        public GameSettingsMenuStateSpace(ref GameSettings gameSettings)
        {
            this.gameSettings = gameSettings;
        }
       
        public void LoadLevel(ContentManager content, GraphicsDeviceManager graphics, Camera camera, StateComponents stateComponents, bool createEntities = true)
        {
            Title = "GAME SETTINGS";
            titleText = content.Load<SpriteFont>("Fonts/TitleText");
            optionText = content.Load<SpriteFont>("Fonts/OptionText");
            smallText = content.Load<SpriteFont>("Fonts/InfoText");
            optionSelection = 0;
            menuOptions = new Option[optionsAmount];

            //Resolution Option
            menuOptions[(int)Options.RESOLUTION] = new Option() { Message = "RESOLUTION: ", OptionsCollection = new List<object>(), Selection = 0 };
            //Set to current resolution after populating resolutions
            foreach (DisplayMode display in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                Vector2 resolution = new Vector2(display.Width, display.Height);
                if(!menuOptions[(int)Options.RESOLUTION].OptionsCollection.Contains(resolution) && resolution.X > 900)
                {
                    menuOptions[(int)Options.RESOLUTION].OptionsCollection.Add(resolution);
                }
            }
            menuOptions[(int)Options.RESOLUTION].Selection = 
                menuOptions[(int)Options.RESOLUTION].OptionsCollection.FindIndex(x => (Vector2)x == gameSettings.Resolution) > 0 ? 
                menuOptions[(int)Options.RESOLUTION].OptionsCollection.FindIndex(x => (Vector2)x == gameSettings.Resolution) : 0;

            //Scale Options
            menuOptions[(int)Options.GRAPHICS_SCALE] = new Option() { Message = "GRAPHICS SCALE: ", OptionsCollection = new List<object>(), Selection = 0 };
            //Set to current scale after populating scale resolutions
            menuOptions[(int)Options.GRAPHICS_SCALE].OptionsCollection.Add(.25f);
            menuOptions[(int)Options.GRAPHICS_SCALE].OptionsCollection.Add(.5f);
            menuOptions[(int)Options.GRAPHICS_SCALE].OptionsCollection.Add(.75f);
            menuOptions[(int)Options.GRAPHICS_SCALE].OptionsCollection.Add(1f);
            menuOptions[(int)Options.GRAPHICS_SCALE].OptionsCollection.Add(1.25f);
            menuOptions[(int)Options.GRAPHICS_SCALE].OptionsCollection.Add(1.5f);
            menuOptions[(int)Options.GRAPHICS_SCALE].OptionsCollection.Add(2f);
            menuOptions[(int)Options.GRAPHICS_SCALE].OptionsCollection.Add(2.25f);
            menuOptions[(int)Options.GRAPHICS_SCALE].OptionsCollection.Add(2.5f);
            menuOptions[(int)Options.GRAPHICS_SCALE].OptionsCollection.Add(4f);
            menuOptions[(int)Options.GRAPHICS_SCALE].Selection =
                menuOptions[(int)Options.GRAPHICS_SCALE].OptionsCollection.FindIndex(x => Convert.ToDouble(x) == (double)gameSettings.Scale) > 0 ?
                menuOptions[(int)Options.GRAPHICS_SCALE].OptionsCollection.FindIndex(x => Convert.ToDouble(x) == (double)gameSettings.Scale) : 0;

            //Borderless Option
            menuOptions[(int)Options.BORDERLESS] = new Option() { Message = "WINDOW BORDERLESS: ", OptionsCollection = new List<object>(), Selection = 0 };
            //Set to current borderless option
            menuOptions[(int)Options.BORDERLESS].OptionsCollection.Add(true);
            menuOptions[(int)Options.BORDERLESS].OptionsCollection.Add(false);
            menuOptions[(int)Options.BORDERLESS].Selection = menuOptions[(int)Options.BORDERLESS].OptionsCollection.FindIndex(x => (bool)x == gameSettings.Borderless);

            //Vsync Option
            menuOptions[(int)Options.VSYNC] = new Option() { Message = "VSYNC: ", OptionsCollection = new List<object>(), Selection = 0 };
            //Set to current vsync option
            menuOptions[(int)Options.VSYNC].OptionsCollection.Add(true);
            menuOptions[(int)Options.VSYNC].OptionsCollection.Add(false);
            menuOptions[(int)Options.VSYNC].Selection = menuOptions[(int)Options.VSYNC].OptionsCollection.FindIndex(x => (bool)x == gameSettings.Vsync);

            //Message Filter Option
            menuOptions[(int)Options.MESSAGE_FILTER] = new Option() { Message = "SHOW UNIMPORTANT MESSAGES: ", OptionsCollection = new List<object>(), Selection = 0 };
            //Set to current message filter option
            menuOptions[(int)Options.MESSAGE_FILTER].OptionsCollection.Add(true);
            menuOptions[(int)Options.MESSAGE_FILTER].OptionsCollection.Add(false);
            menuOptions[(int)Options.MESSAGE_FILTER].Selection = menuOptions[(int)Options.MESSAGE_FILTER].OptionsCollection.FindIndex(x => (bool)x == gameSettings.ShowNormalMessages);

            //Save Changes
            menuOptions[(int)Options.SAVE_CHANGES] = new Option() { Message = "[SAVE CHANGES]", OptionsCollection = null, Selection = 0 };

            //Restore default
            menuOptions[(int)Options.RESTORE_DEFAULTS] = new Option() { Message = "[RESTORE DEFAULTS]", OptionsCollection = null, Selection = 0 };

            //Cancel
            menuOptions[(int)Options.CANCEL] = new Option() { Message = "[CANCEL]", OptionsCollection = null, Selection = 0 };

        }

        public IStateSpace UpdateSpace(GameTime gameTime, ContentManager content, GraphicsDeviceManager graphics, KeyboardState prevKeyboardState, MouseState prevMouseState, GamePadState prevGamepadState, Camera camera, ref GameSettings gameSettings)
        {
            IStateSpace nextSpace = this;
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Escape) && prevKeyboardState.IsKeyUp(Keys.Escape))
            {
                nextSpace = null;
            }
            else if (keyState.IsKeyDown(Keys.Up) && prevKeyboardState.IsKeyUp(Keys.Up))
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
            }
            else if (keyState.IsKeyDown(Keys.Down) && prevKeyboardState.IsKeyUp(Keys.Down))
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
            }
            else if (keyState.IsKeyDown(Keys.Left) && prevKeyboardState.IsKeyUp(Keys.Left) && menuOptions[optionSelection].OptionsCollection != null)
            {
                menuOptions[optionSelection].Selection -= 1;
                if (menuOptions[optionSelection].Selection < 0)
                {
                    menuOptions[optionSelection].Selection = menuOptions[optionSelection].OptionsCollection.Count - 1;
                }
                if (menuOptions[optionSelection].Selection >= menuOptions[optionSelection].OptionsCollection.Count)
                {
                    menuOptions[optionSelection].Selection = 0;
                }
            }
            else if (keyState.IsKeyDown(Keys.Right) && prevKeyboardState.IsKeyUp(Keys.Right) && menuOptions[optionSelection].OptionsCollection != null)
            {
                menuOptions[optionSelection].Selection += 1;
                if (menuOptions[optionSelection].Selection < 0)
                {
                    menuOptions[optionSelection].Selection = menuOptions[optionSelection].OptionsCollection.Count - 1;
                }
                if (menuOptions[optionSelection].Selection >= menuOptions[optionSelection].OptionsCollection.Count)
                {
                    menuOptions[optionSelection].Selection = 0;
                }
            }

            else if (keyState.IsKeyDown(Keys.Enter) && prevKeyboardState.IsKeyUp(Keys.Enter))
            {
                switch (optionSelection)
                {
                    case (int)Options.SAVE_CHANGES:
                        gameSettings.Resolution = (Vector2)menuOptions[(int)Options.RESOLUTION].OptionsCollection[menuOptions[(int)Options.RESOLUTION].Selection];
                        gameSettings.Scale = (float)menuOptions[(int)Options.GRAPHICS_SCALE].OptionsCollection[menuOptions[(int)Options.GRAPHICS_SCALE].Selection];
                        gameSettings.Borderless = (bool)menuOptions[(int)Options.BORDERLESS].OptionsCollection[menuOptions[(int)Options.BORDERLESS].Selection];
                        gameSettings.ShowNormalMessages = (bool)menuOptions[(int)Options.MESSAGE_FILTER].OptionsCollection[menuOptions[(int)Options.MESSAGE_FILTER].Selection];
                        gameSettings.Vsync = (bool)menuOptions[(int)Options.VSYNC].OptionsCollection[menuOptions[(int)Options.VSYNC].Selection];
                        FileIO.SaveGameSettings(ref gameSettings);
                        nextSpace = null;
                        break;
                    case (int)Options.RESTORE_DEFAULTS:
                        FileIO.ResetGameSettings();
                        FileIO.LoadGameSettings(ref gameSettings);
                        nextSpace = new GameSettingsMenuStateSpace(ref gameSettings);
                        break;
                    case (int)Options.CANCEL:
                        nextSpace = null;
                        break;
                }
            }

            return nextSpace;
        }

        public void DrawLevel(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, Camera camera, GameTime gameTime)
        {
            //Nothing here
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
                if(option.OptionsCollection != null)
                {
                    spriteBatch.DrawString(optionText, option.Message + option.OptionsCollection[option.Selection].ToString(), new Vector2((camera.FullViewport.Width / 2) - titleLength.X / 2, (camera.FullViewport.Height / 3) + (messageCount * messageSpacing)),
                        messageCount == optionSelection ? Color.MediumPurple : Color.Goldenrod);
                }
                else
                {
                    spriteBatch.DrawString(optionText, option.Message, new Vector2((camera.FullViewport.Width / 2) - titleLength.X / 2, (camera.FullViewport.Height / 3) + (messageCount * messageSpacing)),
                        messageCount == optionSelection ? Color.MediumPurple : Color.Goldenrod);
                }
                messageCount += 1;
            }
        }

        public DungeonInfo GetSaveData()
        {
            return null;
        }
    }
}
