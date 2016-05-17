using ECSRogue.ECS;
using ECSRogue.ECS.Components;
using ECSRogue.ECS.Components.GraphicalEffectsComponents;
using ECSRogue.ECS.Components.ItemizationComponents;
using ECSRogue.ProceduralGeneration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.BaseEngine
{
    public enum ItemNames
    {
        NONE,
        DOWNSTAIRS,
        GOLD,
        TESTCONSUMABLE,
        TESTARTIFACT
    }

    public enum ItemUseFunctions
    {
        NONE,
        TESTUSE
    }


    public struct ItemInfo
    {
        public ItemNames Name;
        public ItemType DropType;
        public Dictionary<int, int> SpawnDepthsAndChances; //Key: Depths it spawns in, Value: Number of spawn slots in the wheel
        public bool IsRequiredSpawn; //Sometimes it just has to drop.
        public int RequiredSpawnAmount; //Sometimes it just has to drop a few times.
        public Func<StateSpaceComponents, DungeonTile[,], Vector2, List<Vector2>, bool> SpawnFunction; // SpaceComponents, World Grid, Dungeon Dimensions, Free tiles list
    }

    public static class Items
    {
        public static readonly List<ItemInfo> ItemCatalog = new List<ItemInfo>()
        {
            //DownStairs
            new ItemInfo()
            {
                Name = ItemNames.DOWNSTAIRS,
                DropType = ItemType.DOWNSTAIRS,
                IsRequiredSpawn = true,
                RequiredSpawnAmount = 1,
                SpawnDepthsAndChances = new Dictionary<int, int>(),
                SpawnFunction = ItemSpawners.SpawnDownStairway
            },

            //Gold
            new ItemInfo()
            {
                Name = ItemNames.GOLD,
                DropType = ItemType.GOLD,
                IsRequiredSpawn = true,
                RequiredSpawnAmount = 3,
                SpawnDepthsAndChances = new Dictionary<int, int>()
                {
                    {1, 3}, {2, 3 }, {3, 3 }, {4, 5 }, {5, 10 }, {6, 5 }, {7, 5 }, {8, 5 }, {9, 7 }, {10, 10 }, {11, 8 }, {12, 6 }, {13, 4 }, {14, 5 }, {15, 10 }
                },
                SpawnFunction = ItemSpawners.SpawnGold
            },

            //Test Artifact
            new ItemInfo()
            {
                Name = ItemNames.TESTARTIFACT,
                DropType = ItemType.ARTIFACT,
                IsRequiredSpawn = true,
                RequiredSpawnAmount = 1,
                SpawnDepthsAndChances = new Dictionary<int, int>()
                {
                    {1, 5 }, {2, 10}, {3, 10 }, {4, 5 }, {5, 10 }, {6, 5 }
                },
                SpawnFunction = ItemSpawners.SpawnTestArtifact
            },

            //Test Consumable
            new ItemInfo()
            {
                Name = ItemNames.TESTCONSUMABLE,
                DropType = ItemType.CONSUMABLE,
                IsRequiredSpawn = false,
                RequiredSpawnAmount = 0,
                SpawnDepthsAndChances = new Dictionary<int, int>()
                {
                    {1, 10 }, {2, 5 }
                },
                SpawnFunction = ItemSpawners.SpawnTestConsumable
            }
        };
    }


    public static class ItemSpawners
    {
        public static bool SpawnGold(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, List<Vector2> freeTiles)
        {
            Guid id = spaceComponents.CreateEntity();
            spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.Drawable | ComponentMasks.GlowingOutline | ComponentMasks.PickupItem;
            spaceComponents.DisplayComponents[id] = new DisplayComponent()
            {
                AlwaysDraw = false,
                Color = Colors.Messages.Special,
                Opacity = 1f,
                Origin = Vector2.Zero,
                Rotation = 0f,
                Scale = 1f,
                SpriteEffect = SpriteEffects.None,
                SpriteSource = new Rectangle(0 * DevConstants.Grid.CellSize, 0 * DevConstants.Grid.CellSize, DevConstants.Grid.CellSize, DevConstants.Grid.CellSize),
                Symbol = "$",
                SymbolColor = Color.White
            };
            Vector2 position = freeTiles[spaceComponents.random.Next(0, freeTiles.Count)];
            freeTiles.Remove(position);
            spaceComponents.PositionComponents[id] = new PositionComponent() { Position = position };
            spaceComponents.OutlineComponents[id] = new OutlineComponent() { Color = Color.Purple, Opacity = 1f };
            spaceComponents.SecondaryOutlineComponents[id] = new SecondaryOutlineComponent() { AlternateColor = Color.LightBlue, Seconds = 0f, SwitchAtSeconds = .75f };
            spaceComponents.PickupComponents[id] = new PickupComponent() { PickupType = ItemType.GOLD };
            spaceComponents.ValueComponents[id] = new ValueComponent() { Gold = spaceComponents.random.Next(0, 231) };
            spaceComponents.NameComponents[id] = new NameComponent()
            {
                Name = "Gold",
                Description = "Some people try and use fancy names for this mass of wealth. Credits, Stardust, Gil... it buys shelter and women all the same."
            };
            spaceComponents.CollisionComponents[id] = new CollisionComponent() { CollidedObjects = new List<Guid>(), Solid = false };
            return true;
        }
        public static bool SpawnDownStairway(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, List<Vector2> freeTiles)
        {
            Guid id = spaceComponents.CreateEntity();
            spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.Drawable | ComponentMasks.GlowingOutline | ComponentMasks.PickupItem;
            spaceComponents.DisplayComponents[id] = new DisplayComponent()
            {
                AlwaysDraw = false,
                Color = Color.Black,
                Opacity = 1f,
                Origin = Vector2.Zero,
                Rotation = 0f,
                Scale = 1f,
                SpriteEffect = SpriteEffects.None,
                SpriteSource = new Rectangle(0 * DevConstants.Grid.CellSize, 0 * DevConstants.Grid.CellSize, DevConstants.Grid.CellSize, DevConstants.Grid.CellSize),
                Symbol = "<",
                SymbolColor = Color.White
            };
            Vector2 position = freeTiles[spaceComponents.random.Next(0, freeTiles.Count)];
            freeTiles.Remove(position);
            spaceComponents.PositionComponents[id] = new PositionComponent() { Position = position };
            spaceComponents.OutlineComponents[id] = new OutlineComponent() { Color = Color.Goldenrod, Opacity = 1f };
            spaceComponents.SecondaryOutlineComponents[id] = new SecondaryOutlineComponent() { AlternateColor = Color.White, Seconds = 0f, SwitchAtSeconds = 2f };
            spaceComponents.PickupComponents[id] = new PickupComponent() { PickupType = ItemType.DOWNSTAIRS };
            spaceComponents.ValueComponents[id] = new ValueComponent() { Gold = spaceComponents.random.Next(0, 231) };
            spaceComponents.NameComponents[id] = new NameComponent()
            {
                Name = "A Path Downward",
                Description = "A small passageway leading deeper into this cave system.  There's no telling what waits at the other end, but you have a feeling there's no going back once you descend."
            };
            spaceComponents.CollisionComponents[id] = new CollisionComponent() { CollidedObjects = new List<Guid>(), Solid = true };
            return true;
        }
        public static bool SpawnTestConsumable(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, List<Vector2> freeTiles)
        {
            Guid id = spaceComponents.CreateEntity();
            spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.Drawable | ComponentMasks.GlowingOutline | ComponentMasks.PickupItem | ComponentMasks.Consumable;
            spaceComponents.DisplayComponents[id] = new DisplayComponent()
            {
                AlwaysDraw = false,
                Color = Colors.Messages.LootPickup,
                Opacity = 1f,
                Origin = Vector2.Zero,
                Rotation = 0f,
                Scale = 1f,
                SpriteEffect = SpriteEffects.None,
                SpriteSource = new Rectangle(0 * DevConstants.Grid.CellSize, 0 * DevConstants.Grid.CellSize, DevConstants.Grid.CellSize, DevConstants.Grid.CellSize),
                Symbol = "!",
                SymbolColor = Color.White
            };
            Vector2 position = freeTiles[spaceComponents.random.Next(0, freeTiles.Count)];
            freeTiles.Remove(position);
            spaceComponents.PositionComponents[id] = new PositionComponent() { Position = position };
            spaceComponents.OutlineComponents[id] = new OutlineComponent() { Color = Color.Purple, Opacity = 1f };
            spaceComponents.SecondaryOutlineComponents[id] = new SecondaryOutlineComponent() { AlternateColor = Color.LightBlue, Seconds = 0f, SwitchAtSeconds = .75f };
            spaceComponents.PickupComponents[id] = new PickupComponent() { PickupType = ItemType.CONSUMABLE };
            spaceComponents.ValueComponents[id] = new ValueComponent() { Gold = spaceComponents.random.Next(0, 231) };
            spaceComponents.NameComponents[id] = new NameComponent()
            {
                Name = "Test Potion",
                Description = "It is... green."
            };
            spaceComponents.ItemFunctionsComponents[id] = new ItemFunctionsComponent() { Ranged = false, UseFunctionValue = ItemUseFunctions.TESTUSE, Uses = 3, CostToUse = 20 };
            spaceComponents.CollisionComponents[id] = new CollisionComponent() { CollidedObjects = new List<Guid>(), Solid = false };
            return true;
        }
        public static bool SpawnTestArtifact(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, List<Vector2> freeTiles)
        {
            Guid id = spaceComponents.CreateEntity();
            spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.Drawable | ComponentMasks.GlowingOutline | ComponentMasks.PickupItem
                | ComponentMasks.Artifact;
            spaceComponents.DisplayComponents[id] = new DisplayComponent()
            {
                AlwaysDraw = false,
                Color = Colors.Messages.LootPickup,
                Opacity = 1f,
                Origin = Vector2.Zero,
                Rotation = 0f,
                Scale = 1f,
                SpriteEffect = SpriteEffects.None,
                SpriteSource = new Rectangle(0 * DevConstants.Grid.CellSize, 0 * DevConstants.Grid.CellSize, DevConstants.Grid.CellSize, DevConstants.Grid.CellSize),
                Symbol = "{}",
                SymbolColor = Color.White
            };
            Vector2 position = freeTiles[spaceComponents.random.Next(0, freeTiles.Count)];
            freeTiles.Remove(position);
            spaceComponents.PositionComponents[id] = new PositionComponent() { Position = position };
            spaceComponents.OutlineComponents[id] = new OutlineComponent() { Color = Color.Purple, Opacity = 1f };
            spaceComponents.SecondaryOutlineComponents[id] = new SecondaryOutlineComponent() { AlternateColor = Color.LightBlue, Seconds = 0f, SwitchAtSeconds = .75f };
            spaceComponents.PickupComponents[id] = new PickupComponent() { PickupType = ItemType.ARTIFACT };
            spaceComponents.ValueComponents[id] = new ValueComponent() { Gold = 10 };
            spaceComponents.StatModificationComponents[id] = new StatModificationComponent()
            {
                AccuracyChange = 10,
                DefenseChange = 25,
                DieNumberChange = 1,
                HealthChange = 50,
                MaximumDamageChange = 5,
                MinimumDamageChange = -2,
            };
            spaceComponents.NameComponents[id] = new NameComponent()
            {
                Name = "Test Artifact",
                Description = "FORGED IN THE FIERY PITS OF HELL, THIS MESH OF STEEL AND MAGIC HAS ONLY ONE PURPOSE: THE UTTER DECIMATION OF ALL WHO WAGE WAR AGAINST ITS OWNER.  AS YOU EQUIP THIS ITEM YOU FEEL A FORBODING PULSE ALONG YOUR SPINE WHICH RIPPLES OUTWARD INTO EVERY INCH OF YOUR FLESH.  IS IT MADNESS THAT SEEKS A NEW HOME, OR SIMPLY THE GUILT OF DONNING SUCH AN EVIL DEFENSE?"
            };
            spaceComponents.CollisionComponents[id] = new CollisionComponent() { CollidedObjects = new List<Guid>(), Solid = false };
            spaceComponents.ArtifactStatsComponents[id] = new ArtifactStatsComponent() { UpgradeLevel = 1, FloorFound = spaceComponents.GameplayInfoComponent.FloorsReached };
            return true;
        }

    }

    public static class ItemUseFunctionLookup
    {
        //SpaceComponents, Dungeongrid, Dungeondimensions, Item, User, targetposition
        public static Func<StateSpaceComponents, DungeonTile[,], Vector2, Guid, Guid, Vector2, bool> GetUseFunction(ItemUseFunctions useFunction)
        {
            switch (useFunction)
            {
                case ItemUseFunctions.TESTUSE:
                    return ItemUses.TestUse;
                default:
                    return null;
            }
        }
    }

    public static class ItemUses
    {
        public static bool TestUse(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, Guid item, Guid user, Vector2 targetPosition)
        {
            spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.StatusChange, "You use this item and something happens."));
            spaceComponents.DelayedActions.Add(new Action(() =>
            {
                Guid id = spaceComponents.CreateEntity();
                spaceComponents.Entities.Where(c => c.Id == id).First().ComponentFlags = ComponentMasks.DrawableOutline | Component.COMPONENT_TIME_TO_LIVE | ComponentMasks.GlowingOutline;
                spaceComponents.PositionComponents[id] = new PositionComponent() { Position = targetPosition };
                spaceComponents.OutlineComponents[id] = new OutlineComponent() { Color = Color.CornflowerBlue, Opacity = .8f };
                spaceComponents.TimeToLiveComponents[id] = new TimeToLiveComponent() { CurrentSecondsAlive = 0f, SecondsToLive = 4f };
                spaceComponents.SecondaryOutlineComponents[id] = new SecondaryOutlineComponent() { AlternateColor = Color.Red, Seconds = 0f, SwitchAtSeconds = .6f };
            }));
            return true;
        }
    }
}
