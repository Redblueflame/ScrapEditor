using NSwag;

namespace ScrapEditor.Controllers
{
    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public string Token { get; set; } = null;
        public string Username { get; set; } = null;
    }
}