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
        IState UpdateContent(GameTime gameTime, Camera camera);
        void DrawContent(SpriteBatch spriteBatch, Camera camera);
        void SetStateSpace(IStateSpace level, Camera camera);
    }
}
