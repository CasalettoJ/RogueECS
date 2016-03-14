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
            IEnumerable<Guid> entities = spaceComponents.GlobalCollisionComponent.EntitiesThatCollided.Distinct();
            foreach (Guid id in entities)
            {
                Entity collidingObject = spaceComponents.Entities.Where(x => x.Id == id).FirstOrDefault();
                if(collidingObject == null || (((collidingObject.ComponentFlags & ComponentMasks.CombatReadyAI) != ComponentMasks.CombatReadyAI) && (collidingObject.ComponentFlags & Component.COMPONENT_PLAYER) != Component.COMPONENT_PLAYER))
                {
                    //If the colliding object isn't a combat ready AI or a player, don't try to do combat with it.
                    continue;
                }
                foreach(Guid collidedEntity in spaceComponents.CollisionComponents[id].CollidedObjects)
                {
                    Entity collidedObject = spaceComponents.Entities.Where(x => x.Id == collidedEntity).FirstOrDefault();
                    if (collidedObject != null && (((collidedObject.ComponentFlags & ComponentMasks.CombatReadyAI) == ComponentMasks.CombatReadyAI) || (collidedObject.ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER))
                    {
                        int damageDone = 0;
                        SkillLevelsComponent collidedStats = spaceComponents.SkillLevelsComponents[collidedEntity];
                        SkillLevelsComponent attackingStats = spaceComponents.SkillLevelsComponents[id];
                        EntityMessageComponent collidedMessages = spaceComponents.EntityMessageComponents[collidedEntity];
                        EntityMessageComponent attackingMessages = spaceComponents.EntityMessageComponents[id];
                        bool isPlayerAttacking = ((spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER);
                        bool isPlayerBeingAttacked = ((spaceComponents.Entities.Where(x => x.Id == collidedEntity).First().ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER);

                        //If the two attacking creatures don't share an alignment, allow the attack to happen.
                        if (spaceComponents.AIAlignmentComponents[id].Alignment != spaceComponents.AIAlignmentComponents[collidedEntity].Alignment && collidedStats.CurrentHealth > 0 && attackingStats.CurrentHealth > 0)
                        {
                            string combatString = "[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] ";
                             combatString += isPlayerBeingAttacked ?
                                string.Format(attackingMessages.AttackPlayerMessages[spaceComponents.random.Next(0, attackingMessages.AttackPlayerMessages.Count())], spaceComponents.NameComponents[id].Name)
                                : string.Format(attackingMessages.AttackNPCMessages[spaceComponents.random.Next(0, attackingMessages.AttackNPCMessages.Count())], spaceComponents.NameComponents[id].Name, spaceComponents.NameComponents[collidedEntity].Name);

                            //Hit
                            if (CombatSystem.WillMeleeAttackHit(spaceComponents.random, CombatSystem.CalculateAccuracy(spaceComponents, attackingStats, id, collidedStats, collidedEntity)))
                            {
                                //Determine weapon strength and die numbers here, then...
                                damageDone = CombatSystem.CalculateMeleeDamage(spaceComponents, attackingStats, id);
                                collidedStats.CurrentHealth -= damageDone;
                                if (attackingStats.TimesMissed > 5)
                                {
                                    combatString += string.Format(collidedMessages.BrokenDodgeStreakTakeDamageMessages[spaceComponents.random.Next(0, collidedMessages.BrokenDodgeStreakTakeDamageMessages.Count())], damageDone);
                                }
                                else
                                {
                                    combatString += string.Format(collidedMessages.NormalTakeDamageMessages[spaceComponents.random.Next(0, collidedMessages.NormalTakeDamageMessages.Count())], damageDone);
                                }
                                attackingStats.TimesHit += 1;
                                attackingStats.TimesMissed = 0;
                                Color messageColor = (spaceComponents.AIAlignmentComponents[id].Alignment == AIAlignments.ALIGNMENT_HOSTILE) ? Colors.Messages.Bad : Colors.Messages.Good;
                                spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Microsoft.Xna.Framework.Color, string>(messageColor, combatString));
                                InventorySystem.IncrementDamageGivenWithArtifact(spaceComponents, id, damageDone);
                                InventorySystem.UpdateMaxCombo(spaceComponents, id, (int)attackingStats.TimesHit);
                                InventorySystem.IncrementDamageTakenWithArtifact(spaceComponents, collidedEntity, damageDone);
                            }
                            //Miss
                            else
                            {
                                attackingStats.TimesMissed += 1;
                                if (attackingStats.TimesMissed > 5)
                                {
                                    combatString += string.Format(collidedMessages.StreakDodgeMessages[spaceComponents.random.Next(0, collidedMessages.StreakDodgeMessages.Count())], damageDone);
                                }
                                else
                                {
                                    combatString += string.Format(collidedMessages.NormalDodgeMessages[spaceComponents.random.Next(0, collidedMessages.NormalDodgeMessages.Count())], damageDone);
                                }
                                attackingStats.TimesHit = 0;
                                Color messageColor = (spaceComponents.AIAlignmentComponents[id].Alignment == AIAlignments.ALIGNMENT_HOSTILE) ? Colors.Messages.Good : Colors.Messages.Bad;
                                spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Microsoft.Xna.Framework.Color, string>(messageColor, combatString));
                                InventorySystem.IncrementTimesDodgedWithArtifact(spaceComponents, collidedEntity);
                                InventorySystem.IncrementTimesMissesWithArtifact(spaceComponents, id);
                            }


                            if (collidedStats.CurrentHealth <= 0)
                            {
                                InventorySystem.IncrementKillsWithArtifact(spaceComponents, id);
                                spaceComponents.EntitiesToDelete.Add(collidedEntity);
                                if (isPlayerAttacking)
                                {
                                    spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.Special, string.Format("[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] "+ "You killed the {0}!", spaceComponents.NameComponents[collidedEntity].Name)));
                                    GameplayInfoComponent gameInfo = spaceComponents.GameplayInfoComponent;
                                    gameInfo.Kills += 1;
                                    spaceComponents.GameplayInfoComponent = gameInfo;
                                }
                                else if(isPlayerBeingAttacked)
                                {
                                    spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.Special, string.Format("[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + "You were killed by a {0}!", spaceComponents.NameComponents[id].Name)));
                                    //SCORE RECORD
                                }
                                else
                                {
                                    spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(Colors.Messages.Special, string.Format("[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + "{0} killed the {1}!", spaceComponents.NameComponents[id].Name, spaceComponents.NameComponents[collidedEntity].Name)));
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

        public static double CalculateAccuracy(StateSpaceComponents spaceComponents, SkillLevelsComponent attacker, Guid attackerId, SkillLevelsComponent defender, Guid defenderId)
        {
            attacker = InventorySystem.ApplyStatModifications(spaceComponents, attackerId, attacker);
            defender = InventorySystem.ApplyStatModifications(spaceComponents, defenderId, defender);
            return attacker.Accuracy * (Math.Pow(.95, defender.Defense));
        }

        public static int CalculateMeleeDamage(StateSpaceComponents spaceComponents, SkillLevelsComponent attackingStats, Guid attacker)
        {
            attackingStats = InventorySystem.ApplyStatModifications(spaceComponents, attacker, attackingStats);

            int damage = 0;
            for(int i = 0; i < attackingStats.DieNumber; i++)
            {
                damage += spaceComponents.random.Next((int)(attackingStats.MinimumDamage / attackingStats.DieNumber), (int)(attackingStats.MaximumDamage / attackingStats.DieNumber) + 1);
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
