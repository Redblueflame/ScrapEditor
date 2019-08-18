using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using ScrapEditor.ScrapLogic;
using static ScrapEditor.Controllers.GamesListController;

namespace ScrapEditor.Controllers
{
    [ApiController]
    public class GameController : Controller
    {
        private Database db;
        private ScrapManager manager;

        public GameController(IDatabase db, IScrapManager manager)
        {
            this.db = (Database) db;
            this.manager = (ScrapManager) manager;
        }
        /// <summary>
        /// Get every information about a game.
        /// </summary>
        /// <param name="id">The id of the game</param>
        /// <returns>The game</returns>
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(Error), 404)]
        [HttpGet("/game")]
        public async Task<IActionResult> GetGame([FromQuery] ulong id)
        {
            using (var session = db.store.OpenAsyncSession())
            {
                var objects = await session.Query<Game>()
                    .Where(x => x.ScrapEditorId == id)
                    .ToListAsync();
                if (objects.Count > 0) return Ok(objects.First());

                //Check if ID has an associated scrap provider
                var scrapInfo = await session.Query<BasicInfo>()
                    .Where(x => x.ScrapEditorID == id)
                    .ToListAsync();
                if (scrapInfo.Count <= 0)
                {
                    return NotFound(new Error
                    {
                        ErrorName = "NotFound",
                        ErrorMessage = "No game was found with the provided id."
                    });
                }

                // Create new Game
                var game = new Game {SavedInfo = new GameInfo(), ScrapEditorId = id};
                foreach (var info in scrapInfo)
                {
                    game.ScrapInfos.Add(await manager.ScrapGame(info));
                }
                await session.StoreAsync(game);
                await session.SaveChangesAsync();
                return Ok(game);
            }
        }

        [HttpPost("/game")]
        public async Task<IActionResult> SaveGame([FromBody] GameInfo info, [FromQuery] ulong id)
        {
            using (var session = db.store.OpenAsyncSession())
            {
                var game = await session
                    .Query<Game>()
                    .FirstOrDefaultAsync(x => x.ScrapEditorId == id);
                if (game == null)
                    return NotFound(new Error
                    {
                        ErrorName = "NotFound",
                        ErrorMessage = "The document asked was not found."
                    });
                game.SavedInfo = info;
                return Ok("OK");
            }
            
        }
    }
}