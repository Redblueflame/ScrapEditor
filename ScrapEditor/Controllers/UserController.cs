using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using ScrapEditor.LoginLogic;

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
        /// <returns>A <see cref="LoginResult"/> object.</returns>
        /// <response code="200">Successfully executed request.</response>
        /// <response code="500">Error while executing request.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResult), 200)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<LoginResult> Login([FromQuery] string username, [FromQuery] string password)
        {
            var status = await _login.LoginUser(username, password);
            if (status == null)
            {
                return new LoginResult
                {
                    IsSuccess = false
                };
            }

            return new LoginResult
            {
                IsSuccess = true,
                Token = status,
                Username = username
            };
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
        [ProducesResponseType(typeof(void), 404)]
        public async Task<IActionResult> Disconnect([FromQuery] string token)
        {
            var result = await _login.DisconnectUser(token);
            if (result)
            {
                return Ok("OK");
            }

            return NotFound();
        }
    }
}
