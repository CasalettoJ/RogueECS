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
        
        public double Accuracy;
        public double Defense;

        public int MinimumDamage;
        public int MaximumDamage;
        public int DieNumber;

        //"Hidden" statistics
        public double TimesMissed;
        public double TimesHit;
    }
}
