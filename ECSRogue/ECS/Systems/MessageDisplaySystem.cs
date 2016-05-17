using ECSRogue.BaseEngine;
using ECSRogue.BaseEngine.IO.Objects;
using ECSRogue.ECS.Components;
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
    public static class MessageDisplaySystem
    {
        public static void WriteMessages(StateSpaceComponents spaceComponents, SpriteBatch spriteBatch, Camera camera, SpriteFont font, DungeonTile[,] dungeonGrid)
        {
            float opacity = 1.15f;
            float decrement = .09f;
            int messageNumber = 0;
            int messageSpacing = (int)font.MeasureString("g").Y + 1; ;
            //Draw message log
            if(spaceComponents.GameMessageComponent.IndexBegin > 0)
            {
                float textHeight = font.MeasureString(Messages.ScrollingMessages).Y;
                spriteBatch.DrawString(font, Messages.ScrollingMessages, new Vector2(10, (int)camera.DungeonUIViewport.Bounds.Bottom -(int)textHeight - 10 - (messageNumber * messageSpacing)), Color.MediumVioletRed);
                messageNumber += 1;
            }
            foreach(Tuple<Color,string> message in spaceComponents.GameMessageComponent.GameMessages.Reverse<Tuple<Color,string>>().Skip(spaceComponents.GameMessageComponent.IndexBegin))
            {
                if(opacity < 0)
                {
                    break;
                }
                opacity -= decrement;
                string text = MessageDisplaySystem.WordWrap(font, message.Item2, camera.DungeonUIViewport.Width-20);

                float textHeight = font.MeasureString(text).Y;
                spriteBatch.DrawString(font,text, new Vector2(10, (int)camera.DungeonUIViewport.Bounds.Bottom - (int)textHeight - 10 - (messageNumber * messageSpacing)), message.Item1 * opacity);
                messageNumber += Regex.Matches(text, System.Environment.NewLine).Count;
                messageNumber += 1;
            }
            while (spaceComponents.GameMessageComponent.GameMessages.Count > spaceComponents.GameMessageComponent.MaxMessages)
            {
                spaceComponents.GameMessageComponent.GameMessages.RemoveAt(0);
            }
            spriteBatch.DrawString(font, spaceComponents.GameMessageComponent.GlobalMessage, new Vector2(10, camera.Bounds.Height - messageSpacing), spaceComponents.GameMessageComponent.GlobalColor);

            messageNumber = 0;
            //Draw statistics
                List<Tuple<Color,string>> statsToPrint = new List<Tuple<Color,string>>();
                GameplayInfoComponent gameplayInfo = spaceComponents.GameplayInfoComponent;

                statsToPrint.Add(new Tuple<Color, string>( Colors.Messages.Normal, string.Format("Floor {0}", gameplayInfo.FloorsReached)));
                statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("Steps: {0}", gameplayInfo.StepsTaken)));
                statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("Kills: {0}", gameplayInfo.Kills)));
            Entity player = spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).FirstOrDefault();
            if (player != null)
            {
                SkillLevelsComponent skills = InventorySystem.ApplyStatModifications(spaceComponents, player.Id, spaceComponents.SkillLevelsComponents[player.Id]);
                InventoryComponent inventory = spaceComponents.InventoryComponents[player.Id];
                statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("Health:  {0} / {1}", skills.CurrentHealth, skills.Health)));
                statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("Wealth: {0}", skills.Wealth)));
                statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("Damage: {0}-{1}", skills.MinimumDamage, skills.MaximumDamage)));
                statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("Accuracy: {0}", skills.Accuracy)));
                statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, string.Format("Defense: {0}", skills.Defense)));
                statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                //Status Effects:
                Statuses statuses = StatusSystem.GetStatusEffectsOfEntity(spaceComponents, player.Id, dungeonGrid);
                if(statuses == Statuses.NONE)
                {
                    statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, Messages.StatusMessages.Normal));
                    statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                }
                //If there are status effects on the player..
                else
                {
                    if((statuses & Statuses.BURNING) == Statuses.BURNING)
                    {
                        BurningComponent burning = spaceComponents.BurningComponents[player.Id];
                        statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Bad, string.Format(Messages.StatusMessages.Burning, burning.MinDamage, burning.MaxDamage, burning.TurnsLeft)));
                    }
                    if((statuses & Statuses.UNDERWATER) == Statuses.UNDERWATER)
                    {
                        statsToPrint.Add(new Tuple<Color, string>(Colors.Caves.WaterInRange, Messages.StatusMessages.Underwater));
                    }
                    if((statuses & Statuses.HEALTHREGEN) == Statuses.HEALTHREGEN)
                    {
                        HealthRegenerationComponent healthRegen = spaceComponents.HealthRegenerationComponents[player.Id];
                        statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Good, string.Format(Messages.StatusMessages.HealthRegen, healthRegen.HealthRegain, healthRegen.RegenerateTurnRate)));
                    }
                    statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                }

                statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Special, string.Format(Messages.InventoryArtifacts + " ({0}/{1})", inventory.Artifacts.Count, inventory.MaxArtifacts)));
                foreach (Guid id in inventory.Artifacts)
                {
                    NameComponent name = spaceComponents.NameComponents[id];
                    ArtifactStatsComponent artifactStats = spaceComponents.ArtifactStatsComponents[id];
                    statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.LootPickup, string.Format("{0} Lv{1}", name.Name, artifactStats.UpgradeLevel)));
                }

                statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Special, string.Format(Messages.InventoryConsumables + " ({0}/{1})", inventory.Consumables.Count, inventory.MaxConsumables)));
                if(inventory.Consumables.Count > 0)
                {
                    statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                    NameComponent name = spaceComponents.NameComponents[inventory.Consumables[0]];
                    ItemFunctionsComponent funcs = spaceComponents.ItemFunctionsComponents[inventory.Consumables[0]];
                    statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.LootPickup, string.Format("{0}({1})", name.Name, funcs.Uses)));
                    statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, "Q - Use"));
                    statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, "X - Throw"));
                    statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, System.Environment.NewLine));
                    if (inventory.Consumables.Count > 1)
                    {
                        name = spaceComponents.NameComponents[inventory.Consumables[1]];
                        funcs = spaceComponents.ItemFunctionsComponents[inventory.Consumables[1]];
                        statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.LootPickup, string.Format("{0}({1})", name.Name, funcs.Uses)));
                        statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, "E - Use"));
                        statsToPrint.Add(new Tuple<Color, string>(Colors.Messages.Normal, "C - Throw"));
                    }
                }
            }

            if (font != null)
                {
                    foreach (Tuple<Color,string> stat in statsToPrint)
                    {
                        string text = MessageDisplaySystem.WordWrap(font, stat.Item2, camera.DungeonUIViewportLeft.Width - messageSpacing);
                        Vector2 messageSize = font.MeasureString(stat.Item2);
                        spriteBatch.DrawString(font, text, new Vector2(camera.DungeonUIViewportLeft.X + 10, 10 + (messageSpacing * messageNumber)), stat.Item1);
                        messageNumber += 1;
                        messageNumber += Regex.Matches(text, System.Environment.NewLine).Count;
                    }
                }
        }

        public static void GenerateRandomGameMessage(StateSpaceComponents spaceComponents, string[] messageList, Color color)
        {
                spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(color, "[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + messageList[spaceComponents.random.Next(0, messageList.Count())]));
        }

        public static void SetRandomGlobalMessage(StateSpaceComponents spaceComponents, string[] messageList)
        {
            GameMessageComponent newMessage = spaceComponents.GameMessageComponent;
            newMessage.GlobalMessage = messageList[spaceComponents.random.Next(0, messageList.Count())];
            spaceComponents.GameMessageComponent = newMessage;
        }

        public static void ScrollMessage(KeyboardState prevKey, KeyboardState currentKey, StateSpaceComponents spaceComponents)
        {
            GameMessageComponent messageComponent = spaceComponents.GameMessageComponent;
            if (currentKey.IsKeyDown(Keys.PageUp) && !prevKey.IsKeyDown(Keys.PageUp))
            {
                messageComponent.IndexBegin = 0;
            }
            else if (currentKey.IsKeyDown(Keys.PageDown) && !prevKey.IsKeyDown(Keys.PageDown))
            {
                messageComponent.IndexBegin = messageComponent.GameMessages.Count - 1;
            }
            else if (currentKey.IsKeyDown(Keys.Down) && !prevKey.IsKeyDown(Keys.Down) && messageComponent.IndexBegin < messageComponent.GameMessages.Count - 1)
            {
                messageComponent.IndexBegin += 1;
            }
            else if (currentKey.IsKeyDown(Keys.Up) && !prevKey.IsKeyDown(Keys.Up) && messageComponent.IndexBegin > 0)
            {
                messageComponent.IndexBegin -= 1;
            }
            spaceComponents.GameMessageComponent = messageComponent;
        }

        public static string WordWrap(SpriteFont font, string text, float maxLineWidth)
        {
            if(string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            string[] splitMessage = text.Split(' ');
            StringBuilder sb = new StringBuilder();
            float lineWidth = 0f;
            float spaceWidth = font.MeasureString(" ").X;
            foreach (string word in splitMessage)
            {
                Vector2 size = font.MeasureString(word);
                if(lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append(System.Environment.NewLine + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }
            return sb.ToString();
        }
    }
}
