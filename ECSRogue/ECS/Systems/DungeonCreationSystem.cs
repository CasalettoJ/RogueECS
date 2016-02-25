using ECSRogue.BaseEngine;
using ECSRogue.ECS.Components.ItemizationComponents;
using ECSRogue.ProceduralGeneration;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class DungeonCreationSystem
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

            foreach (MonsterInfo monster in Monsters.MonsterCatalog.Where(x => x.IsRequiredSpawn))
            {
                monster.SpawnFunction(spaceComponents, dungeonGrid, dungeonDimensions, cellsize, freeTiles);
            }
        }

        public static void CreateDungeonDrops(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, List<Vector2> freeTiles)
        {
            //These should be a formula based on level once a formula has been decided.
            int numberOfArtifacts = 5;
            int numberOfConsumablesAndGold = 25;
            List<ItemInfo> artifactPossibilities = new List<ItemInfo>();
            List<ItemInfo> consumableAndGoldPossibilities = new List<ItemInfo>();

            foreach (ItemInfo item in Items.ItemCatalog.Where(x => x.SpawnDepthsAndChances.ContainsKey(spaceComponents.GameplayInfoComponent.FloorsReached)))
            {
                for (int i = 0; i < item.SpawnDepthsAndChances[spaceComponents.GameplayInfoComponent.FloorsReached]; i++)
                {
                    switch(item.DropType)
                    {
                        case ItemType.ARTIFACT:
                            artifactPossibilities.Add(item);
                            break;
                        case ItemType.CONSUMABLE:
                        case ItemType.GOLD:
                            consumableAndGoldPossibilities.Add(item);
                            break;
                    }
                }
            }

            //Spawn Artifacts
            if(artifactPossibilities.Count > 0)
            {
                for(int i = 0; i <= numberOfArtifacts; i++)
                {
                    artifactPossibilities[spaceComponents.random.Next(0, artifactPossibilities.Count)].SpawnFunction(spaceComponents, dungeonGrid, dungeonDimensions, freeTiles);
                }
            }

            //Spawn gold and consumables
            if(consumableAndGoldPossibilities.Count > 0)
            {
                for(int i = 0; i <= numberOfConsumablesAndGold; i++)
                {
                    consumableAndGoldPossibilities[spaceComponents.random.Next(0, consumableAndGoldPossibilities.Count)].SpawnFunction(spaceComponents, dungeonGrid, dungeonDimensions, freeTiles);
                }
            }

            foreach (ItemInfo item in Items.ItemCatalog.Where(x => x.IsRequiredSpawn))
            {
                item.SpawnFunction(spaceComponents, dungeonGrid, dungeonDimensions, freeTiles);
            }

        }


    }
}
