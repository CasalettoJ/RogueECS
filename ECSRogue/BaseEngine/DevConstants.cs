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
            public static readonly string SpriteSheet = "Sprites/Ball";
            public static readonly string MessageFont = "Fonts/InfoText";
            public static readonly string AsciiFont = "Fonts/DisplayText";
            public static readonly string UISheet = "Sprites/Ball";
            public static readonly string CavesSpriteSheet = "Sprites/Ball";
        }

        public static class Grid
        {
            public static readonly int CellSize = 32;
            public static readonly int TileSpriteSize = 40;
            public static readonly int TileBorderSize = (TileSpriteSize - CellSize) / 2;
            public static readonly int WallWeight = 10000;
        }
    }
}
