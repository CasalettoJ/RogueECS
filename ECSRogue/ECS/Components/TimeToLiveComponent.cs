using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components
{
    public struct TimeToLiveComponent
    {
        public float SecondsToLive;
        public float CurrentSecondsAlive;
    }
}
