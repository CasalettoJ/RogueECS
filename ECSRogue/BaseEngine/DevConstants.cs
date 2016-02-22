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
    }
}
