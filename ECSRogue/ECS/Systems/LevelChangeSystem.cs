using ECSRogue.BaseEngine;
using ECSRogue.ECS.Components;
using ECSRogue.ECS.Components.ItemizationComponents;
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

        public static void RetainNecessaryComponents(StateComponents stateComponents, StateSpaceComponents spaceComponents)
        {
            //Transfer components, then delete all AI-related components and all item related components that aren't in the players' inventories.
            stateComponents.StateSpaceComponents = spaceComponents;
            foreach(Guid id in stateComponents.StateSpaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.CombatReadyAI) == ComponentMasks.CombatReadyAI).Select(x => x.Id))
            {
                //Change this to only hostile AI when allies need to be implemented.
                stateComponents.StateSpaceComponents.EntitiesToDelete.Add(id);
            }
            foreach (Guid id in stateComponents.StateSpaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.PickupItem) == ComponentMasks.PickupItem).Select(x => x.Id))
            {
                foreach(Guid player in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER).Select(x => x.Id))
                {
                    InventoryComponent inventory = stateComponents.StateSpaceComponents.InventoryComponents[player];
                    if(!inventory.Artifacts.Contains(id) && !inventory.Consumables.Contains(id))
                    {
                        stateComponents.StateSpaceComponents.EntitiesToDelete.Add(id);
                        break;
                    }
                }
            }
        }

        public static void LoadPlayerSkillset(StateComponents stateComponents, StateSpaceComponents spaceComponents)
        {
            foreach(Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER).Select(x => x.Id))
            {
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
