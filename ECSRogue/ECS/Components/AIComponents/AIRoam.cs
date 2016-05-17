using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components.AIComponents
{
    public struct AIRoam
    {
        public Vector2 TargetTile;
        public int ChanceToDetect;
    }
}
