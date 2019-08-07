using System;
using System.IO;
using Newtonsoft.Json;

namespace ScrapEditor
{
    public class Configuration : IConfiguration
    {
        public string DevID { get; set; }
        public string DevPassword { get; set; }
        public string SoftName { get; set; }

        public static Configuration LoadConfiguration(string fileName)
        {
            if (File.Exists(fileName)) {
                using (var r = new StreamReader(fileName))
                {
                    var json = r.ReadToEnd();
                    return JsonConvert.DeserializeObject<Configuration>(json);
                }
            }
            Console.WriteLine("Config file not found...");
            var config = new Configuration
            {
                DevID = "PleaseReplaceMe",
                DevPassword = "PleaseReplaceMe",
                SoftName = "ScrapEditor"
            };
            using (StreamWriter file = File.CreateText(fileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, config);
            }

            return config;

        }
    }
}