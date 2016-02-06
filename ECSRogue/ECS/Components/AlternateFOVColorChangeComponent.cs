using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components
{
    public struct AlternateFOVColorChangeComponent
    {
        public Color AlternateColor;
        public float Seconds;
        public float SwitchAtSeconds;
    }
}
