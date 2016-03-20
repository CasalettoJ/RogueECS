using ECSRogue.BaseEngine;
using ECSRogue.ECS.Components;
using ECSRogue.ECS.Components.AIComponents;
using ECSRogue.ECS.Components.ItemizationComponents;
using ECSRogue.ECS.Components.StatusComponents;
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
        public static bool CreateOrDestroyObserver(StateSpaceComponents spaceComponents)
        {
            //Get the first player's position
            PositionComponent pos = spaceComponents.PositionComponents[spaceComponents.Entities.Where(c => (c.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).First().Id];

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

        public static void HandleObserverFindings(StateSpaceComponents spaceComponents, KeyboardState key, KeyboardState prevKey, DungeonTile[,] dungeonGrid)
        {
            ObserverComponent observer = spaceComponents.ObserverComponent;
            Entity observerInfo = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Observer) == ComponentMasks.Observer).FirstOrDefault();
            
            Vector2 playerPos = spaceComponents.PositionComponents[spaceComponents.Entities.Where(c => (c.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).First().Id].Position;
            bool inWater = dungeonGrid[(int)playerPos.X, (int)playerPos.Y].Type == TileType.TILE_WATER;
            observer.SeeUnknown = false;

            if (observerInfo != null)
            {
                observer.Observed = new List<Guid>();
                int selectedItem = 0;
                PositionComponent observerPosition = spaceComponents.PositionComponents[observerInfo.Id];

                //gather a list of all observed items
                foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Observable) == ComponentMasks.Observable).Select(x => x.Id))
                {

                    PositionComponent enPos = spaceComponents.PositionComponents[id];
                    if (dungeonGrid[(int)observerPosition.Position.X, (int)observerPosition.Position.Y].InRange && enPos.Position == observerPosition.Position)
                    {
                        bool observedInWater = dungeonGrid[(int)enPos.Position.X, (int)enPos.Position.Y].Type == TileType.TILE_WATER;
                        if (observedInWater != inWater)
                        {
                            observer.SeeUnknown = true;
                            break;
                        }
                        observer.Observed.Add(id);
                    }
                }

                //set the current observed item to the first item if there isn't one yet
                if ((observer.SelectedItem == Guid.Empty && observer.Observed.Count > 0) || (observer.Observed.Count > 0 && !observer.Observed.Contains(observer.SelectedItem)))
                {
                    observer.SelectedItem = observer.Observed[selectedItem];
                }

                //set the index of the selected item
                if (observer.Observed.Count > 0)
                {
                    selectedItem = observer.Observed.IndexOf(observer.SelectedItem);
                }

                //Change the index if necessary
                if (key.IsKeyDown(Keys.Down) && prevKey.IsKeyUp(Keys.Down))
                {
                    selectedItem += 1;
                    if (selectedItem >= observer.Observed.Count)
                    {
                        selectedItem = 0;
                    }
                }
                else if (key.IsKeyDown(Keys.Up) && prevKey.IsKeyUp(Keys.Up))
                {
                    selectedItem -= 1;
                    if (selectedItem < 0)
                    {
                        selectedItem = observer.Observed.Count - 1;
                    }
                }

                //Update the component
                if (observer.Observed.Count > 0)
                {
                    observer.SelectedItem = observer.Observed[selectedItem];
                }

                spaceComponents.ObserverComponent = observer;
            }
        }

        public static void PrintObserver(StateSpaceComponents spaceComponents, SpriteFont font, SpriteBatch spriteBatch, DungeonTile[,] dungeonGrid, Camera camera, Texture2D UITexture)
        {
            ObserverComponent observer = spaceComponents.ObserverComponent;
            Entity observerInfo = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Observer) == ComponentMasks.Observer).FirstOrDefault();
            if(observerInfo != null)
            {
                //Find out where the observer is
                PositionComponent observerPosition = spaceComponents.PositionComponents[observerInfo.Id];

                //Obtain the list of entities from the observer component
                List<Entity> observedItems = new List<Entity>();
                foreach (Guid id in observer.Observed)
                {
                    Entity entity = spaceComponents.Entities.Where(x => x.Id == id).FirstOrDefault();
                    if (entity != null)
                    {
                        observedItems.Add(entity);
                    }
                }

                //Set the initial variables
                int messageSpacing = (int)font.MeasureString("g").Y + 1;
                int messageLeft = 0;
                int messageRight = 0;
                int panelWidth = (int)((camera.DungeonViewport.Width / 2) - (DevConstants.Grid.CellSize * 2));
                List<Tuple<Color, string>> leftFindings = new List<Tuple<Color, string>>();
                List<Tuple<Color, string>> rightFindings = new List<Tuple<Color, string>>();

                //Gather information for the left side
                if (!dungeonGrid[(int)observerPosition.Position.X, (int)observerPosition.Position.Y].Found)
                {
                    leftFindings.Add(new Tuple<Color, string>(Colors.Messages.Bad, Messages.Observer.NotFound));
                }
                else
                {
                    if (dungeonGrid[(int)observerPosition.Position.X, (int)observerPosition.Position.Y].InRange)
                    {
                        leftFindings.Add(new Tuple<Color, string>(Colors.Messages.Good, Messages.Observer.InRange));
                    }
                    else
                    {
                        leftFindings.Add(new Tuple<Color, string>(Colors.Messages.Bad, Messages.Observer.OutOfRange));
                    }
                    leftFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                    switch (dungeonGrid[(int)observerPosition.Position.X, (int)observerPosition.Position.Y].Type)
                    {
                        case TileType.TILE_FLOOR:
                            leftFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, Messages.Observer.Floor));
                            break;
                        case TileType.TILE_WALL:
                            leftFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, Messages.Observer.Wall));
                            break;
                        case TileType.TILE_ROCK:
                            leftFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, Messages.Observer.Rock));
                            break;
                        case TileType.TILE_WATER:
                            leftFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, Messages.Observer.Water));
                            break;
                        case TileType.TILE_TALLGRASS:
                            leftFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, Messages.Observer.TallGrass));
                            break;
                        case TileType.TILE_FLATTENEDGRASS:
                            leftFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, Messages.Observer.FlatGrass));
                            break;
                        case TileType.TILE_FIRE:
                            leftFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, Messages.Observer.Fire));
                            break;
                        case TileType.TILE_ASH:
                            leftFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, Messages.Observer.Ash));
                            break;

                    }
                }

                if(observer.SeeUnknown)
                {
                    leftFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                    leftFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, Messages.Observer.Unknown));
                }

                if(observer.Observed.Count > 0)
                {
                    leftFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                    leftFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, "From here you can see the following: "));
                }
                //Gather observed names for the left side
                foreach(Entity en in observedItems)
                {
                    string prepend = (en.Id == observer.SelectedItem) ? "> " : string.Empty;
                    Color color = (en.Id == observer.SelectedItem) ? Colors.Messages.LootPickup : Colors.Messages.Normal;
                    leftFindings.Add(new Tuple<Color, string>(color, prepend + spaceComponents.NameComponents[en.Id].Name));
                }

                //Gather information for right side
                Entity selectedEntity = spaceComponents.Entities.Where(x => x.Id == observer.SelectedItem).FirstOrDefault();
                if(selectedEntity != null)
                {
                    if (spaceComponents.NameComponents[selectedEntity.Id].Name == "You")
                    {
                        rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("You see yourself here.")));
                    }
                    else
                    {
                        rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, spaceComponents.NameComponents[selectedEntity.Id].Name));
                    }
                    rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, spaceComponents.NameComponents[selectedEntity.Id].Description));

                    //If the finding is an AI, gather the information for it
                    if((selectedEntity.ComponentFlags & ComponentMasks.ObservableAI) == ComponentMasks.ObservableAI)
                    {
                        AIState state = spaceComponents.AIStateComponents[selectedEntity.Id];
                        AIAlignment alignment = spaceComponents.AIAlignmentComponents[selectedEntity.Id];
                        SkillLevelsComponent skills = spaceComponents.SkillLevelsComponents[selectedEntity.Id];

                        switch (alignment.Alignment)
                        {
                            case AIAlignments.ALIGNMENT_NONE:
                                rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, "Neutral"));
                                break;
                            case AIAlignments.ALIGNMENT_HOSTILE:
                                rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Bad, "Hostile"));
                                break;
                            case AIAlignments.ALIGNMENT_FRIENDLY:
                                rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Good, "Friendly"));
                                break;
                        }
                        switch (state.State)
                        {
                            case AIStates.STATE_SLEEPING:
                                rightFindings.Add(new Tuple<Color, string>(Colors.Messages.StatusChange, "Sleeping"));
                                break;
                            case AIStates.STATE_ROAMING:
                                rightFindings.Add(new Tuple<Color, string>(Colors.Messages.StatusChange, "Roaming"));
                                break;
                            case AIStates.STATE_ATTACKING:
                                rightFindings.Add(new Tuple<Color, string>(Colors.Messages.StatusChange, "Attacking"));
                                break;
                            case AIStates.STATE_FLEEING:
                                rightFindings.Add(new Tuple<Color, string>(Colors.Messages.StatusChange, "Fleeing"));
                                break;
                        }
                        //Status Effects:
                        Statuses statuses = StatusSystem.GetStatusEffectsOfEntity(spaceComponents, selectedEntity.Id, dungeonGrid);
                        if (statuses == Statuses.NONE)
                        {
                            rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, Messages.StatusMessages.Normal));
                            rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                        }
                        //If there are status effects on the player..
                        else
                        {
                            if ((statuses & Statuses.BURNING) == Statuses.BURNING)
                            {
                                BurningComponent burning = spaceComponents.BurningComponents[selectedEntity.Id];
                                rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Bad, string.Format(Messages.StatusMessages.Burning, burning.MinDamage, burning.MaxDamage, burning.TurnsLeft)));
                            }
                            if ((statuses & Statuses.UNDERWATER) == Statuses.UNDERWATER)
                            {
                                rightFindings.Add(new Tuple<Color, string>(Colors.Caves.WaterInRange, Messages.StatusMessages.Underwater));
                            }
                            if ((statuses & Statuses.HEALTHREGEN) == Statuses.HEALTHREGEN)
                            {
                                HealthRegenerationComponent healthRegen = spaceComponents.HealthRegenerationComponents[selectedEntity.Id];
                                rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Good, string.Format(Messages.StatusMessages.HealthRegen, healthRegen.HealthRegain, healthRegen.RegenerateTurnRate)));
                            }
                            rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                        }


                        Entity player = spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER).First();
                        rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                        rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("Health: {0} / {1}", skills.CurrentHealth, skills.Health)));
                        if (alignment.Alignment != AIAlignments.ALIGNMENT_FRIENDLY)
                        {
                            rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Good, string.Format("You have a {0}% chance to hit.", Math.Ceiling(CombatSystem.CalculateAccuracy(spaceComponents, spaceComponents.SkillLevelsComponents[player.Id], player.Id, skills, selectedEntity.Id)))));
                            rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Bad, string.Format("It has a {0}% chance of hitting you for a maximum of {1}.", Math.Ceiling(CombatSystem.CalculateAccuracy(spaceComponents, skills, selectedEntity.Id, spaceComponents.SkillLevelsComponents[player.Id], player.Id)), skills.MaximumDamage)));
                        }
                    }

                    //If the observed item is an item, gather that information instead
                    if ((selectedEntity.ComponentFlags & ComponentMasks.ObservableItem) == ComponentMasks.ObservableItem)
                    {
                        rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                        PickupComponent pickup = spaceComponents.PickupComponents[selectedEntity.Id];
                        switch (pickup.PickupType)
                        {
                            case ItemType.GOLD:
                                rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Special, "Gold"));
                                break;
                            case ItemType.CONSUMABLE:
                                rightFindings.Add(new Tuple<Color, string>(Colors.Messages.LootPickup, "Consumable"));
                                break;
                            case ItemType.ARTIFACT:
                                rightFindings.Add(new Tuple<Color, string>(Colors.Messages.LootPickup, "Artifact"));
                                break;
                        }

                        if((selectedEntity.ComponentFlags & ComponentMasks.ObservableValue) == ComponentMasks.ObservableValue && pickup.PickupType != ItemType.DOWNSTAIRS)
                        {
                            ValueComponent value = spaceComponents.ValueComponents[selectedEntity.Id];
                            rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Special, string.Format("This item is worth {0} gold.", value.Gold)));
                        }

                        if ((selectedEntity.ComponentFlags & ComponentMasks.ObservableSkillModifications) == ComponentMasks.ObservableSkillModifications)
                        {
                            StatModificationComponent stats = spaceComponents.StatModificationComponents[selectedEntity.Id];
                            rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                            rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, "This artifact affects the following stats: "));
                            if (stats.AccuracyChange != 0)
                            {
                                string sign = stats.AccuracyChange > 0 ? "+" : string.Empty;
                                Color color = stats.AccuracyChange > 0 ? Colors.Messages.Good : Colors.Messages.Bad;
                                rightFindings.Add(new Tuple<Color, string>(color, string.Format("Accuracy {0}{1}", sign, stats.AccuracyChange)));
                            }
                            if (stats.DefenseChange != 0)
                            {
                                string sign = stats.DefenseChange > 0 ? "+" : string.Empty;
                                Color color = stats.DefenseChange > 0 ? Colors.Messages.Good : Colors.Messages.Bad;
                                rightFindings.Add(new Tuple<Color, string>(color, string.Format("Defense {0}{1}", sign, stats.DefenseChange)));
                            }
                            if (stats.HealthChange != 0)
                            {
                                string sign = stats.HealthChange > 0 ? "+" : string.Empty;
                                Color color = stats.HealthChange > 0 ? Colors.Messages.Good : Colors.Messages.Bad;
                                rightFindings.Add(new Tuple<Color, string>(color, string.Format("Maximum Health {0}{1}", sign, stats.HealthChange)));
                            }
                            if (stats.DieNumberChange != 0)
                            {
                                string sign = stats.DieNumberChange > 0 ? "+" : string.Empty;
                                Color color = stats.DieNumberChange > 0 ? Colors.Messages.Good : Colors.Messages.Bad;
                                rightFindings.Add(new Tuple<Color, string>(color, string.Format("Dice Number on Attack {0}{1}", sign, stats.DieNumberChange)));
                            }
                            if (stats.MinimumDamageChange != 0)
                            {
                                string sign = stats.MinimumDamageChange > 0 ? "+" : string.Empty;
                                Color color = stats.MinimumDamageChange > 0 ? Colors.Messages.Good : Colors.Messages.Bad;
                                rightFindings.Add(new Tuple<Color, string>(color, string.Format("Minimum Damage {0}{1}", sign, stats.MinimumDamageChange)));
                            }
                            if (stats.MaximumDamageChange != 0)
                            {
                                string sign = stats.MaximumDamageChange > 0 ? "+" : string.Empty;
                                Color color = stats.MaximumDamageChange > 0 ? Colors.Messages.Good : Colors.Messages.Bad;
                                rightFindings.Add(new Tuple<Color, string>(color, string.Format("Maximum Damage {0}{1}", sign, stats.MaximumDamageChange)));
                            }
                        }

                        if((selectedEntity.ComponentFlags & ComponentMasks.ObservableUsage) == ComponentMasks.ObservableUsage)
                        {
                            ItemFunctionsComponent funcs = spaceComponents.ItemFunctionsComponents[selectedEntity.Id];
                            rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                            rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Good, string.Format("This item has {0} uses left.", funcs.Uses)));
                            rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Bad, string.Format("This item loses {0} value per use.", funcs.CostToUse)));
                            if(funcs.Ranged)
                            {
                                rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, "This item is cast at a range."));
                            }
                            else
                            {
                                rightFindings.Add(new Tuple<Color, string>(Colors.Messages.Normal, "This item is used where you stand."));
                            }
                        }

                    }

                }

                //Draw sections
                //Left section
                spriteBatch.Draw(UITexture, new Rectangle(0, 0, panelWidth, (int)camera.DungeonViewport.Height), Color.Black * .5f);
                foreach (Tuple<Color, string> message in leftFindings)
                {
                    if (string.IsNullOrEmpty(message.Item2))
                    {
                        continue;
                    }
                    string text = MessageDisplaySystem.WordWrap(font, message.Item2, panelWidth - messageSpacing);

                    float textHeight = font.MeasureString(message.Item2).Y;
                    spriteBatch.DrawString(font, text, new Vector2(messageSpacing, messageSpacing + (messageLeft * messageSpacing)), message.Item1);
                    messageLeft += Regex.Matches(text, System.Environment.NewLine).Count;
                    messageLeft += 1;
                }
                //Right section
                if (observer.Observed.Count > 0)
                {
                    spriteBatch.Draw(UITexture, new Rectangle((int)camera.DungeonViewport.Bounds.Right - panelWidth, 0, panelWidth, (int)camera.DungeonViewport.Height), Color.Black * .5f);
                    foreach (Tuple<Color, string> message in rightFindings)
                    {
                        if (string.IsNullOrEmpty(message.Item2))
                        {
                            continue;
                        }
                        string text = MessageDisplaySystem.WordWrap(font, message.Item2, panelWidth - messageSpacing);

                        float textHeight = font.MeasureString(message.Item2).Y;
                        spriteBatch.DrawString(font, text, new Vector2((int)camera.DungeonViewport.Bounds.Right - panelWidth + messageSpacing, messageSpacing + (messageRight * messageSpacing)), message.Item1);
                        messageRight += Regex.Matches(text, System.Environment.NewLine).Count;
                        messageRight += 1;
                    }
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
