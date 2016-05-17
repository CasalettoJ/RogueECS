using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components.ItemizationComponents
{
    public struct InventoryComponent
    {
        public List<Guid> Consumables;
        public List<Guid> Artifacts;
        public int MaxConsumables;
        public int MaxArtifacts;
    }
}
