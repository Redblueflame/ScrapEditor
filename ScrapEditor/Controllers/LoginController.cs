using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ScrapEditor.LoginLogic;

namespace ScrapEditor.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private ILoginLogic _login;

        public LoginController(ILoginLogic logic)
        {
            _login = logic;
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<LoginResult> Login([FromBody] string username, [FromBody] string password)
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
    }
}
