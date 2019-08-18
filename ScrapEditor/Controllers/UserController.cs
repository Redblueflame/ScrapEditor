using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ScrapEditor.LoginLogic;
using static ScrapEditor.Controllers.GamesListController;

namespace ScrapEditor.Controllers
{
    [Route("/user/")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private ILoginLogic _login;

        public UserController(ILoginLogic logic)
        {
            _login = logic;
        }
        /// <summary>
        /// Sign in the user to the in memory database.
        /// </summary>
        /// <param name="username">User's username</param>
        /// <param name="password">User's password</param>
        /// <returns>A <see cref="Controllers.Login"/> object.</returns>
        /// <response code="200">Successfully executed request.</response>
        /// <response code="500">Error while executing request.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(Login), 200)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> Login([FromQuery] string username, [FromQuery] string password)
        {
            var status = await _login.LoginUser(username, password);
            if (status == null)
            {
                return NotFound(new Error
                {
                    ErrorName = "LoginNotValid",
                    ErrorMessage = "The username and password provided does not correspond..."
                });
            }

            return Ok(new Login
            {
                Token = status,
                Username = username
            }
            );
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <response code="200">Successfully signed out user.</response>
        /// <response code="404">Token not found.</response>
        [HttpGet("disconnect")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(Error), 404)]
        public async Task<IActionResult> Disconnect([FromQuery] string token)
        {
            var result = await _login.DisconnectUser(token);
            if (result)
            {
                return Ok("OK");
            }

            return NotFound(new Error
            {
                ErrorName = "NotFound",
                ErrorMessage = "The user requested was not found."
            });
        }
    }
}
