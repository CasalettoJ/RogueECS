using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.BaseEngine
{
    public static class Colors
    {
        public static class Caves
        {
            public static readonly Color Wall = Color.DarkViolet;
            public static readonly Color Floor = Color.DarkGreen;
            public static readonly Color FloorInRange = Color.Green;
            public static readonly Color WallInRange = Color.Violet;
        }

        public static class Messages
        {
            public static readonly Color Normal = Color.White;
            public static readonly Color LootPickup = new Color(80, 58, 150);
            public static readonly Color Good = new Color(9, 125, 9);
            public static readonly Color Bad = new Color(191, 32, 32);
            public static readonly Color StatusChange = Color.OrangeRed;
            public static readonly Color Special = new Color(156, 132, 12);
        }
    }
}
