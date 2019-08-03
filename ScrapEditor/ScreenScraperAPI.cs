using System.Net;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

namespace ScrapEditor
{
    public class ScreenScraperAPI
    {
        private Configuration config;
        public ScreenScraperAPI(Configuration config)
        {
            this.config = config;
        }
        /// <summary>
        /// Check if login is valid on ScreenScraper
        /// </summary>
        /// <param name="userName">Username to verify</param>
        /// <param name="password">Password to verify</param>
        /// <returns>True if valid, false if an error occured or invalid</returns>
        public async Task<bool> Login(string userName, string password)
        {
            var person = await "https://www.screenscraper.fr/api2/ssuserInfos.php"
                .SetQueryParam("devid", config.DevID)
                .SetQueryParam("devpassword", config.DevPassword)
                .SetQueryParam("softname", config.SoftName)
                .SetQueryParam("output", "json")
                .SetQueryParam("ssid", userName)
                .SetQueryParam("sspassword", password)
                .GetAsync();
            return person.StatusCode == HttpStatusCode.OK;
        }
    }
}