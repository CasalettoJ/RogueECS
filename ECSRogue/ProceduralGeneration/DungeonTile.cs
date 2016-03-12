using Microsoft.Xna.Framework;
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
        TILE_FLOOR,
        TILE_WATER,
        TILE_TALLGRASS,
        TILE_SPIKE,
        TILE_FLATTENEDGRASS,
        TILE_FIRE,
        TILE_ASH
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
        public string Symbol;
        public int ChanceToIgnite;
        public int TurnsToBurn;
        public Color SymbolColor;
    }

    public struct DijkstraMapTile
    {
        public int Weight;
        public bool Checked;
    }

    public static class Tiles
    {
        public static readonly string TallGrassSymbol = "\"";
        public static readonly Color TallGrassSymbolColor = Color.Wheat;
        public static readonly int TallGrassIgniteChange = 50;

        public static readonly string WaterSymbol = "~";
        public static readonly Color WaterSymbolColor = Color.CornflowerBlue;
        public static readonly int WaterIgniteChance = 0;

        public static readonly string FireSymbol = "^";
        public static readonly Color FireSymbolColor = Color.Firebrick;
        public static readonly int FireIgniteChance = 0;

        public static readonly string FlatGrassSymbol = "\'";
        public static readonly Color FlatGrassSymbolColor = Color.Wheat;
        public static readonly int FlatGrassIgniteChance = 20;

        public static readonly int FloorIgniteChance = 1;
    }
}
