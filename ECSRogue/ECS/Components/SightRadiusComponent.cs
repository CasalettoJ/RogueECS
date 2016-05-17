using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components
{
    public struct SightRadiusComponent
    {
        public int CurrentRadius;
        public int MaxRadius;
        public bool DrawRadius;
    }
}
