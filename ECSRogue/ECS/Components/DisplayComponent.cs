using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components
{
    public struct DisplayComponent
    {
        public Texture2D Texture; //Maybe be replaced with a rectangle of where on the sprite sheet the texture should come from.
        public Color Color;
    }
}
