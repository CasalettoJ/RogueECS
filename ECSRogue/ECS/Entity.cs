using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS
{
    public class Entity
    {
        public Guid Id { get; set; }
        public Component ComponentFlags { get; set; }

        public Entity()
        {
            Id = Guid.NewGuid();
            ComponentFlags = Component.NONE;
        }
    }
}
