using ECSRogue.BaseEngine.IO.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.BaseEngine.Interfaces
{
    public interface IState
    {
        IState UpdateContent(GameTime gameTime, Camera camera, ref GameSettings gameSettings);
        void DrawContent(SpriteBatch spriteBatch, Camera camera);
        void DrawUserInterface(SpriteBatch spriteBatch, Camera camera);
        void SetStateSpace(IStateSpace stateSpace, Camera camera);
    }
}
