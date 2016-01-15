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
    public static class MessageDisplaySystem
    {
        public static void WriteMessages(StateSpaceComponents spaceComponents, SpriteBatch spriteBatch, Camera camera)
        {
            float opacity = 1.15f;
            float decrement = .15f;
            int messageNumber = 0;
            int messageSpacing = 20;
            foreach(Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_GAMEMESSAGE) == Component.COMPONENT_GAMEMESSAGE).Select(x => x.Id))
            {
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
