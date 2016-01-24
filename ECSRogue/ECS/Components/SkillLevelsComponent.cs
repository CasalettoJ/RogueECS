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

        public int Hunger;
        public int CurrentHunger;

        public int Wealth;

        public double Power;
        public double Accuracy;
        public double Defense;

        //"Hidden" statistics
        public double TimesMissed;
        public double TimesHit;
    }
}
