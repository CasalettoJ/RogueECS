using ECSRogue.BaseEngine;
using ECSRogue.ECS.Components;
using ECSRogue.ECS.Components.AIComponents;
using ECSRogue.ECS.Components.ItemizationComponents;
using ECSRogue.ProceduralGeneration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ECSRogue.ECS.Systems
{
    public static class ObserverSystem
    {
        public static bool CreateOrDestroyObserver(StateSpaceComponents spaceComponents, PositionComponent pos)
        {
            IEnumerable<Guid> observers = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Observer) == ComponentMasks.Observer).Select(c => c.Id);
            if (observers.Count() > 0)
            {
                foreach (Guid observer in observers)
                {
                    spaceComponents.EntitiesToDelete.Add(observer);
                }
                foreach (Entity player in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER))
                {
                    player.ComponentFlags |= Component.COMPONENT_INPUTMOVEMENT;
                }
                return false; //Do not stop movement of other inputs
            }
            else
            {
                foreach (Entity player in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER))
                {
                    player.ComponentFlags &= ~Component.COMPONENT_INPUTMOVEMENT;
                }
                spaceComponents.DelayedActions.Add(new Action(() =>
                {
                    CreateObserver(spaceComponents, pos);
                }));
                return true; //Do stop movement of other inputs
            }
        }

        public static void PrintObserversFindings(StateSpaceComponents spaceComponents, SpriteFont font, SpriteBatch spriteBatch, DungeonTile[,] dungeonGrid, Camera camera, Texture2D UITexture)
        {
            Entity observer = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Observer) == ComponentMasks.Observer).FirstOrDefault();
            if (observer != null)
            {
                PositionComponent observerPosition = spaceComponents.PositionComponents[observer.Id];
                int messageSpacing = 20;
                int messageNumber = 0;
                List<Tuple<Color, string>> observersFindings = new List<Tuple<Color, string>>();

                if(!dungeonGrid[(int)observerPosition.Position.X,(int)observerPosition.Position.Y].Found)
                {
                    observersFindings.Add(new Tuple<Color, string>( Colors.Messages.Bad, Messages.Observer.NotFound));
                }
                else
                {
                    if (dungeonGrid[(int)observerPosition.Position.X, (int)observerPosition.Position.Y].InRange)
                    {
                        observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Good, Messages.Observer.InRange));
                    }
                    else
                    {
                        observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Bad, Messages.Observer.OutOfRange));
                    }
                    switch (dungeonGrid[(int)observerPosition.Position.X, (int)observerPosition.Position.Y].Type)
                    {
                        case TileType.TILE_FLOOR:
                            observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, Messages.Observer.Floor));
                            break;
                        case TileType.TILE_WALL:
                            observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, Messages.Observer.Wall));
                            break;
                        case TileType.TILE_ROCK:
                            observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, Messages.Observer.Rock));
                            break;
                    }
                    foreach(Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.DrawableAIName) == ComponentMasks.DrawableAIName).Select(x => x.Id))
                    {
                        PositionComponent pos = spaceComponents.PositionComponents[id];
                        if(pos.Position == observerPosition.Position && dungeonGrid[(int)observerPosition.Position.X, (int)observerPosition.Position.Y].InRange)
                        {
                            observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                            if(spaceComponents.NameComponents[id].Name == "You")
                            {
                                observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("You see yourself here.")));
                            }
                            else
                            {
                                observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("You see {0} here.", spaceComponents.NameComponents[id].Name)));
                            }
                            observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, spaceComponents.NameComponents[id].Description));
                        }
                    }
                    foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.DrawableAIState) == ComponentMasks.DrawableAIState).Select(x => x.Id))
                    {
                        PositionComponent pos = spaceComponents.PositionComponents[id];
                        if (pos.Position == observerPosition.Position && dungeonGrid[(int)observerPosition.Position.X, (int)observerPosition.Position.Y].InRange)
                        {
                            AIState state = spaceComponents.AIStateComponents[id];
                            AIAlignment alignment = spaceComponents.AIAlignmentComponents[id];
                            switch(alignment.Alignment)
                            {
                                case AIAlignments.ALIGNMENT_NONE:
                                    observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, "Neutral"));
                                    break;
                                case AIAlignments.ALIGNMENT_HOSTILE:
                                    observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Bad, "Hostile"));
                                    break;
                                case AIAlignments.ALIGNMENT_FRIENDLY:
                                    observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Good, "Friendly"));
                                    break;
                            }
                            switch(state.State)
                            {
                                case AIStates.STATE_SLEEPING:
                                    observersFindings.Add(new Tuple<Color, string>(Colors.Messages.StatusChange, "Sleeping"));
                                    break;
                                case AIStates.STATE_ROAMING:
                                    observersFindings.Add(new Tuple<Color, string>(Colors.Messages.StatusChange, "Roaming"));
                                    break;
                                case AIStates.STATE_ATTACKING:
                                    observersFindings.Add(new Tuple<Color, string>(Colors.Messages.StatusChange, "Attacking"));
                                    break;
                                case AIStates.STATE_FLEEING:
                                    observersFindings.Add(new Tuple<Color, string>(Colors.Messages.StatusChange, "Fleeing"));
                                    break;
                            }
                        }
                    }
                    foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.DrawableSkills) == ComponentMasks.DrawableSkills).Select(x => x.Id))
                    {
                        PositionComponent pos = spaceComponents.PositionComponents[id];
                        if (pos.Position == observerPosition.Position && dungeonGrid[(int)observerPosition.Position.X, (int)observerPosition.Position.Y].InRange)
                        {
                            SkillLevelsComponent skills = spaceComponents.SkillLevelsComponents[id];
                            AIAlignment alignment = spaceComponents.AIAlignmentComponents[id];
                            Entity player = spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER).First();
                            observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                            observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("Health: {0} / {1}", skills.CurrentHealth, skills.Health)));
                            if(alignment.Alignment != AIAlignments.ALIGNMENT_FRIENDLY)
                            {
                                observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Good, string.Format("You have a {0}% chance to hit.", Math.Ceiling(CombatSystem.CalculateAccuracy(spaceComponents.SkillLevelsComponents[player.Id], skills)))));
                                observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Bad, string.Format("It has a {0}% chance of hitting you for a maximum of {1}.", Math.Ceiling(CombatSystem.CalculateAccuracy(skills, spaceComponents.SkillLevelsComponents[player.Id])), skills.MaximumDamage)));
                            }
                        }
                    }
                    foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.ObservableItem) == ComponentMasks.ObservableItem).Select(x => x.Id))
                    {
                        PositionComponent pos = spaceComponents.PositionComponents[id];
                        if(pos.Position == observerPosition.Position && dungeonGrid[(int)observerPosition.Position.X, (int)observerPosition.Position.Y].InRange)
                        {
                            PickupComponent pickup = spaceComponents.PickupComponents[id];
                            observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                            observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("You see {0} here.", spaceComponents.NameComponents[id].Name)));
                            observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, spaceComponents.NameComponents[id].Description));
                            switch (pickup.PickupType)
                            {
                                case ItemType.GOLD:
                                    observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Special, "Gold"));
                                    break;
                                case ItemType.CONSUMABLE:
                                    observersFindings.Add(new Tuple<Color, string>(Colors.Messages.LootPickup, "Consumable"));
                                    break;
                                case ItemType.ARTIFACT:
                                    observersFindings.Add(new Tuple<Color, string>(Colors.Messages.LootPickup, "Artifact"));
                                    break;
                            }
                        }
                    }
                    foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.ObservableValue) == ComponentMasks.ObservableValue).Select(x => x.Id))
                    {
                        PositionComponent pos = spaceComponents.PositionComponents[id];
                        if (pos.Position == observerPosition.Position && dungeonGrid[(int)observerPosition.Position.X, (int)observerPosition.Position.Y].InRange)
                        {
                            ValueComponent value = spaceComponents.ValueComponents[id];
                            observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Special, string.Format("This item is worth {0} gold.", value.Gold)));
                        }
                    }

                    foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.ObservableSkillModifications) == ComponentMasks.ObservableSkillModifications).Select(x => x.Id))
                    {
                        PositionComponent pos = spaceComponents.PositionComponents[id];
                        if (pos.Position == observerPosition.Position && dungeonGrid[(int)observerPosition.Position.X, (int)observerPosition.Position.Y].InRange)
                        {
                            StatModificationComponent stats = spaceComponents.StatModificationComponents[id];
                            observersFindings.Add(new Tuple<Color, string>(Colors.Messages.Good, "This artifact affects the following stats: "));
                            if(stats.AccuracyChange != 0)
                            {
                                string sign = stats.AccuracyChange > 0 ? "+" : string.Empty;
                                Color color = stats.AccuracyChange > 0 ? Colors.Messages.Normal : Colors.Messages.Bad;
                                observersFindings.Add(new Tuple<Color, string>(color, string.Format("Accuracy {0}{1}", sign, stats.AccuracyChange)));
                            }
                            if (stats.DefenseChange != 0)
                            {
                                string sign = stats.DefenseChange > 0 ? "+" : string.Empty;
                                Color color = stats.DefenseChange > 0 ? Colors.Messages.Normal : Colors.Messages.Bad;
                                observersFindings.Add(new Tuple<Color, string>(color, string.Format("Defense {0}{1}", sign, stats.DefenseChange)));
                            }
                            if (stats.HealthChange != 0)
                            {
                                string sign = stats.HealthChange > 0 ? "+" : string.Empty;
                                Color color = stats.HealthChange > 0 ? Colors.Messages.Normal : Colors.Messages.Bad;
                                observersFindings.Add(new Tuple<Color, string>(color, string.Format("Maximum Health {0}{1}", sign, stats.HealthChange)));
                            }
                            if (stats.PowerChange != 0)
                            {
                                string sign = stats.PowerChange > 0 ? "+" : string.Empty;
                                Color color = stats.PowerChange > 0 ? Colors.Messages.Normal : Colors.Messages.Bad;
                                observersFindings.Add(new Tuple<Color, string>(color, string.Format("Power {0}{1}", sign, stats.PowerChange)));
                            }
                            if (stats.DieNumberChange != 0)
                            {
                                string sign = stats.DieNumberChange > 0 ? "+" : string.Empty;
                                Color color = stats.DieNumberChange > 0 ? Colors.Messages.Normal : Colors.Messages.Bad;
                                observersFindings.Add(new Tuple<Color, string>(color, string.Format("Dice Number on Attack {0}{1}", sign, stats.DieNumberChange)));
                            }
                            if (stats.MinimumDamageChange != 0)
                            {
                                string sign = stats.MinimumDamageChange > 0 ? "+" : string.Empty;
                                Color color = stats.MinimumDamageChange < 0 ? Colors.Messages.Normal : Colors.Messages.Bad;
                                observersFindings.Add(new Tuple<Color, string>(color, string.Format("Minimum Damage {0}{1}", sign, stats.MinimumDamageChange)));
                            }
                            if (stats.MaximumDamageChange != 0)
                            {
                                string sign = stats.MaximumDamageChange > 0 ? "+" : string.Empty;
                                Color color = stats.MaximumDamageChange > 0 ? Colors.Messages.Normal : Colors.Messages.Bad;
                                observersFindings.Add(new Tuple<Color, string>(color, string.Format("Maximum Damage {0}{1}", sign, stats.MaximumDamageChange)));
                            }
                        }
                    }

                }

                spriteBatch.Draw(UITexture, new Rectangle(0, 0, (int)(camera.DungeonViewport.Width / 2 - 20 - DevConstants.Grid.TileSpriteSize), (int)camera.DungeonViewport.Height), Color.Black * .5f);

                foreach(Tuple<Color,string> message in observersFindings)
                {
                    if(string.IsNullOrEmpty(message.Item2))
                    {
                        continue;
                    }
                    string text = MessageDisplaySystem.WordWrap(font, message.Item2, camera.DungeonViewport.Width/2 - 30 - DevConstants.Grid.TileSpriteSize);

                    float textHeight = font.MeasureString(message.Item2).Y;
                    spriteBatch.DrawString(font, text, new Vector2(10, (int)camera.DungeonViewport.Bounds.Top + (int)textHeight + 10 + (messageNumber * messageSpacing)), message.Item1);
                    messageNumber += Regex.Matches(text, System.Environment.NewLine).Count;
                    messageNumber += 1;
                }
            }
        }

        private static void CreateObserver(StateSpaceComponents spaceComponents, PositionComponent position)
        {
            Guid id = spaceComponents.CreateEntity();
            spaceComponents.Entities.Where(x => x.Id == id).First().ComponentFlags = ComponentMasks.Observer;
            spaceComponents.PositionComponents[id] = position;
            spaceComponents.CollisionComponents[id] = new CollisionComponent() { Solid = false, CollidedObjects = new List<Guid>() };
            spaceComponents.InputMovementComponents[id] = new InputMovementComponent() { InitialWait = .3f, IsButtonDown = false, LastKeyPressed = Keys.None, TimeIntervalBetweenMovements = .1f, TimeSinceLastMovement = 0f, TotalTimeButtonDown = 0f };
            spaceComponents.DisplayComponents[id] = new DisplayComponent()
            {
                Color = Colors.Messages.Special,
                Rotation = 0f,
                Origin = Vector2.Zero,
                Scale = 1f,
                SpriteEffect = SpriteEffects.None,
                SpriteSource = new Rectangle(0 * DevConstants.Grid.CellSize, 0 * DevConstants.Grid.CellSize, DevConstants.Grid.CellSize, DevConstants.Grid.CellSize),
                AlwaysDraw = true,
                Opacity = .6f
            };

        }
    }
}
