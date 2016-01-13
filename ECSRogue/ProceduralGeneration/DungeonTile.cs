using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ProceduralGeneration
{
    public enum TileType
    {
        NONE,
        TILE_ROCK,
        TILE_TOPLEFT_WALL,
        TILE_TOP_WALL,
        TILE_TOPRIGHT_WALL,
        TILE_LEFT_WALL,
        TILE_WALL,
        TILE_RIGHT_WALL,
        TILE_BOTTOMLEFT_WALL,
        TILE_BOTTOM_WALL,
        TILE_BOTTOMRIGHT_WALL,
        TILE_FLOOR
    }

    public struct DungeonTile
    {
        public TileType Type;
        public bool Reached;
        public bool Found;
        public bool NewlyFound;
        public bool InRange;
        public bool Occupiable;
        public float Opacity;
    }
}
