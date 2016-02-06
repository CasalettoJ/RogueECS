using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components.AIComponents
{
    public struct AIFieldOfView
    {
        public int radius;
        public bool DrawField;
        public List<Vector2> SeenTiles;
        public Color Color;
    }
}
