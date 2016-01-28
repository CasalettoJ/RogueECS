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
        TILE_WALL,
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

    public struct DijkstraMapTile
    {
        public int Weight;
        public bool Checked;
    }
}
