using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using ScrapEditor.ScrapLogic;
using static ScrapEditor.Controllers.GamesListController;

namespace ScrapEditor.Controllers
{
    [Route("/game/")]
    [ApiController]
    public class GameController : Controller
    {
        private readonly Database _db;
        private readonly ScrapManager _manager;
        private readonly ScreenScraperAPI _api;

        public GameController(IDatabase db, IScrapManager manager, IScreenScraperAPI api)
        {
            _db = (Database) db;
            _manager = (ScrapManager) manager;
            _api = (ScreenScraperAPI) api;
        }
        /// <summary>
        /// Get every information about a game.
        /// </summary>
        /// <param name="id">The id of the game</param>
        /// <returns>The game</returns>
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(Error), 404)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGame([FromRoute] long id)
        {
            using (var session = _db.store.OpenAsyncSession())
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
                    game.ScrapInfos.Add(await _manager.ScrapGame(info));
                }
                await session.StoreAsync(game);
                await session.SaveChangesAsync();
                return Ok(game);
            }
        }

        /// <summary>
        /// Links a game from the database to a ScreenScraper one
        /// </summary>
        /// <param name="id">The internal Id of the Game</param>
        /// <param name="screenScraperId">ScreenScraper's ID for the game</param>
        /// <returns>A response telling if the game was successfully created</returns>
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(Error), 404)]
        [HttpGet("{id}/link")]
        public async Task<IActionResult> LinkGame([FromRoute] long id, [FromQuery] long screenScraperId)
        {
            using (var session = _db.store.OpenAsyncSession())
            {
                var game = await session
                    .Query<Game>()
                    .FirstOrDefaultAsync(x => x.ScrapEditorId == id);
                game = await session.LoadAsync<Game>(game.Id);
                if (game == null)
                    return NotFound(new Error
                    {
                        ErrorName = "NotFound",
                        ErrorMessage = "The document asked was not found."
                    });
                game.ScreenScraperId = screenScraperId;
                await session.SaveChangesAsync();
                return Ok(new
                {
                    result = "Ok"
                });
            }
        }

        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(Error), 404)]
        [HttpGet("{id}/upload")]
        public async Task<IActionResult> UploadGame([FromRoute] long id)
        {
            using (var session = _db.store.OpenAsyncSession())
            {
                //TODO: Add support for translators.
                var game = await session
                    .Query<Game>()
                    .FirstOrDefaultAsync(x => x.ScrapEditorId == id);
                if (game == null)
                    return NotFound(new Error
                    {
                        ErrorName = "NotFound",
                        ErrorMessage = "The document asked was not found."
                    });
                if (game.ScreenScraperId == 0)
                {
                    return UnprocessableEntity(new Error
                    {
                        ErrorName = "GameNotLinked",
                        ErrorMessage = "The game is not linked to a ScreenScraper Id."

                    });
                }
                // Save the choice
                if (game.SavedInfo.InternalId == 0)
                {
                    if (game.ScrapInfos.Count != 1)
                    {
                        return UnprocessableEntity(new Error
                        {
                            ErrorName = "MultipleChoices",
                            ErrorMessage =
                                "There are multiple Scraps, so it's impossible to choose one. Select one before executing this command."
                        });
                    }

                    game.SavedInfo = game.ScrapInfos.First();
                }
                // Downloading game data from ScreenScraper
                //TODO: Set the console ID for each console
                var ssgame = _api.GetScreenScraperInfo(game.ScreenScraperId, 91, _api.GetDefaultUser());
                //Check if changes needed

                //Description
                if (ssgame.Description != null && ssgame.Description.All(x => x.Region != "en"))
                {
                    _api.PublishText(
                        "description",
                        game.SavedInfo.Description.First().Value,
                        "EveryGameGoing via ScrapEditor",
                        game.ScreenScraperId,
                        _api.GetDefaultUser(),
                        lang: "en"
                        );
                }
                //Name
                if (ssgame.Names != null && ssgame.Names.All(x => x.Region != _manager.GetRegionId(game.SavedInfo.Names.First().Region)))
                {
                    _api.PublishText(
                        "name",
                        game.SavedInfo.Names.First().Value,
                        "EveryGameGoing via ScrapEditor",
                        game.ScreenScraperId,
                        _api.GetDefaultUser(),
                        _manager.GetRegionId(game.SavedInfo.Description.First().Region)
                    );
                }
                //Editor
                if (string.IsNullOrEmpty(ssgame.Editor))
                {
                    _api.PublishText(
                        "editeur",
                        game.SavedInfo.Editor,
                        "EveryGameGoing via ScrapEditor",
                        game.ScreenScraperId,
                        _api.GetDefaultUser()
                    );
                }
                //Developer
                if (string.IsNullOrEmpty(ssgame.Developer))
                {
                    _api.PublishText(
                        "developpeur",
                        game.SavedInfo.Developer,
                        "EveryGameGoing via ScrapEditor",
                        game.ScreenScraperId,
                        _api.GetDefaultUser()
                    );
                }
                //Players
                if (string.IsNullOrEmpty(ssgame.Developer) && !string.IsNullOrEmpty(game.SavedInfo.NbPlayers))
                {
                    _api.PublishText(
                        "players",
                        game.SavedInfo.NbPlayers,
                        "EveryGameGoing via ScrapEditor",
                        game.ScreenScraperId,
                        _api.GetDefaultUser()
                    );
                }
                // Release Date
                if (ssgame.ReleaseDate == null || ssgame.ReleaseDate.All(x => x.Region != _manager.GetRegionId(game.SavedInfo.Names.First().Region)))
                {
                    _api.PublishText(
                        "datessortie",
                        game.SavedInfo.ReleaseDate.First().Value,
                        "EveryGameGoing via ScrapEditor",
                        game.ScreenScraperId,
                        _api.GetDefaultUser(),
                        _manager.GetRegionId(game.SavedInfo.Description.First().Region)
                    );
                }
                game.IsUploadedToScreenScraper = true;
                await session.SaveChangesAsync();
                return Ok(new
                {
                    result = "Ok"
                });
            }
        }
        /// <summary>
        /// Updates an existing game
        /// </summary>
        /// <param name="info">The info of the game</param>
        /// <param name="id">The ID of the game</param>
        /// <returns></returns>
        [HttpPost("/{id}")]
        public async Task<IActionResult> SaveGame([FromBody] GameInfo info, [FromRoute] long id)
        {
            using (var session = _db.store.OpenAsyncSession())
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
                await session.SaveChangesAsync();
                return Ok("OK");
            }
            
        }
    }
}