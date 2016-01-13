using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.BaseEngine.Interfaces
{
    public interface IStateSpace
    {
        void LoadLevel(ContentManager content, GraphicsDeviceManager graphics, Camera camera);
        IStateSpace UpdateLevel(GameTime gameTime, ContentManager content, GraphicsDeviceManager graphics, KeyboardState prevKeyboardState, MouseState prevMouseState, GamePadState prevGamepadState, Camera camera);
        void DrawLevel(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, Camera camera);
        void DrawUserInterface(SpriteBatch spriteBatch, Camera camera);
    }
}
