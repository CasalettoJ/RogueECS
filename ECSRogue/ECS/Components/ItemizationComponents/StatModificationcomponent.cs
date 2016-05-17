using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components.ItemizationComponents
{
    public struct StatModificationComponent
    {
        public int HealthChange;
        
        public int AccuracyChange;
        public int DefenseChange;

        public int MinimumDamageChange;
        public int MaximumDamageChange;
        public int DieNumberChange;
    }
}
