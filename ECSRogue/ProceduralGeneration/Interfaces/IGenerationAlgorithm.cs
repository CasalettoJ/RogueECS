using ECSRogue.BaseEngine;
using ECSRogue.ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ProceduralGeneration.Interfaces
{
    public interface IGenerationAlgorithm
    {
        Vector2 GenerateDungeon(ref DungeonTile[,] dungeonGrid, int worldMin, int worldMax, Random random);
        void GenerateDungeonEntities(StateSpaceComponents spaceComponents);
        int GetCellsize();
        string GetDungeonSpritesheetFileName();
        DungeonColorInfo GetColorInfo();
    }
}
