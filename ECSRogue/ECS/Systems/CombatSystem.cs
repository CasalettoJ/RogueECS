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
                    if((spaceComponents.Entities.Where(x => x.Id == collidedEntity).First().ComponentFlags & ComponentMasks.CombatReadyAI) == ComponentMasks.CombatReadyAI)
                    {
                        int damageDone = 0;
                        SkillLevelsComponent collidedStats = spaceComponents.SkillLevelsComponents[collidedEntity];
                        SkillLevelsComponent attackingStats = spaceComponents.SkillLevelsComponents[id];

                        //Check AI to see if it's an aggressive entity here
                        //Then...

                        string combatString = string.Format(Messages.MeleeAttack[spaceComponents.random.Next(0, Messages.MeleeAttack.Count())], spaceComponents.NameComponents[id].Name, spaceComponents.NameComponents[collidedEntity].Name);
                        //Hit
                        if (CombatSystem.WillMeleeAttackHit(spaceComponents.random, CombatSystem.CalculateAccuracy(attackingStats, collidedStats)))
                        {
                            //Determine weapon strength and die numbers here, then...
                            damageDone = CombatSystem.CalculateMeleeDamage(spaceComponents.random, attackingStats.MinimumDamage, attackingStats.MaximumDamage, attackingStats.DieNumber);
                            collidedStats.CurrentHealth -= damageDone;
                            if (attackingStats.TimesMissed > 5)
                            {
                                combatString += string.Format(Messages.BrokenMissSpree[spaceComponents.random.Next(0, Messages.BrokenMissSpree.Count())], damageDone);
                            }
                            else
                            {
                                combatString += string.Format(Messages.DamageDealt[spaceComponents.random.Next(0, Messages.DamageDealt.Count())], damageDone);
                            }
                            attackingStats.TimesMissed = 0;
                            spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Microsoft.Xna.Framework.Color, string>(MessageColors.DamageDealt, combatString));
                        }
                        //Miss
                        else
                        {
                            attackingStats.TimesMissed += 1;
                            if(attackingStats.TimesMissed > 5)
                            {
                                combatString += string.Format(Messages.MeleeMissedALot[spaceComponents.random.Next(0, Messages.MeleeMissedALot.Count())]);
                            }
                            else
                            {
                                combatString += string.Format(Messages.MeleeMissed[spaceComponents.random.Next(0, Messages.MeleeMissed.Count())]);
                            }
                            spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Microsoft.Xna.Framework.Color, string>(MessageColors.Failure, combatString));
                        }

                        if (collidedStats.CurrentHealth > 0)
                        {
                            spaceComponents.SkillLevelsComponents[collidedEntity] = collidedStats;
                        }
                        else
                        {
                            spaceComponents.EntitiesToDelete.Add(collidedEntity);
                            GameplayInfoComponent gameInfo = spaceComponents.GameplayInfoComponent;
                            gameInfo.Kills += 1;
                            spaceComponents.GameplayInfoComponent = gameInfo;
                            spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(MessageColors.SpecialAction, string.Format("{0} killed the {1}!", spaceComponents.NameComponents[id].Name, spaceComponents.NameComponents[collidedEntity].Name)));
                        }
                        spaceComponents.SkillLevelsComponents[id] = attackingStats;
                        //spaceComponents.DelayedActions.Add(new Action(() =>
                        //{
                        //    MakeCombatText("-" + damageDone.ToString(), MessageColors.Harm, spaceComponents, spaceComponents.PositionComponents[collidedEntity], cellSize);
                        //}));

                    }
                }
            }
        }

        public static bool WillMeleeAttackHit(Random random, double accuracy)
        {
            if (accuracy >= 100.00)
            {
                return true;
            }
            return random.NextDouble()*100 <= accuracy;
        }

        public static double CalculateAccuracy(SkillLevelsComponent attacker, SkillLevelsComponent defender)
        {
            return attacker.Accuracy * (Math.Pow(.95, defender.Defense));
        }

        public static int CalculateMeleeDamage(Random random, int minDamage, int maxDamage, int diceNum)
        {
            int damage = 0;
            for(int i = 0; i < diceNum; i++)
            {
                damage += random.Next((int)(minDamage / diceNum), (int)(maxDamage / diceNum) + 1);
            }
            return damage;
        }

        public static int CalculateRangeDamate()
        {
            return 0;
        }



        //public static void MakeCombatText(string text, Color messageColor, StateSpaceComponents spaceComponents, PositionComponent position, int cellSize)
        //{
        //    Guid id = spaceComponents.CreateEntity();
        //    spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.DrawableLabel | ComponentMasks.MovingEntity;
        //    spaceComponents.PositionComponents[id] = new PositionComponent() { Position = new Vector2(position.Position.X *= cellSize, position.Position.Y *= cellSize) };
        //    spaceComponents.LabelComponents[id] = new LabelComponent()
        //    {
        //        Color = messageColor,
        //        Origin = Vector2.Zero,
        //        Rotation = 0f,
        //        Scale = 1.25f,
        //        SpriteEffect = SpriteEffects.None,
        //        Text = text
        //    };
        //    spaceComponents.VelocityComponents[id] = new VelocityComponent() { Velocity = new Vector2(150,150)};
        //    spaceComponents.TargetPositionComponents[id] = new TargetPositionComponent() { DestroyWhenReached = true, TargetPosition = new Vector2(spaceComponents.PositionComponents[id].Position.X, spaceComponents.PositionComponents[id].Position.Y - 100) };
        //}


    }
}
