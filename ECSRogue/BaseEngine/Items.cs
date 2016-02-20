using ECSRogue.ECS;
using ECSRogue.ECS.Components;
using ECSRogue.ECS.Components.GraphicalEffectsComponents;
using ECSRogue.ProceduralGeneration;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.BaseEngine
{
    public enum ItemNames
    {
        NONE,
        GOLD,
        TESTCONSUMABLE,
        TESTARTIFACT
    }

    public enum ItemUseFunctions
    {
        NONE,
        TESTUSE
    }

    public static class ItemUseFunctionLookup
    {
        //SpaceComponents, Dungeongrid, Dungeondimensions, Freetiles, Item, User, targetposition
        public static Func<StateSpaceComponents, DungeonTile[,], Vector2, List<Vector2>, Guid, Guid, Vector2, bool> GetUseFunction(ItemUseFunctions useFunction)
        {
            switch(useFunction)
            {
                case ItemUseFunctions.TESTUSE:
                    return ItemUses.TestUse;
                default:
                    return null;
            }
        }
    }

    public static class ItemInfo
    {

    }

    public static class Items
    {
    }

    public static class ItemUses
    {
        public static bool TestUse(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid, Vector2 dungeonDimensions, List<Vector2> dungeonTile, Guid item, Guid user, Vector2 targetPosition)
        {
            spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.StatusChange, "You use this item and something happens."));
            spaceComponents.DelayedActions.Add(new Action(() =>
            {
                Guid id = spaceComponents.CreateEntity();
                spaceComponents.Entities.Where(c => c.Id == id).First().ComponentFlags = ComponentMasks.DrawableOutline | Component.COMPONENT_TIME_TO_LIVE;
                spaceComponents.PositionComponents[id] = new PositionComponent() { Position = targetPosition };
                spaceComponents.OutlineComponents[id] = new OutlineComponent() { Color = Color.CornflowerBlue, Opacity = .8f };
                spaceComponents.TimeToLiveComponents[id] = new TimeToLiveComponent() { CurrentSecondsAlive = 0f, SecondsToLive = 4f };
            }));
            return true;
        }
    }
}
