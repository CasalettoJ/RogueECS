using ECSRogue.ECS.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.BaseEngine
{
    public static class DevConstants
    {
        public static class Graphics
        {
            public static readonly string SpriteSheet = "Sprites/Ball2";
            public static readonly string MessageFont = "Fonts/InfoText";
            public static readonly string OptionFont = "Fonts/OptionText";
            public static readonly string TitleText = "Fonts/TitleText";
            public static readonly string AsciiFont = "Fonts/DisplayText";
            public static readonly string UISheet = "Sprites/Ball2";
            public static readonly string CavesSpriteSheet = "Sprites/Ball2";
        }

        public static class Grid
        {
            public static readonly int CellSize = 32;
            public static readonly int TileSpriteSize = 36;
            public static readonly int TileBorderSize = (TileSpriteSize - CellSize) / 2;
            public static readonly int WallWeight = 10000;
        }

        public static class ConstantComponents
        {
            public static readonly DisplayComponent UnknownDisplay = new DisplayComponent()
            {
                AlwaysDraw = false,
                Color = Color.Black,
                Opacity = 1f,
                Origin = Vector2.Zero,
                Rotation = 0f,
                Scale = 1f,
                SpriteEffect = Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
                SpriteSource = new Rectangle(0 * DevConstants.Grid.CellSize, 0 * DevConstants.Grid.CellSize, DevConstants.Grid.CellSize, DevConstants.Grid.CellSize),
                Symbol = "?",
                SymbolColor = Color.White
            };
        }

        public static class StatusEffects
        {
            public static class Burning
            {
                public static readonly int Turns = 7;
                public static readonly int MinDamage = 1;
                public static readonly int MaxDamage = 8;
            }
        }
    }
}
