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
            "Twenty-five floors beneath here waits a covetous DRAGON.  Brave the depths, claim her life, and take her gold!"
        };

        public static readonly string[] CameraDetatchedMessage =
        {
            "Press \'R\' to reset the camera position."
        };

        public static readonly string ScrollingMessages = "[Scrolling Log: \'PageUp\': Top, \'PageDown\': Bottom, \'Arrows\': Scroll]";

        public static readonly string[] MeleeAttack =
        {
            "{0} swings at the {1}",
            "{0} applies fury to the {1}'s face",
            "{0} attempts brute force against {1}",
            "{0} uses a walking attack fearlessly at {1}"
        };
        public static readonly string[] PlayerIsAttackedMelee =
        {
            "{0} attacks you",
            "{0} tries to maul you",
            "{0} leaps toward you",
            "{0} strikes at you with a melee attack"
        };
        public static readonly string[] MeleeAttackPlayer =
        {
            "You attack the {0}",
            "You take a swing at the {0}",
            "You swipe at {0}",
            "You try to damage the {0}",
            "You slash viciously at the {0}"
        };

        public static readonly string[] DamageDealt =
        {
            " and hits it for {0} damage.",
            " and deals {0} damage to the poor thing.",
            " and it cries out, having taken {0} damage.",
            " and it seems {0} health weaker...",
            " dealing {0} damage to it!"
        };
        public static readonly string[] PlayerTookDamage =
        {
            " and you take {0} damage.",
            " and it hurts! You take {0} damage.",
            "! Ow. You lose {0} health.",
            " and hits you for {0} damage."
        };
        public static readonly string[] DamageDealtPlayer =
        {
            " and you hit it for {0} damage.",
            " and it takes {0} damage.",
            " and it loses {0} health!",
            " and successfully smash its face for {0}."
        };

        public static readonly string[] MeleeMissed =
        {
            " but the attack missed!",
            " and the creature dodges the attack.",
            " but the creature's defense protects it.",
            " and recoils in embarrassment as the blow fails to damage it.",
            " and misses expertly!",
            " and shyly calls out \"That was a practice attack!\" as it fails."
        };
        public static readonly string[] MeleePlayerDodged =
        {
            " and the attack misses you!",
            " but nothing happened.",
            " ... but it failed!",
            " and your defense protects you.",
            " but it fails to connect."
        };
        public static readonly string[] MeleeMissedPlayer =
        {
            " but you missed!",
            " but the creature's defense protected it.",
            " and dust yourself off after a miss.",
            " but it misses..",
            " but your blow only hits the air."
        };

        public static readonly string[] MeleeMissedALot =
        {
            " but stops, realizing the futility of it all.",
            " and, as always, misses the attack.",
            " the creature doesn't even flinch.",
            " while the monster stands there and laughs.",
            "; no one watching thinks this will work."
        };
        public static readonly string[] MeleePlayerDodgedALot =
        {
            " but you don't even notice.",
            " and you laugh at the attempt.",
            " but you easily dodge it again.",
            " and misses you. Again!"
        };
        public static readonly string[] MeleeMissedALotPlayer =
        {
            " but maybe it's pointless to try.",
            "; you should have given up.",
            " instead of saving yourself the embarrassment.",
            " while the monster stands there and laughs.",
            " and, as always, miss the attack.",
            " and you miss again.  YOU MISS AGAIN."
        };

        public static readonly string[] BrokenMissSpree =
        {
            " and against all odds deals {0} damage!",
            " and the cocky creature allows {0} damage to go through!",
            ", breaking impossible odds, landing {0} damage!!"
        };
        public static readonly string[] BrokenPlayerDodgeSpree =
        {
            " and you finally take {0} damage.",
            " and this time you lose {0} health! Ow!",
            " and hits you for {0} THIS time.",
            "! {0} damage taken! Don't get cocky..."
        };
        public static readonly string[] BrokenMissSpreePlayer =
        {
            " and you break through its defense! {0} damage!!",
            " and the cocky creature allows {0} damage to go through!",
            " and counter impossible odds! {0} damage!!",
            " and smash it for {0} damage. Finally!"
        };

    }
}
