using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.BaseEngine.IO.Objects
{
    public class GameSettings
    {
        public Vector2 Resolution;
        public float Scale;
        public bool Vsync;
        public bool ShowGlow;
        public bool Borderless;
        public bool HasChanges;
    }
}
