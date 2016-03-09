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
            public static readonly Color WallInRange = Color.Violet;
            public static readonly Color FloorInRange = Color.Green;
            public static readonly Color Water = new Color(11, 56, 89);
            public static readonly Color WaterInRange = new Color(28, 144, 227);
            public static readonly Color DeepWater = new Color(3, 26, 80);
            public static readonly Color DeepWaterInRange = new Color(7, 38, 109);
            public static readonly Color TallGrass = new Color(20, 73, 0);
            public static readonly Color TallGrassInRange = new Color(43, 109, 18);
            public static readonly Color Spikes = new Color(103, 112, 85);
            public static readonly Color SpikesInRange = new Color(170, 187, 138);
            //Alternate Wall Color:
            //new Color(127, 157, 175);
            //Alternate Wall Inrange Color:
            //new Color(177, 194, 206); 
        }

        public static class Monsters
        {
            public static readonly Color Red = new Color(220,56,110);
        }

        public static class Messages
        {
            public static readonly Color Normal = Color.White;
            public static readonly Color LootPickup = new Color(156, 1, 114);
            public static readonly Color Good = new Color(9, 125, 9);
            public static readonly Color Bad = new Color(191, 32, 32);
            public static readonly Color StatusChange = Color.OrangeRed;
            public static readonly Color Special = new Color(156, 132, 12);
        }
    }
}
