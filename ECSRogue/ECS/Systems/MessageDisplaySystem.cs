using ECSRogue.BaseEngine;
using ECSRogue.ECS.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
                foreach(Tuple<Color,string> message in spaceComponents.GameMessageComponents[id].GameMessages.Reverse<Tuple<Color,string>>())
                {
                    opacity -= decrement;
                    spriteBatch.DrawString(spaceComponents.GameMessageComponents[id].Font, message.Item2, new Vector2(10,(int)10 + (messageNumber * messageSpacing)), message.Item1 * opacity);
                    messageNumber += 1;
                }
                while (spaceComponents.GameMessageComponents[id].GameMessages.Count > 8)
                {
                    spaceComponents.GameMessageComponents[id].GameMessages.RemoveAt(0);
                }
                spriteBatch.DrawString(spaceComponents.GameMessageComponents[id].Font, spaceComponents.GameMessageComponents[id].GlobalMessage, new Vector2(10, camera.Bounds.Height - messageSpacing), spaceComponents.GameMessageComponents[id].GlobalColor);
            }
        }

        public static void GenerateRandomGameMessage(Random random, StateSpaceComponents spaceComponents, string[] messageList, Color color)
        {
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_GAMEMESSAGE) == Component.COMPONENT_GAMEMESSAGE).Select(x => x.Id))
            {
                spaceComponents.GameMessageComponents[id].GameMessages.Add(new Tuple<Color,string>(color,messageList[random.Next(0, messageList.Count())]));
            }
        }

        public static void SetRandomGlobalMessage(Random random, StateSpaceComponents spaceComponents, string[] messageList)
        {
            foreach (Guid id in spaceComponents.Entities.Where(x => (x.ComponentFlags & Component.COMPONENT_GAMEMESSAGE) == Component.COMPONENT_GAMEMESSAGE).Select(x => x.Id))
            {
                GameMessageComponent newMessage = spaceComponents.GameMessageComponents[id];
                newMessage.GlobalMessage = messageList[random.Next(0, messageList.Count())];
                spaceComponents.GameMessageComponents[id] = newMessage;
            }
        }
    }
}
