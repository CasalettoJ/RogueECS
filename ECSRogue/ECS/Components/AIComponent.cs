using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components
{
    public enum AIAlignment
    {
        ALIGNMENT_NONE,
        ALIGNMENT_FRIENDLY,
        ALIGNMENT_HOSTILE
    }

    public enum AIState
    {
        STATE_SLEEPING,
        STATE_ROAMING,
        STATE_FLEEING,
        STATE_ATTACKING
    }

    public enum AIAttackType
    {
        ATTACK_TYPE_NORMAL,
        ATTACK_TYPE_MIXED,
        ATTACK_TYPE_SPECIAL_ONLY
    }

    public struct AIComponent
    {
        public AIAlignment Alignment;
        public AIState State;
        public AIAttackType AttackType;
        public bool FleesWhenLowHealth;
        public Vector2 TargetTile;
        public Guid TargetEntity;
    }
}
