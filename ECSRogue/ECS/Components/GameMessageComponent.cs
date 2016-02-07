using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components
{
    public static class MessageColors
    {
        public static readonly Color SpecialAction = Color.Goldenrod;
        public static readonly Color Harm = Color.Red;
        public static readonly Color ItemPickup = Color.MediumPurple;
        public static readonly Color DamageDealt = Color.LightGreen;
        public static readonly Color StatusChange = Color.SaddleBrown;
        public static readonly Color Failure = Color.PaleVioletRed;
        public static readonly Color Normal = Color.White;
        public static readonly Color Dodge = Color.DarkGreen;
        public static readonly Color Danger = Color.OrangeRed;
    }

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
