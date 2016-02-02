using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components
{
    public struct GlobalCollisionComponent
    {
        public List<Guid> EntitiesThatCollided;
    }
}
