using ECSRogue.BaseEngine;
using ECSRogue.BaseEngine.IO.Objects;
using ECSRogue.ECS.Components;
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
        public static void WriteMessages(StateSpaceComponents spaceComponents, SpriteBatch spriteBatch, Camera camera, SpriteFont font)
        {
            float opacity = 1.15f;
            float decrement = .12f;
            int messageNumber = 0;
            int messageSpacing = 20;
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
                List<string> statsToPrint = new List<string>();
                GameplayInfoComponent gameplayInfo = spaceComponents.GameplayInfoComponent;

                statsToPrint.Add(string.Format("Floor {0}", gameplayInfo.FloorsReached));
                statsToPrint.Add(string.Format("Steps: {0}", gameplayInfo.StepsTaken));
                statsToPrint.Add(string.Format("Kills: {0}", gameplayInfo.Kills));
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_PLAYER) == Component.COMPONENT_PLAYER).Select(x => x.Id))
            {
                SkillLevelsComponent skills = spaceComponents.SkillLevelsComponents[id];
                statsToPrint.Add(System.Environment.NewLine);
                statsToPrint.Add(string.Format("Health:  {0} / {1}", skills.CurrentHealth, skills.Health));
                statsToPrint.Add(string.Format("Hunger:  {0} / {1}", skills.CurrentHunger, skills.Hunger));
                statsToPrint.Add(string.Format("Wealth: {0}", skills.Wealth));
                statsToPrint.Add(System.Environment.NewLine);
                statsToPrint.Add(string.Format("Power: {0}", skills.Power));
                statsToPrint.Add(string.Format("Accuracy: {0}", skills.Accuracy));
                statsToPrint.Add(string.Format("Defense: {0}", skills.Defense));
            }

            if (font != null)
                {
                    foreach (string stat in statsToPrint)
                    {
                        Vector2 messageSize = font.MeasureString(stat);
                        spriteBatch.DrawString(font, stat, new Vector2(camera.DungeonUIViewportLeft.X + 10, 10 + (messageSpacing * messageNumber)), Colors.Messages.Special);
                        messageNumber += 1;
                    }
                }
        }

        public static void GenerateRandomGameMessage(StateSpaceComponents spaceComponents, string[] messageList, Color color)
        {
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_GAMEMESSAGE) == Component.COMPONENT_GAMEMESSAGE).Select(x => x.Id))
            {
                spaceComponents.GameMessageComponent.GameMessages.Add(new Tuple<Color, string>(color, "[TURN " + spaceComponents.GameplayInfoComponent.StepsTaken + "] " + messageList[spaceComponents.random.Next(0, messageList.Count())]));
            }
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
