using ECSRogue.BaseEngine;
using ECSRogue.ECS.Components;
using ECSRogue.ECS.Components.AIComponents;
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
                    Entity collidedObject = spaceComponents.Entities.Where(x => x.Id == collidedEntity).First();
                    if (collidedObject != null && (((collidedObject.ComponentFlags & ComponentMasks.CombatReadyAI) == ComponentMasks.CombatReadyAI) || (collidedObject.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player))
                    {
                        int damageDone = 0;
                        SkillLevelsComponent collidedStats = spaceComponents.SkillLevelsComponents[collidedEntity];
                        SkillLevelsComponent attackingStats = spaceComponents.SkillLevelsComponents[id];
                        bool isPlayer = ((spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player);
                        bool isAttackingPlayer = ((spaceComponents.Entities.Where(x => x.Id == collidedEntity).First().ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player);

                        //If the two attacking creatures don't share an alignment, allow the attack to happen.
                        if (spaceComponents.AIAlignmentComponents[id].Alignment != spaceComponents.AIAlignmentComponents[collidedEntity].Alignment && collidedStats.CurrentHealth > 0 && attackingStats.CurrentHealth > 0)
                        {
                            string combatString = isPlayer ?
                                string.Format(Messages.MeleeAttackPlayer[spaceComponents.random.Next(0, Messages.MeleeAttackPlayer.Count())], spaceComponents.NameComponents[collidedEntity].Name)
                               : isAttackingPlayer?
                                   string.Format(Messages.PlayerIsAttackedMelee[spaceComponents.random.Next(0, Messages.PlayerIsAttackedMelee.Count())], spaceComponents.NameComponents[id].Name)
                                 : string.Format(Messages.MeleeAttack[spaceComponents.random.Next(0, Messages.MeleeAttack.Count())], spaceComponents.NameComponents[id].Name, spaceComponents.NameComponents[collidedEntity].Name);

                            //Hit
                            if (CombatSystem.WillMeleeAttackHit(spaceComponents.random, CombatSystem.CalculateAccuracy(attackingStats, collidedStats)))
                            {
                                //Determine weapon strength and die numbers here, then...
                                damageDone = CombatSystem.CalculateMeleeDamage(spaceComponents.random, attackingStats.MinimumDamage, attackingStats.MaximumDamage, attackingStats.DieNumber);
                                collidedStats.CurrentHealth -= damageDone;
                                if (attackingStats.TimesMissed > 5)
                                {
                                    combatString += isPlayer?
                                          string.Format(Messages.BrokenMissSpreePlayer[spaceComponents.random.Next(0, Messages.BrokenMissSpreePlayer.Count())], damageDone)
                                        : isAttackingPlayer?
                                              string.Format(Messages.BrokenPlayerDodgeSpree[spaceComponents.random.Next(0, Messages.BrokenPlayerDodgeSpree.Count())], damageDone)
                                            : string.Format(Messages.BrokenMissSpree[spaceComponents.random.Next(0, Messages.BrokenMissSpree.Count())], damageDone);
                                }
                                else
                                {
                                    combatString += isPlayer?
                                          string.Format(Messages.DamageDealtPlayer[spaceComponents.random.Next(0, Messages.DamageDealtPlayer.Count())], damageDone)
                                        : isAttackingPlayer?
                                              string.Format(Messages.PlayerTookDamage[spaceComponents.random.Next(0, Messages.PlayerTookDamage.Count())], damageDone)
                                            : string.Format(Messages.DamageDealt[spaceComponents.random.Next(0, Messages.DamageDealt.Count())], damageDone);
                                }
                                attackingStats.TimesMissed = 0;
                                Color messageColor = (spaceComponents.AIAlignmentComponents[id].Alignment == AIAlignments.ALIGNMENT_HOSTILE) ? MessageColors.Harm : MessageColors.DamageDealt;
                                spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Microsoft.Xna.Framework.Color, string>(messageColor, combatString));
                            }
                            //Miss
                            else
                            {
                                attackingStats.TimesMissed += 1;
                                if (attackingStats.TimesMissed > 5)
                                {
                                    combatString += isPlayer?
                                          string.Format(Messages.MeleeMissedALotPlayer[spaceComponents.random.Next(0, Messages.MeleeMissedALotPlayer.Count())])
                                        : isAttackingPlayer?
                                              string.Format(Messages.MeleePlayerDodgedALot[spaceComponents.random.Next(0, Messages.MeleePlayerDodgedALot.Count())])
                                            : string.Format(Messages.MeleeMissedALot[spaceComponents.random.Next(0, Messages.MeleeMissedALot.Count())]);
                                }
                                else
                                {
                                    combatString += isPlayer?
                                          string.Format(Messages.MeleeMissedPlayer[spaceComponents.random.Next(0, Messages.MeleeMissedPlayer.Count())])
                                        : isAttackingPlayer?
                                              string.Format(Messages.MeleePlayerDodged[spaceComponents.random.Next(0, Messages.MeleePlayerDodged.Count())])
                                            : string.Format(Messages.MeleeMissed[spaceComponents.random.Next(0, Messages.MeleeMissed.Count())]);
                                }
                                Color messageColor = (spaceComponents.AIAlignmentComponents[id].Alignment == AIAlignments.ALIGNMENT_HOSTILE) ? MessageColors.StatusChange : MessageColors.Failure;
                                spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Microsoft.Xna.Framework.Color, string>(MessageColors.Failure, combatString));
                            }


                            if (collidedStats.CurrentHealth <= 0)
                            {
                                spaceComponents.EntitiesToDelete.Add(collidedEntity);
                                if (isPlayer)
                                {
                                    spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(MessageColors.SpecialAction, string.Format("You killed the {0}!", spaceComponents.NameComponents[collidedEntity].Name)));
                                    GameplayInfoComponent gameInfo = spaceComponents.GameplayInfoComponent;
                                    gameInfo.Kills += 1;
                                    spaceComponents.GameplayInfoComponent = gameInfo;
                                }
                                else if(isAttackingPlayer)
                                {
                                    spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(MessageColors.SpecialAction, string.Format("You were killed by a {0}!", spaceComponents.NameComponents[id].Name)));
                                }
                                else
                                {
                                    spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(MessageColors.SpecialAction, string.Format("{0} killed the {1}!", spaceComponents.NameComponents[id].Name, spaceComponents.NameComponents[collidedEntity].Name)));
                                }
                            }
                            spaceComponents.SkillLevelsComponents[id] = attackingStats;
                            spaceComponents.SkillLevelsComponents[collidedEntity] = collidedStats;
                            //spaceComponents.DelayedActions.Add(new Action(() =>
                            //{
                            //    MakeCombatText("-" + damageDone.ToString(), MessageColors.Harm, spaceComponents, spaceComponents.PositionComponents[collidedEntity], cellSize);
                            //}));
                        }



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
