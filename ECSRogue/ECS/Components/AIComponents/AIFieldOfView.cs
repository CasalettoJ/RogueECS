using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components.AIComponents
{
    public static class FOVColors
    {
        public static readonly Color Sleeping = Color.CornflowerBlue;
        public static readonly Color Roaming = Color.OrangeRed;
        public static readonly Color Attacking = Color.DarkRed;
        public static readonly Color Fleeing = Color.LightGoldenrodYellow;
        public static readonly Color Friendly = Color.LightGreen;
    }

    public struct AIFieldOfView
    {
        public int radius;
        public bool DrawField;
        public List<Vector2> SeenTiles;
        public Color Color;
        public float Opacity;
    }
}
