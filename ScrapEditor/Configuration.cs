using System.IO;
using Newtonsoft.Json;

namespace ScrapEditor
{
    public class Configuration
    {
        public string DevID { get; }
        public string DevPassword { get; }
        public string SoftName { get; }

        public static Configuration LoadConfiguration(string fileName)
        {
            using (var r = new StreamReader(fileName))
            {
                var json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<Configuration>(json);
            }
        }
    }
}