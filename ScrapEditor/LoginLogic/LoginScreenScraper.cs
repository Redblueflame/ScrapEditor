using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScrapEditor.LoginLogic
{
    public class LoginScreenScraper : ILoginLogic
    {
        private Dictionary<string, User> _users;
        private ScreenScraperAPI api;
        public LoginScreenScraper(ScreenScraperAPI api)
        {
            this.api = api;
            _users = new Dictionary<string, User>();
        }
        public async Task<string> LoginUser(string username, string password)
        {
            if (!await api.Login(username, password)) return null;
            var guid = Guid.NewGuid().ToString();
            _users.Add(guid, new User(username, password));
            return guid;
        }

        public Task<User> GetUser(string uuid)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> DisconnectUser(string uuid)
        {
            throw new System.NotImplementedException();
        }
    }
}