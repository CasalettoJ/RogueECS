using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components.AIComponents
{
    public enum AIAttackTypes
    {
        ATTACK_TYPE_NORMAL,
        ATTACK_TYPE_MIXED,
        ATTACK_TYPE_SPECIAL_ONLY
    }

    public struct AICombat
    {
        public AIAttackTypes AttackType;
        public bool FleesWhenLowHealth;
    }
}
