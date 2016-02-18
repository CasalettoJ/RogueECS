using ECSRogue.BaseEngine.IO.Objects;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ECSRogue.BaseEngine.IO
{
    public static class FileIO
    {
        #region Game Settings
        public static void LoadGameSettings(ref GameSettings gameSettings)
        {
            try {
                Directory.CreateDirectory(FileNames.SettingsDirectory);
                if (!File.Exists(FileNames.GameSettings))
                {
                    FileIO.ResetGameSettings();
                    gameSettings.HasChanges = true;
                }
                using (StreamReader fs = File.OpenText(FileNames.GameSettings))
                {
                    JsonSerializer js = new JsonSerializer();
                    gameSettings = (GameSettings)js.Deserialize(fs, typeof(GameSettings));
                }
            }
            catch
            {
                FileIO.ResetGameSettings();
                FileIO.LoadGameSettings(ref gameSettings);
            }
        }

        public static void SaveGameSettings(ref GameSettings gameSettings)
        {
            Directory.CreateDirectory(FileNames.SettingsDirectory);
            string jsonSettings = JsonConvert.SerializeObject(gameSettings);
            File.WriteAllText(FileNames.GameSettings, jsonSettings);
            gameSettings.HasChanges = true;
        }

        public static void ResetGameSettings()
        {
            try
            {
                Directory.CreateDirectory(FileNames.SettingsDirectory);
                if (!File.Exists(FileNames.DefaultGameSettings))
                {
                    FileIO.CreateDefaultSettingsFile();
                }
                File.Copy(FileNames.DefaultGameSettings, FileNames.GameSettings, true);
            }
            catch
            {
                FileIO.CreateDefaultSettingsFile();
                FileIO.ResetGameSettings();
            }
        }

        private static void CreateDefaultSettingsFile()
        {
            GameSettings defaultSettings = new GameSettings()
            {
                HasChanges = false,
                Scale = .5f,
                Resolution = new Vector2(1024, 768),
                ShowGlow = true
            };
            string defaultSettingsJson = JsonConvert.SerializeObject(defaultSettings);
            Directory.CreateDirectory(FileNames.SettingsDirectory);
            File.WriteAllText(FileNames.DefaultGameSettings, defaultSettingsJson);
        }

        #endregion

        #region Dungeon Data
        public static void LoadDungeonData(ref DungeonInfo data)
        {
            Directory.CreateDirectory(FileNames.DungeonDirectory);
            if(File.Exists(FileNames.DungeonSaveFile))
            {
                try
                {
                    using (StreamReader fs = File.OpenText(FileNames.DungeonSaveFile))
                    {
                        JsonSerializer js = new JsonSerializer();
                        data = (DungeonInfo)js.Deserialize(fs, typeof(DungeonInfo));
                    }
                }
                catch
                {
                    data = null;
                }
            }
            else
            {
                data = null;
            }
        }

        public static void SaveDungeonData(DungeonInfo data)
        {
            Directory.CreateDirectory(FileNames.DungeonDirectory);
            string jsonData = JsonConvert.SerializeObject(data);
            File.WriteAllText(FileNames.DungeonSaveFile, jsonData);
        }
        #endregion
    }
}
