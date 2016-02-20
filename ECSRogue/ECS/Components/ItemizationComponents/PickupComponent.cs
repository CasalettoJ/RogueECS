using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components.ItemizationComponents
{
    public enum ItemType
    {
        NONE,
        CONSUMABLE,
        ARTIFACT,
        GOLD
    }

    public struct PickupComponent
    {
        public ItemType PickupType;
    }
}
