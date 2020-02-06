using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using ScrapEditor.ScrapLogic;
using Sparrow;

#pragma warning disable 1998

namespace ScrapEditor.Controllers
{
    [ApiController]
    public partial class GamesListController : Controller
    {
        private Database db;
        private IScrapManager _manager;
        public GamesListController(IDatabase db, IScrapManager manager)
        {
            _manager = manager;
            this.db = (Database) db;
        }
        /// <summary>
        /// Get the list of games imported to the platform.
        /// </summary>
        /// <param name="start">Index of the first element to take.</param>
        /// <param name="nb">Number of elements to take. Max is 50.</param>
        /// <returns>A list of <see cref="BasicInfo"/> </returns>
        /// <response code="200">Returns elements asked</response>
        /// <response code="400">Asked too many elements, 50 is the maximum</response>
        [HttpGet("/list")]
        [ProducesResponseType(typeof(List<BasicInfo>), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        public async Task<IActionResult> GetList([FromQuery] int start = 0, [FromQuery] int nb = 25)
        {
            if (nb >= 50)
            {
                return BadRequest(new Error
                {
                    ErrorName = "MaxElemByPage",
                    ErrorMessage = "Too big request ! The maximum is 50."
                });
            } 
            using (var session = db.store.OpenSession()) // Open a session for a default 'Database'
            {
                var values = session
                    .Query<BasicInfo>()
                    .Skip(start).Take(nb)
                    .ToList();
                return Ok(
                    values
                    );
            }
        }
        /// <summary>
        /// Get the list of games in the console imported in the platform.
        /// </summary>
        /// <param name="console">The name of the console to search</param>
        /// <param name="start">Index of the first element to take.</param>
        /// <param name="nb">Number of elements to take. Max is 50.</param>
        /// <returns>A list of <see cref="BasicInfo"/> </returns>
        /// <response code="200">Returns elements asked</response>
        /// <response code="400">Asked too many elements, 50 is the maximum</response>
        /// <response code="404">Console not found</response>
        [HttpGet("/list/{console}")]
        [ProducesResponseType(typeof(List<BasicInfo>), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 404)]
        public async Task<IActionResult> GetListByConsole([FromRoute] string console, [FromQuery] int start = 0,
            [FromQuery] int nb = 25)
        {
            if (nb >= 50)
            {
                return BadRequest(new
                {
                    error = new
                    {
                        Id="MaxElemByPage",
                        Text="Asking for more than 50 elements is forbidden.",
                    },
                    result = new {}
                });
            }
            if (!_manager.GetConsoles().Select(x=> x.ToLower()).Contains(console.ToLower()))
            {
                return NotFound(new
                {
                    error = new
                    {
                        Id="ConsoleNotFound",
                        Text="No console with this name is found.",
                    },
                    result = new {}
                });
            }
            using (var session = db.store.OpenSession()) // Open a session for a default 'Database'
            {
                var values = session
                    .Query<BasicInfo>()
                    .Where(x => string.Equals(x.Console, console, StringComparison.CurrentCultureIgnoreCase))
                    .Skip(start)
                    .Take(nb)
                    .ToList();
                Console.WriteLine(values.Count);
                return Ok(
                    values
                );
            }
        }
        /// <summary>
        /// Search in the database any game.
        /// </summary>
        /// <param name="name">The name to search</param>
        /// <param name="start">The first element to take</param>
        /// <param name="nb">The number of elements to take max is 50.</param>
        /// <returns>An array of game matching the search</returns>
        /// <response code="200">Returns elements asked</response>
        /// <response code="400">Asked too many elements, 50 is the maximum</response>
        [HttpGet("/search")]
        [ProducesResponseType(typeof(List<BasicInfo>), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        public async Task<IActionResult> GetListByName([FromQuery] string name,[FromQuery] int start = 0, [FromQuery] int nb = 25)
        {
            if (nb >= 50)
            {
                return BadRequest(new
                {
                    error = new
                    {
                        Id="MaxElemByPage",
                        Text="Asking for more than 50 elements is forbidden.",
                    },
                    result = new {}
                });
            }
            using (var session = db.store.OpenSession()) // Open a session for a default 'Database'
            {
                var values = session
                    .Query<BasicInfo>()
                    .Search(x => x.Name, name)
                    .Skip(start)
                    .Take(nb)
                    .ToList();
                Console.WriteLine(values.Count);
                return Ok(
                    values
                );
            }
        }
        /*
        [HttpGet("/import/{id}")]
        [ProducesResponseType(typeof(ResultGame), 200)]
        [ProducesResponseType(typeof(Error), 404)]
        public async Task<IActionResult> ImportGame([FromRoute] int id)
        {

        }
        */
    }

    public class ResultGame
    {
        public bool IsOk { get; set; }
    }
}