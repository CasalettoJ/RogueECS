using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components
{
    public struct GameMessageComponent
    {
        public List<string> GameMessages;
        public string GlobalMessage;
        public SpriteFont Font;
        public Color Color;
    }
}
