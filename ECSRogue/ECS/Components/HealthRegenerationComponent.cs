using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components
{
    public struct HealthRegenerationComponent
    {
        public int RegenerateTurnRate;
        public int TurnsSinceLastHeal;
        public int HealthRegain;
    }
}
