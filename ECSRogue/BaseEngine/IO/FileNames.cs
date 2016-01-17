using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSRogue.BaseEngine.IO
{
    public static class FileNames
    {
        #region Game Settings
        public const string SettingsDirectory = @"Settings/";
        public const string GameSettings = SettingsDirectory + "Config.json";
        public const string DefaultGameSettings = SettingsDirectory + "DefaultConfig.json";
        #endregion

        #region Dungeon Data
        public const string DungeonDirectory = @"Saves/";
        public const string DungeonSaveFile = DungeonDirectory + "Dungeon.sav";
        #endregion

        #region Game Data
        public const string GameDataDirectory = @"Saves/";
        public const string GameDataFile = GameDataDirectory + "Statistics.sav";
        #endregion
    }
}
