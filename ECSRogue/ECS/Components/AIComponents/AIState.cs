using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components.AIComponents
{
    public enum AIStates
    {
        STATE_SLEEPING,
        STATE_ROAMING,
        STATE_FLEEING,
        STATE_ATTACKING
    }

    public struct AIState
    {
        public AIStates State;
    }
}
