using System;
using System.IO;
using Newtonsoft.Json;

namespace ScrapEditor
{
    public class ConfigurationFile : IConfigurationFile
    {
        public string DevID { get; set; }
        public string DevPassword { get; set; }
        public string SoftName { get; set; }
        public string DBLink { get; set; }
        public string DBCertPath { get; set; }
        public string DBName { get; set; }

        public static ConfigurationFile LoadConfiguration(string fileName)
        {
            if (File.Exists(fileName)) {
                using (var r = new StreamReader(fileName))
                {
                    var json = r.ReadToEnd();
                    return JsonConvert.DeserializeObject<ConfigurationFile>(json);
                }
            }
            Console.WriteLine("Config file not found...");
            var config = new ConfigurationFile
            {
                DevID = "PleaseReplaceMe",
                DevPassword = "PleaseReplaceMe",
                SoftName = "ScrapEditor",
                DBLink = "http://live-test.ravendb.net",
                DBCertPath = "none",
                DBName = "ScrapEditor-Dev"
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