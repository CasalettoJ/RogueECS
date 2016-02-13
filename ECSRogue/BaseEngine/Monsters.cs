﻿using ECSRogue.ECS;
using ECSRogue.ECS.Components;
using ECSRogue.ECS.Components.AIComponents;
using ECSRogue.ECS.Components.MeleeMessageComponents;
using ECSRogue.ProceduralGeneration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.BaseEngine
{
    public struct MonsterInfo
    {
        public int[] ValidDepths;
        public int spawnIndexSlots; // Number of times the monster will be entered into the spawn array, which rolls to see what monster gets added to the floor
        public bool isRequiredSpawn; //Boss monsters, some special entities, might need to ALWAYS be added to a floor
        public Func<StateSpaceComponents, DungeonTile[,], Vector2, int, List<Vector2>, bool> SpawnFunction; // SpaceComponents, World Grid, Dungeon Dimensions, Free tiles list
    }

    public static class Monsters
    {
        public static readonly List<MonsterInfo> MonsterCatalog = new List<MonsterInfo>()
        {
            //Test Enemy NPC
            new MonsterInfo()
            {
                ValidDepths = new int[] { 1, 2, 3, 4, 6, 7 },
                isRequiredSpawn = false,
                spawnIndexSlots = 7,
                SpawnFunction = MonsterSpawners.SpawnTestEnemyNPC
            },

            //Wild Vines
            new MonsterInfo()
            {
                ValidDepths = new int[] { 1, 2, 3, 4, 6, 7, 8 },
                isRequiredSpawn = false,
                spawnIndexSlots = 3,
                SpawnFunction = MonsterSpawners.SpawnWildVines
            }
        };
    }

    public static class MonsterSpawners
    {
        public static bool SpawnTestEnemyNPC(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, int cellSize, List<Vector2> freeTiles)
        {
            Guid id = spaceComponents.CreateEntity();
            spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.Drawable | ComponentMasks.CombatReadyAI | ComponentMasks.AIView;

            int tileIndex = spaceComponents.random.Next(0, freeTiles.Count);
            spaceComponents.PositionComponents[id] = new PositionComponent() { Position = freeTiles[tileIndex] };
            freeTiles.RemoveAt(tileIndex);
            spaceComponents.DisplayComponents[id] = new DisplayComponent()
            {
                Color = Color.DarkRed,
                Origin = Vector2.Zero,
                Rotation = 0f,
                Scale = 1f,
                SpriteEffect = SpriteEffects.None,
                SpriteSource = new Rectangle(0 * cellSize, 0 * cellSize, cellSize, cellSize),
                Symbol = "t",
                SymbolColor = Color.White
            };
            spaceComponents.SkillLevelsComponents[id] = new SkillLevelsComponent() { CurrentHealth = 25, DieNumber = 1, Health = 25, Power = 5, Defense = 1, Accuracy = 100, Wealth = 25, MinimumDamage = 1, MaximumDamage = 2 };
            spaceComponents.CollisionComponents[id] = new CollisionComponent() { Solid = true, CollidedObjects = new List<Guid>() };
            spaceComponents.NameComponents[id] = new NameComponent() { Name = "TEST ENEMY NPC" };
            spaceComponents.AIAlignmentComponents[id] = new AIAlignment() { Alignment = AIAlignments.ALIGNMENT_HOSTILE };
            spaceComponents.AICombatComponents[id] = new AICombat() { AttackType = AIAttackTypes.ATTACK_TYPE_NORMAL, FleesWhenLowHealth = true };
            spaceComponents.AIStateComponents[id] = new AIState() { State = AIStates.STATE_SLEEPING };
            spaceComponents.AIFieldOfViewComponents[id] = new AIFieldOfView() { DrawField = true, radius = 2, SeenTiles = new List<Vector2>(), Color = FOVColors.Sleeping };
            spaceComponents.AISleepComponents[id] = new AISleep() { ChanceToWake = 15, FOVRadiusChangeOnWake = 2 };
            spaceComponents.AIRoamComponents[id] = new AIRoam() { ChanceToDetect = 25 };
            spaceComponents.AIFleeComponents[id] = new AIFlee() { DoesFlee = true, FleeAtHealthPercent = 25, FleeUntilHealthPercent = 30 };
            spaceComponents.MeleeAttackNPCMessageComponents[id] = new MeleeAttackNPCMessageComponent()
            {
                AttackNPCMessages = new string[]
                {
                        "{0} swings at the {1}",
                        "{0} applies fury to the {1}'s face",
                        "{0} attempts brute force against {1}",
                        "{0} uses a walking attack fearlessly at {1}"
                }
            };
            spaceComponents.MeleeAttackPlayerMessageComponents[id] = new MeleeAttackPlayerMessageComponent()
            {
                AttackPlayerMessages = new string[]
                {
                        "{0} tests a mighty attack on you",
                        "The {0} glitches out against you",
                        "Watch out! {0} tries to attack you"
                }
            };
            spaceComponents.DodgeMeleeMessageComponents[id] = new DodgeMeleeMessageComponent()
            {
                NormalDodgeMessages = new string[]
                {
                        " but the attack missed!",
                        " and the creature dodges the attack.",
                        " but the creature's defense protects it.",
                        " and the defense test is a success.",
                        " but the combat system test makes the attack miss."
                },
                StreakDodgeMessages = new string[]
                {
                        " and, as always, the attack misses.",
                        " and it misses again!",
                        " and shows how advanced its AI is by dodging again.",
                        " and taunts at the attack. \"Give up!\""
                }
            };
            spaceComponents.TakeMeleeDamageMesageComponents[id] = new TakeMeleeDamageMesageComponent()
            {
                NormalTakeDamageMessages = new string[]
                {
                        " and it takes {0} damage.",
                        " and it cries out, {0} health weaker.",
                        " and it glitches out, health dropping by {0}.",
                        " for {0} damage."
                },
                BrokenDodgeStreakTakeDamageMessages = new string[]
                {
                        " and against all odds deals {0} damage!",
                        " and the cocky creature allows {0} damage to go through!",
                        ", breaking impossible odds, landing {0} damage!!",
                        " and the test is broken! It takes {0} damage!"
                }
            };
            return true;
        }
        public static bool SpawnWildVines(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, int cellSize, List<Vector2> freeTiles)
        {
            Guid id = spaceComponents.CreateEntity();
            spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.Drawable | ComponentMasks.CombatReadyAI | ComponentMasks.AIView;

            int tileIndex = spaceComponents.random.Next(0, freeTiles.Count);
            spaceComponents.PositionComponents[id] = new PositionComponent() { Position = freeTiles[tileIndex] };
            freeTiles.RemoveAt(tileIndex);
            spaceComponents.DisplayComponents[id] = new DisplayComponent()
            {
                Color = Color.DarkRed,
                Origin = Vector2.Zero,
                Rotation = 0f,
                Scale = 1f,
                SpriteEffect = SpriteEffects.None,
                SpriteSource = new Rectangle(0 * cellSize, 0 * cellSize, cellSize, cellSize),
                Symbol = "W",
                SymbolColor = Color.White
            };
            spaceComponents.SkillLevelsComponents[id] = new SkillLevelsComponent() { CurrentHealth = 45, DieNumber = 2, Health = 45, Power = 5, Defense = 10, Accuracy = 135, Wealth = 25, MinimumDamage = 5, MaximumDamage = 14 };
            spaceComponents.CollisionComponents[id] = new CollisionComponent() { Solid = true, CollidedObjects = new List<Guid>() };
            spaceComponents.NameComponents[id] = new NameComponent() { Name = "WILD ROOTS" };
            spaceComponents.AIAlignmentComponents[id] = new AIAlignment() { Alignment = AIAlignments.ALIGNMENT_HOSTILE };
            spaceComponents.AICombatComponents[id] = new AICombat() { AttackType = AIAttackTypes.ATTACK_TYPE_NORMAL, FleesWhenLowHealth = true };
            spaceComponents.AIStateComponents[id] = new AIState() { State = AIStates.STATE_ROAMING };
            spaceComponents.AIFieldOfViewComponents[id] = new AIFieldOfView() { DrawField = false, radius = 5, SeenTiles = new List<Vector2>(), Color = FOVColors.Roaming };
            spaceComponents.AISleepComponents[id] = new AISleep() { ChanceToWake = 10, FOVRadiusChangeOnWake = 2 };
            spaceComponents.AIRoamComponents[id] = new AIRoam() { ChanceToDetect = 40 };
            spaceComponents.AIFleeComponents[id] = new AIFlee() { DoesFlee = false, FleeAtHealthPercent = 25, FleeUntilHealthPercent = 30 };
            spaceComponents.MeleeAttackNPCMessageComponents[id] = new MeleeAttackNPCMessageComponent()
            {
                AttackNPCMessages = new string[]
                {
                        "{0} swings its tentacles toward {1}",
                        "{0} applies fury to the {1}'s face",
                        "{0} attempts brute force against {1}",
                        "{0} uses a walking attack fearlessly at {1}"
                }
            };
            spaceComponents.MeleeAttackPlayerMessageComponents[id] = new MeleeAttackPlayerMessageComponent()
            {
                AttackPlayerMessages = new string[]
                {
                        "{0} wiggles its tentacles at you",
                        "The {0} swipes at you multiple times",
                        "Gross.. the tentacles of {0} smear around you"
                }
            };
            spaceComponents.DodgeMeleeMessageComponents[id] = new DodgeMeleeMessageComponent()
            {
                NormalDodgeMessages = new string[]
                {
                        " but the attack missed!",
                        " and the creature dodges the attack.",
                        " but the creature's defense protects it.",
                        " but it uses its tentacle vines to protect itself.",
                        " but the vines are too thick for the attack!"
                },
                StreakDodgeMessages = new string[]
                {
                        " and, as always, the attack misses.",
                        " and it misses again!",
                        " and it slithers around, cackling",
                        " and taunts at the attack. \"Give up!\""
                }
            };
            spaceComponents.TakeMeleeDamageMesageComponents[id] = new TakeMeleeDamageMesageComponent()
            {
                NormalTakeDamageMessages = new string[]
                {
                        " and it takes {0} damage.",
                        " and it cries out, {0} health weaker.",
                        " and takes {0}, some of its vines dropping dead",
                        " for {0} damage."
                },
                BrokenDodgeStreakTakeDamageMessages = new string[]
                {
                        " and against all odds deals {0} damage!",
                        " and the cocky creature allows {0} damage to go through!",
                        ", breaking impossible odds, landing {0} damage!!",
                        " and the test is broken! It takes {0} damage!"
                }
            };
            return true;
        }
    }
}