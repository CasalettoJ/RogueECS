using ECSRogue.ECS.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class LevelChangeSystem
    {
        public static void RetainPlayerStatistics(StateComponents stateComponents, StateSpaceComponents spaceComponents)
        {
            Guid id = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).Select(x => x.Id).First();
            SkillLevelsComponent skills = spaceComponents.SkillLevelsComponents[id];
            GameplayInfoComponent gameInfo = spaceComponents.GameplayInfoComponents[id];
            stateComponents.GameplayInfo = gameInfo;
            stateComponents.PlayerSkillLevels = skills;
        }
    }
}
