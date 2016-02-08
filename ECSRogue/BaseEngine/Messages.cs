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

        public static readonly string[] GameEntranceMessages =
        {
            "There is a DRAGON at the bottom of this cave!  Find her!  Kill her!  Steal her treasure!",
            "The map that led you to these caves shows a giant gold hoard twenty-five floors down. ....And a massive DRAGON.",
            "Find the pile of gold and jewels beneath these caves.  You won't let a little DRAGON scare you, right?!",
            "A DRAGON lies below these caverns.  Kill her and take her treasure!",
            "You enter a maze of caves which a DRAGON calls home; slay her and claim her hoard as your own.",
            "Twenty-five floors beneath here waits a covetous DRAGON.  Brave the depths, claim her life, and take her gold!",
        };

        public static readonly string[] CameraDetatchedMessage =
        {
            "Press \'R\' to reset the camera position."
        };

        public static readonly string ScrollingMessages = "[Scrolling Log: \'PageUp\': Top, \'PageDown\': Bottom, \'Arrows\': Scroll]";

        public static readonly string[] AwokenBySight =
        {
            "The {0} awakens...",
            "{0} wakes up!",
            "{0} stirs from sleep due to a nearby presence.",
            "The {0} wakes up and starts wandering around."
        };

        public static readonly string[] FoundBySight =
        {
            "The {0} sees a target!",
            "{0} spots prey and starts to attack.",
            "{0} finds something to beat on.",
            "The {0} turns face up in attack mode!"
        };

        public static readonly string[] Flee =
        {
            "{0} fears for its life and runs!",
            "{0} flees the area.",
            "{0} attempts a strategic retreat.",
            "The {0} is afraid!"
        };
    }
}
