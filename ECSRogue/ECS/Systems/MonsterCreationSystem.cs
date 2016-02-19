using ECSRogue.BaseEngine;
using ECSRogue.ProceduralGeneration;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class MonsterCreationSystem
    {
        public static void CreateDungeonMonsters(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, int cellsize, List<Vector2> freeTiles)
        {
            int numberOfSpawns = 15; //Should be a formula based on depth level once a formula has been decided
            List<MonsterInfo> monsterPossibilities = new List<MonsterInfo>();
            //populate the monster possibility array based on how many slots a monster gets
            foreach (MonsterInfo monster in Monsters.MonsterCatalog.Where(x => x.SpawnDepthsAndChances.ContainsKey(spaceComponents.GameplayInfoComponent.FloorsReached)))
            {
                for (int i = 0; i < monster.SpawnDepthsAndChances[spaceComponents.GameplayInfoComponent.FloorsReached]; i++)
                {
                    monsterPossibilities.Add(monster);
                }
            }
            //Roll randomly in the index and add whatever monster it lands on
            if(monsterPossibilities.Count > 0)
            {
                for (int i = 0; i < numberOfSpawns; i++)
                {
                    monsterPossibilities[spaceComponents.random.Next(0, monsterPossibilities.Count)].SpawnFunction(spaceComponents, dungeonGrid, dungeonDimensions, cellsize, freeTiles);
                }
            }

            foreach (MonsterInfo monster in Monsters.MonsterCatalog.Where(x => x.isRequiredSpawn))
            {
                monster.SpawnFunction(spaceComponents, dungeonGrid, dungeonDimensions, cellsize, freeTiles);
            }
        }
    }
}
