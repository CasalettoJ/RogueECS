using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Components
{
    public struct SkillLevelsComponent
    {
        public int Health;
        public int CurrentHealth;

        public int Wealth;

        public int PhysicalAttack;
        public int MagicAttack;
        public int PhysicalDefense;
        public int MagicDefense;
    }
}
