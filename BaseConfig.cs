using System;
using System.CodeDom;
using System.IO;
using System.Text.Json;

namespace demonviglu.config
{
    public class BaseConfig
    {
        //public static string SaveFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string SaveFolderPath = ".\\";
        public static string SaveFolderName = "DemonConfig";

        private static readonly JsonSerializerOptions JSOP = new() { WriteIndented = true };

        public BaseConfig()
        {

        }

        public void Save<T>() where T : BaseConfig
        {
            string saveFullPath = Path.Combine(SaveFolderPath, SaveFolderName, GetType().Name);

            string data = JsonSerializer.Serialize((T)this, JSOP);

            if (!Directory.Exists(Path.Combine(SaveFolderPath, SaveFolderName)))
            {
                Directory.CreateDirectory(Path.Combine(SaveFolderPath, SaveFolderName));
            }

            File.WriteAllText(saveFullPath, data);
        }

        public void Save<T>(string filePath) where T : BaseConfig
        {
            string saveFullPath = filePath;

            string data = JsonSerializer.Serialize((T)this, JSOP);

            if (!Directory.Exists(Path.Combine(SaveFolderPath, SaveFolderName)))
            {
                Directory.CreateDirectory(Path.Combine(SaveFolderPath, SaveFolderName));
            }

            File.WriteAllText(saveFullPath, data);
        }

        public static T Load<T>() where T : BaseConfig, new()
        {
            if (File.Exists(Path.Combine(SaveFolderPath, SaveFolderName, typeof(T).Name)))
            {
                string data = File.ReadAllText(Path.Combine(SaveFolderPath, SaveFolderName, typeof(T).Name));

                return JsonSerializer.Deserialize<T>(data) ?? new();
            }
            else
            {
                return new T();
            }
        }

        public string GetConfigFullPath()
        {
            return Path.Combine(SaveFolderPath, SaveFolderName, GetType().Name);
        }
    }
}