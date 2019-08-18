using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScrapEditor.ScrapLogic
{
    public interface IScrapProvider
    {
        /// <summary>
        /// Get a list of games. Used to creates indexes of games.
        /// </summary>
        /// <returns>A list of all the games present on the platform</returns>
        Task<List<BasicInfo>> GetGamesList();
        /// <summary>
        /// Get more information about a game
        /// </summary>
        /// <param name="info">THe basic info about the game.</param>
        /// <returns>All informations present in the platform</returns>
        Task<GameInfo> GetGameInfo(BasicInfo info);
        /// <summary>
        /// Get a list of all supported systems in the platform.
        /// </summary>
        /// <returns></returns>
        List<string> GetSupportedSystems();
        /// <summary>
        /// Get the name of the Provider. Can't be the same as another provider registered.
        /// </summary>
        /// <returns>The name of the provider.</returns>
        string GetName();
    }

    public class BasicInfo
    {
        public ulong ScrapEditorID { get; set; }
        public string InternalId { get; set; }
        public string Link { get; set; }
        public string Name { get; set; }
        public string Console { get; set; }
        public string Id { get; set; }
        public string Provider { get; set; }
        
    }
}