using System.Threading.Tasks;

namespace ScrapEditor
{
    public interface IScreenScraperAPI
    {
        /// <summary>
        /// Check if login is valid on ScreenScraper
        /// </summary>
        /// <param name="userName">Username to verify</param>
        /// <param name="password">Password to verify</param>
        /// <returns>True if valid, false if an error occured or invalid</returns>
        Task<bool> Login(string userName, string password);
    }
}