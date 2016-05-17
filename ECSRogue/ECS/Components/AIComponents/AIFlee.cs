using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components.AIComponents
{
    public struct AIFlee
    {
        public double FleeAtHealthPercent;
        public double FleeUntilHealthPercent;
        public bool DoesFlee;
    }
}
