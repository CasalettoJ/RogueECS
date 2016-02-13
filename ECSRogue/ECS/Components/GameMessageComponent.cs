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
        public List<Tuple<Color,string>> GameMessages;
        public string GlobalMessage;
        public string MenuMessage;
        public int IndexBegin;
        public int MaxMessages;
        public Color GlobalColor;
    }
}
