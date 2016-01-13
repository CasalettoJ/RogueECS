using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.BaseEngine
{
    public static class Messages
    {
        public static readonly string[] WallCollisionMessages =
        {
            "You run face-first into a wall.",
            "You stub your toe against a wall.  Ow!",
            "Distracted, you mistake a wall for the floor.",
            "You hit a wall.  How embarrassing.",
            "You attempt to occupy the same space as a wall. Your attempt is not successful.",
            "You hear distant laughter as you walk straight into a wall."
        };

        public static readonly string[] CaveEntranceMessages =
        {
            "You enter the DANK caves.",
            "You descend into a complex cavern.",
            "You delve deeper into the dungeon, until you are surrounded by the stale air of an ancient cavern.",
            "Your eyes adjust to the darkness and you find yourself surrounded by tunnels."
        };

        public static readonly string[] CameraDetatchedMessage =
        {
            "Press \'R\' to reset the camera position."
        };
    }
}
