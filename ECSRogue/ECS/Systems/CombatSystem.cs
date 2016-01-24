using ECSRogue.BaseEngine;
using ECSRogue.ECS.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.ECS.Systems
{
    public static class CombatSystem
    {
        public static void HandleMeleeCombat(StateSpaceComponents spaceComponents, int cellSize)
        {
            IEnumerable<Guid> entities = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Collidable) == ComponentMasks.Collidable).Select(x => x.Id);
            foreach (Guid id in entities)
            {
                foreach(Guid collidedEntity in spaceComponents.CollisionComponents[id].CollidedObjects)
                {
                    if((spaceComponents.Entities.Where(x => x.Id == collidedEntity).First().ComponentFlags & ComponentMasks.NPC) == ComponentMasks.NPC)
                    {
                        int damageDone = spaceComponents.random.Next(0, (int)spaceComponents.SkillLevelsComponents[id].Power); //Replace with actual damage calculation
                        SkillLevelsComponent collidedStats = spaceComponents.SkillLevelsComponents[collidedEntity];
                        collidedStats.CurrentHealth -= damageDone;
                        string combatString = string.Format(Messages.MeleeAttack[spaceComponents.random.Next(0, Messages.MeleeAttack.Count())], spaceComponents.NameComponents[collidedEntity].Name);
                        combatString += string.Format(Messages.DamageDealt[spaceComponents.random.Next(0, Messages.DamageDealt.Count())], damageDone);
                        if (collidedStats.CurrentHealth > 0)
                        {
                            spaceComponents.SkillLevelsComponents[collidedEntity] = collidedStats;
                            spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Microsoft.Xna.Framework.Color, string>(MessageColors.DamageDealt, combatString));
                        }
                        else
                        {
                            spaceComponents.EntitiesToDelete.Add(collidedEntity);
                            GameplayInfoComponent gameInfo = spaceComponents.GameplayInfoComponent;
                            gameInfo.Kills += 1;
                            spaceComponents.GameplayInfoComponent = gameInfo;
                            spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(MessageColors.SpecialAction, string.Format("You killed the {0}!", spaceComponents.NameComponents[collidedEntity].Name)));
                        }
                        //spaceComponents.DelayedActions.Add(new Action(() =>
                        //{
                        //    MakeCombatText("-" + damageDone.ToString(), MessageColors.Harm, spaceComponents, spaceComponents.PositionComponents[collidedEntity], cellSize);
                        //}));

                    }
                }
            }
        }

        public static void MakeCombatText(string text, Color messageColor, StateSpaceComponents spaceComponents, PositionComponent position, int cellSize)
        {
            Guid id = spaceComponents.CreateEntity();
            spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.DrawableLabel | ComponentMasks.MovingEntity;
            spaceComponents.PositionComponents[id] = new PositionComponent() { Position = new Vector2(position.Position.X *= cellSize, position.Position.Y *= cellSize) };
            spaceComponents.LabelComponents[id] = new LabelComponent()
            {
                Color = messageColor,
                Origin = Vector2.Zero,
                Rotation = 0f,
                Scale = 1.25f,
                SpriteEffect = SpriteEffects.None,
                Text = text
            };
            spaceComponents.VelocityComponents[id] = new VelocityComponent() { Velocity = new Vector2(150,150)};
            spaceComponents.TargetPositionComponents[id] = new TargetPositionComponent() { DestroyWhenReached = true, TargetPosition = new Vector2(spaceComponents.PositionComponents[id].Position.X, spaceComponents.PositionComponents[id].Position.Y - 100) };
        }


    }
}
