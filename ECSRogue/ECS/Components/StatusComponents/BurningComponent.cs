using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components.StatusComponents
{
    public struct BurningComponent
    {
        public int TurnsLeft;
        public int MaxDamage;
        public int MinDamage;
    }
}
