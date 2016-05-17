using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components
{
    public struct CollisionComponent
    {
        public bool Solid;
        public List<Guid> CollidedObjects;
    }
}
