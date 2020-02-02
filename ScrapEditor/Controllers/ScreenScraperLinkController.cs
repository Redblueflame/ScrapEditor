using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Session;
using ScrapEditor.LoginLogic;
using ScrapEditor.ScrapLogic;

namespace ScrapEditor.Controllers
{
    [ApiController]
    public class ScreenScraperLinkController : ControllerBase
    {
        private readonly Database _db;
        private readonly ScrapManager _manager;
        private readonly ScreenScraperAPI _api;
        private readonly ILogger _logger;

        public ScreenScraperLinkController(IDatabase db, IScrapManager manager,
            ILogger<ScreenScraperLinkController> logger, IScreenScraperAPI api)
        {
            this._db = (Database) db;
            this._manager = (ScrapManager) manager;
            this._api = (ScreenScraperAPI) api;
            _logger = logger;
        }

        /// <summary>
        /// Manually link a game to a ScreenScraper one
        /// </summary>
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(Error), 404)]
        [HttpGet("/auto-link")]
        public async Task<IActionResult> AutoLink([FromQuery] int start, [FromQuery] int take)
        {
            _logger.LogInformation("Starting AutoLink...");
            // Get games in database
            using (var session = _db.store.OpenAsyncSession())
            {
                var query = await session
                    .Query<Game>()
                    .Where(x => x.ScreenScraperId == 0).OrderBy(x => x.ScrapEditorId, OrderingType.Long).Skip(start)
                    .Take(take).ToListAsync();
                Parallel.ForEach(query, new ParallelOptions {MaxDegreeOfParallelism = 10}, (game) =>
                {
                    // Check on screenscraper the name of the game
                    var id = _api.GetGameId(game.ScrapInfos[0].Names[0].Value,
                        game.ScrapInfos[0].Console, _api.GetDefaultUser());
                    if (id == long.MaxValue) return;
                    session.Advanced.Patch(
                        game,
                        x => x.ScreenScraperId, id);
                    Thread.Sleep(250);
                });
                await session.SaveChangesAsync();
                _logger.LogInformation("Finished AutoLink !");
            }

            return Ok(new {result = "Ok"});
        }

        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(Error), 404)]
        [HttpGet("/scrap-games")]
        public async Task<IActionResult> AutoScrap([FromQuery] int start, [FromQuery] int take)
        {
            // Query games
            using (var session = _db.store.OpenAsyncSession())
            {
                _logger.LogInformation("Starting scrapping...");
                _logger.LogInformation("Querying database for infos...");
                var query = await session.Query<BasicInfo>().OrderBy(x => x.ScrapEditorID, OrderingType.Long)
                    .Skip(start).Take(take).ToListAsync();
                _logger.LogInformation("Queried database for infos !");
                // Got every game
                foreach (var info in query)
                {
                    using (var localSession = _db.store.OpenAsyncSession())
                    {
                        var objects = await localSession.Query<Game>()
                            .Where(x => x.ScrapEditorId == (long) info.ScrapEditorID)
                            .ToListAsync();
                        if (objects.Count > 0)
                        {
                            // Update the existing game
                            var game = objects.First();
                            if (game.ScrapInfos.Any(x => x.Provider == info.Provider)) continue;
                            session.Advanced.Patch<Game, GameInfo>(game.Id, x => x.ScrapInfos,
                                infos => infos.Add(new[] {_manager.ScrapGame(info).Result}));
                        }
                        else
                        {
                            //Create a new game
                            var game = new Game {SavedInfo = new GameInfo(), ScrapEditorId = info.ScrapEditorID};
                            game.ScrapInfos.Add(await _manager.ScrapGame(info));
                            await session.StoreAsync(game);
                        }
                    }
                }

                await session.SaveChangesAsync();
                return Ok(new {result = "Ok"});
            }
        }

        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(Error), 404)]
        [HttpGet("/mass-upload/{nb}")]
        public async Task<IActionResult> MassUpload([FromRoute] int nb)
        {
            // Query games
            using (var session = _db.store.OpenAsyncSession())
            {
                var query = await session.Query<Game>().Where(x => x.ScreenScraperId != 0 && x.IsUploadedToScreenScraper == false)
                    .OrderBy(x => x.ScrapEditorId, OrderingType.Long)
                    .Take(nb).ToListAsync();
                foreach (var game in query)
                {
                    session.Advanced.IgnoreChangesFor(game);
                    //TODO: Add support for translators.
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
                    if (ssgame.Description == null || ssgame.Description.All(x => x.Region != "en"))
                    {
                        if (game.SavedInfo.Description.First().Value != "No short description is available for this item yet. ")
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
                    }

                    //Name
                    if (ssgame.Description == null || ssgame.Names.All(x =>
                            x.Region != _manager.GetRegionId(game.SavedInfo.Names.First().Region)))
                    {
                        _api.PublishText(
                            "name",
                            game.SavedInfo.Names.First().Value,
                            "EveryGameGoing via ScrapEditor",
                            game.ScreenScraperId,
                            _api.GetDefaultUser(),
                            _manager.GetRegionIdName(game.SavedInfo.Description.First().Region)
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
                    if (string.IsNullOrEmpty(ssgame.NbPlayers) && !string.IsNullOrEmpty(game.SavedInfo.NbPlayers))
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
                    if (ssgame.ReleaseDate == null || ssgame.ReleaseDate.All(x =>
                            x.Region != _manager.GetRegionId(game.SavedInfo.Names.First().Region)))
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
                    session.Advanced.Patch<Game, bool>(game.Id, x => x.IsUploadedToScreenScraper, true);
                }
                await session.SaveChangesAsync();
            }
            return Ok(new
            {
                result = "Ok"
            });
        }
    }
}