using ECSRogue.BaseEngine;
using ECSRogue.ECS.Components;
using Microsoft.Xna.Framework;
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
            Guid id = spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER).Select(x => x.Id).First();
            SkillLevelsComponent skills = spaceComponents.SkillLevelsComponents[id];
            GameplayInfoComponent gameInfo = spaceComponents.GameplayInfoComponent;
            stateComponents.GameplayInfo = gameInfo;
            stateComponents.PlayerSkillLevels = skills;
        }

        public static void LoadPlayerSkillset(StateComponents stateComponents, StateSpaceComponents spaceComponents)
        {
            foreach(Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER).Select(x => x.Id))
            if (stateComponents != null)
            {
                spaceComponents.SkillLevelsComponents[id] = stateComponents.PlayerSkillLevels;
            }
            else
            {
                spaceComponents.SkillLevelsComponents[id] = new SkillLevelsComponent()
                {
                    CurrentHealth = 100,
                    Health = 100,
                    Defense = 50,
                    Accuracy = 100,
                    Wealth = 0
                };
            }
        }

        public static void CreateGameplayInfo(StateComponents stateComponents, StateSpaceComponents spaceComponents)
        {
            if (stateComponents != null)
            {
                GameplayInfoComponent info = stateComponents.GameplayInfo;
                info.FloorsReached += 1;
                spaceComponents.GameplayInfoComponent = info;
            }
            else
            {
                //Set GameplayInfo
                spaceComponents.GameplayInfoComponent = new GameplayInfoComponent() { Kills = 0, StepsTaken = 0, FloorsReached = 1, Madness = 0 };
            }
        }

        public static void CreateMessageLog(StateSpaceComponents spaceComponents)
        {
            Guid id = spaceComponents.CreateEntity();
            spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = Component.COMPONENT_GAMEMESSAGE;
            spaceComponents.GameMessageComponent = new GameMessageComponent()
            {
                GlobalColor = Color.White,
                GlobalMessage = string.Empty,
                MaxMessages = 100,
                IndexBegin = 0,
                GameMessages = new List<Tuple<Color, string>>()
            };
            if (spaceComponents.GameplayInfoComponent.FloorsReached <= 1)
            {
                MessageDisplaySystem.GenerateRandomGameMessage(spaceComponents, Messages.GameEntranceMessages, Colors.Messages.Special);
            }
            else
            {
                MessageDisplaySystem.GenerateRandomGameMessage(spaceComponents, Messages.CaveEntranceMessages, Colors.Messages.Special);
            }
        }
    }
}
