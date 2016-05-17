using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components
{
    public struct InputMovementComponent
    {
        public float TimeSinceLastMovement;
        public float TimeIntervalBetweenMovements;
        public float InitialWait;
        public float TotalTimeButtonDown;
        public bool IsButtonDown;
        public Keys LastKeyPressed;
    }
}
