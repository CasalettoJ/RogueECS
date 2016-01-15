﻿using ECSRogue.BaseEngine;
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
    public static class MessageDisplaySystem
    {
        public static void WriteMessages(StateSpaceComponents spaceComponents, SpriteBatch spriteBatch, Camera camera)
        {
            float opacity = 1.15f;
            float decrement = .15f;
            int messageNumber = 0;
            int messageSpacing = 20;
            SpriteFont font = null;
            //Draw message log
            foreach(Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_GAMEMESSAGE) == Component.COMPONENT_GAMEMESSAGE).Select(x => x.Id))
            {
                if(font == null)
                {
                    font = spaceComponents.GameMessageComponents[id].Font;
                }
                if(spaceComponents.GameMessageComponents[id].IndexBegin > 0)
                {
                    spriteBatch.DrawString(spaceComponents.GameMessageComponents[id].Font, Messages.ScrollingMessages, new Vector2(10, (int)10 + (messageNumber * messageSpacing)), Color.MediumVioletRed);
                    messageNumber += 1;
                }
                foreach(Tuple<Color,string> message in spaceComponents.GameMessageComponents[id].GameMessages.Reverse<Tuple<Color,string>>().Skip(spaceComponents.GameMessageComponents[id].IndexBegin))
                {
                    opacity -= decrement;
                    spriteBatch.DrawString(spaceComponents.GameMessageComponents[id].Font, message.Item2, new Vector2(10,(int)10 + (messageNumber * messageSpacing)), message.Item1 * opacity);
                    messageNumber += 1;
                }
                while (spaceComponents.GameMessageComponents[id].GameMessages.Count > spaceComponents.GameMessageComponents[id].MaxMessages)
                {
                    spaceComponents.GameMessageComponents[id].GameMessages.RemoveAt(0);
                }
                spriteBatch.DrawString(spaceComponents.GameMessageComponents[id].Font, spaceComponents.GameMessageComponents[id].GlobalMessage, new Vector2(10, camera.Bounds.Height - messageSpacing), spaceComponents.GameMessageComponents[id].GlobalColor);
            }

            messageNumber = 0;
            //Draw statistics
            foreach(Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Player) == ComponentMasks.Player).Select(x => x.Id))
            {
                List<string> statsToPrint = new List<string>();
                GameplayInfoComponent gameplayInfo = spaceComponents.GameplayInfoComponents[id];
                SkillLevelsComponent skills = spaceComponents.SkillLevelsComponents[id];
                statsToPrint.Add(string.Format("Steps: {0}", gameplayInfo.StepsTaken));
                statsToPrint.Add(string.Format("Kills: {0}", gameplayInfo.Kills));
                statsToPrint.Add("\n");
                statsToPrint.Add(string.Format("Health:  {0} / {1}", skills.CurrentHealth, skills.Health));
                statsToPrint.Add(string.Format("Wealth: {0}", skills.Wealth));
                statsToPrint.Add("\n");
                statsToPrint.Add(string.Format("ATK: {0}", skills.PhysicalAttack));
                statsToPrint.Add(string.Format("DEF: {0}", skills.PhysicalDefense));
                statsToPrint.Add(string.Format("MATK: {0}", skills.MagicAttack));
                statsToPrint.Add(string.Format("MDEF: {0}", skills.MagicDefense));

                if (font != null)
                {
                    foreach (string stat in statsToPrint)
                    {
                        Vector2 messageSize = font.MeasureString(stat);
                        spriteBatch.DrawString(font, stat, new Vector2(camera.Bounds.Width - messageSize.X - 10, 10 + (messageSpacing * messageNumber)), MessageColors.SpecialAction);
                        messageNumber += 1;
                    }
                }
            }
        }

        public static void GenerateRandomGameMessage(StateSpaceComponents spaceComponents, string[] messageList, Color color)
        {
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_GAMEMESSAGE) == Component.COMPONENT_GAMEMESSAGE).Select(x => x.Id))
            {
                spaceComponents.GameMessageComponents[id].GameMessages.Add(new Tuple<Color,string>(color,messageList[spaceComponents.random.Next(0, messageList.Count())]));
            }
        }

        public static void SetRandomGlobalMessage(StateSpaceComponents spaceComponents, string[] messageList)
        {
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_GAMEMESSAGE) == Component.COMPONENT_GAMEMESSAGE).Select(x => x.Id))
            {
                GameMessageComponent newMessage = spaceComponents.GameMessageComponents[id];
                newMessage.GlobalMessage = messageList[spaceComponents.random.Next(0, messageList.Count())];
                spaceComponents.GameMessageComponents[id] = newMessage;
            }
        }

        public static void ScrollMessage(KeyboardState prevKey, KeyboardState currentKey, StateSpaceComponents spaceComponents)
        {
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_GAMEMESSAGE) == Component.COMPONENT_GAMEMESSAGE).Select(x => x.Id))
            {
                GameMessageComponent messageComponent = spaceComponents.GameMessageComponents[id];
                if (currentKey.IsKeyDown(Keys.PageUp) && !prevKey.IsKeyDown(Keys.PageUp))
                {
                    messageComponent.IndexBegin = 0;
                }
                else if (currentKey.IsKeyDown(Keys.PageDown) && !prevKey.IsKeyDown(Keys.PageDown))
                {
                    messageComponent.IndexBegin = messageComponent.GameMessages.Count - 1;
                }
                else if (currentKey.IsKeyDown(Keys.Up) && !prevKey.IsKeyDown(Keys.Up) && messageComponent.IndexBegin < messageComponent.GameMessages.Count - 1)
                {
                    messageComponent.IndexBegin += 1;
                }
                else if (currentKey.IsKeyDown(Keys.Down) && !prevKey.IsKeyDown(Keys.Down) && messageComponent.IndexBegin > 0)
                {
                    messageComponent.IndexBegin -= 1;
                }
                spaceComponents.GameMessageComponents[id] = messageComponent;
            }
        }
    }
}