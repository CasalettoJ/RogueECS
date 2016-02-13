using ECSRogue.ECS;
using ECSRogue.ProceduralGeneration;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.BaseEngine.IO.Objects
{
    public class DungeonInfo
    {
        public Vector2 dungeonDimensions;
        public DungeonTile[,] dungeonGrid;
        public List<Vector2> freeTiles;
        public string dungeonSpriteFile;
        public DungeonColorInfo dungeonColorInfo;
        public StateComponents stateComponents;
        public StateSpaceComponents stateSpaceComponents;
    }
}
