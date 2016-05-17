using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components
{
    public struct EntityMessageComponent
    {
        public string[] NormalDodgeMessages;
        public string[] StreakDodgeMessages;
        public string[] AttackNPCMessages;
        public string[] AttackPlayerMessages;
        public string[] NormalTakeDamageMessages;
        public string[] BrokenDodgeStreakTakeDamageMessages;
        public string[] PickupItemMessages;
        public string[] ConsumablesFullMessages;
        public string[] ArtifactsFullMessages;
    }
}
