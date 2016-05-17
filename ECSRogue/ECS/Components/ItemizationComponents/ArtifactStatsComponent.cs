using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components.ItemizationComponents
{
    public struct ArtifactStatsComponent
    {
        public int UpgradeLevel;
        public int FloorFound;
        public int KillsWith;
        public int DamageTakenWith;
        public int DamageGivenWith;
        public int DodgesWith;
        public int MissesWith;
        public int MaximumComboWith;
    }
}
