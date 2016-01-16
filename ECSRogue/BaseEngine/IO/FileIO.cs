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

        public static void SaveGameSettings(ref GameSettings gameSettings)
        {
            Directory.CreateDirectory(FileNames.SettingsDirectory);
            string jsonSettings = JsonConvert.SerializeObject(gameSettings);
            File.WriteAllText(FileNames.GameSettings, jsonSettings);
            gameSettings.HasChanges = true;
        }

        public static void ResetGameSettings()
        {
            Directory.CreateDirectory(FileNames.SettingsDirectory);
            if (!File.Exists(FileNames.DefaultGameSettings))
            {
                FileIO.CreateDefaultSettingsFile();
            }
            File.Copy(FileNames.DefaultGameSettings, FileNames.GameSettings, true);
        }

        private static void CreateDefaultSettingsFile()
        {
            GameSettings defaultSettings = new GameSettings()
            {
                HasChanges = false,
                Scale = new Vector2(2880, 1620),
                Resolution = new Vector2(1024, 576),
                ShowNormalMessages = true
            };
            string defaultSettingsJson = JsonConvert.SerializeObject(defaultSettings);
            Directory.CreateDirectory(FileNames.SettingsDirectory);
            File.WriteAllText(FileNames.DefaultGameSettings, defaultSettingsJson);
        }

        #endregion
    }
}
