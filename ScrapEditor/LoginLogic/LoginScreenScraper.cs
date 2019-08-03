using System.Collections.Generic;

namespace ScrapEditor.LoginLogic
{
    public class LoginScreenScraper : ILoginLogic
    {
        private Dictionary<string, User> _users;
        public LoginScreenScraper()
        {
            _users = new Dictionary<string, User>();
        }
        public string LoginUser(string username, string password)
        {
            
        }

        public User GetUser(string uuid)
        {
            throw new System.NotImplementedException();
        }

        public void DisconnectUser(string uuid)
        {
            throw new System.NotImplementedException();
        }
    }
}