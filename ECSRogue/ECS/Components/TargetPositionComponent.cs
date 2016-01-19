using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components
{
    public struct TargetPositionComponent
    {
        public Vector2 TargetPosition;
        public bool DestroyWhenReached;
    }
}
