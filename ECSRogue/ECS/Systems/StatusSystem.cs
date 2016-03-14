using ECSRogue.BaseEngine;
using ECSRogue.ECS.Components;
using ECSRogue.ECS.Components.StatusComponents;
using ECSRogue.ProceduralGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class StatusSystem
    {
        public static void RegenerateHealth(StateSpaceComponents spaceComponents)
        {
            if (spaceComponents.PlayerComponent.PlayerTookTurn)
            {
                foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.HealthRegen) == ComponentMasks.HealthRegen).Select(x => x.Id))
                {
                    HealthRegenerationComponent healthRegen = spaceComponents.HealthRegenerationComponents[id];
                    healthRegen.TurnsSinceLastHeal += 1;
                    if (healthRegen.TurnsSinceLastHeal >= healthRegen.RegenerateTurnRate)
                    {
                        SkillLevelsComponent skills = spaceComponents.SkillLevelsComponents[id];
                        skills.CurrentHealth += healthRegen.HealthRegain;
                        skills.CurrentHealth = (skills.CurrentHealth >= skills.Health) ? skills.Health : skills.CurrentHealth;
                        spaceComponents.SkillLevelsComponents[id] = skills;
                    }
                    spaceComponents.HealthRegenerationComponents[id] = healthRegen;
                }
            }
        }

        public static void ApplyBurnEffect(StateSpaceComponents spaceComponents, Guid entity, int turns, int minDamage, int maxDamage)
        {
            Entity burnedEntity = spaceComponents.Entities.Where(x => x.Id == entity).FirstOrDefault();
            if (burnedEntity != null)
            {
                burnedEntity.ComponentFlags |= Component.COMPONENT_BURNING;
                spaceComponents.BurningComponents[entity] = new BurningComponent() { MaxDamage = maxDamage, MinDamage = minDamage, TurnsLeft = turns };
            }
        }

        public static void ApplyBurnDamage(StateSpaceComponents spaceComponents, DungeonTile[,] dungeonGrid)
        {
            if (spaceComponents.PlayerComponent.PlayerTookTurn)
            {
                Entity player = spaceComponents.Entities.Where(z => (z.ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER).FirstOrDefault();
                foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.BurningStatus) == ComponentMasks.BurningStatus).Select(x => x.Id))
                {
                    bool isPlayer = player.Id == id;
                    bool extinguished = false;
                    //If the entity is in water, extinguish the burning effect instead.
                    if(spaceComponents.PositionComponents.ContainsKey(id))
                    {
                        PositionComponent pos = spaceComponents.PositionComponents[id];
                        if(dungeonGrid[(int)pos.Position.X, (int)pos.Position.Y].Type == TileType.TILE_WATER)
                        {
                            spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags &= ~Component.COMPONENT_BURNING;
                            spaceComponents.DelayedActions.Add(new Action(() =>
                            {
                                spaceComponents.BurningComponents.Remove(id);
                            }));
                            if(isPlayer)
                            {
                                spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Microsoft.Xna.Framework.Color, string>(Colors.Messages.StatusChange, string.Format("[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + "You extinguish yourself in the water.")));
                            }
                            extinguished = true;
                        }
                    }
                    if(!extinguished)
                    {
                        BurningComponent burning = spaceComponents.BurningComponents[id];
                        burning.TurnsLeft -= 1;
                        SkillLevelsComponent skills = spaceComponents.SkillLevelsComponents[id];
                        int damage = spaceComponents.random.Next(burning.MinDamage, burning.MaxDamage + 1);
                        skills.CurrentHealth -= damage;
                        spaceComponents.SkillLevelsComponents[id] = skills;
                        spaceComponents.BurningComponents[id] = burning;

                        //Handle Death
                        if (skills.CurrentHealth <= 0)
                        {
                            spaceComponents.EntitiesToDelete.Add(id);
                            if (isPlayer)
                            {
                                //SCORE RECORD
                                spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Microsoft.Xna.Framework.Color, string>(Colors.Messages.Special, string.Format("[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + Messages.Deaths.FirePlayer, spaceComponents.NameComponents[id].Name)));
                            }
                            else
                            {
                                spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Microsoft.Xna.Framework.Color, string>(Colors.Messages.Special, string.Format("[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + Messages.Deaths.Fire, spaceComponents.NameComponents[id].Name)));
                                GameplayInfoComponent gameInfo = spaceComponents.GameplayInfoComponent;
                                gameInfo.Kills += 1;
                                spaceComponents.GameplayInfoComponent = gameInfo;
                            }
                        }

                        //Handle Burning running out
                        if (burning.TurnsLeft <= 0)
                        {
                            spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags &= ~Component.COMPONENT_BURNING;
                            spaceComponents.DelayedActions.Add(new Action(() =>
                            {
                                spaceComponents.BurningComponents.Remove(id);
                            }));
                        }
                    }
                    

                }
            }
        }

    }
}
