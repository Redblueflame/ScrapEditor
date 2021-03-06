using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#pragma warning disable 1998

namespace ScrapEditor.LoginLogic
{
    public class LoginScreenScraper : ILoginLogic
    {
        private readonly Dictionary<string, User> _users;
        private readonly IScreenScraperAPI _api;
        public LoginScreenScraper(IScreenScraperAPI api)
        {
            this._api = api;
            _users = new Dictionary<string, User>();
        }
        public async Task<string> LoginUser(string username, string password)
        {
            if (!await _api.Login(username, password)) return null;
            var guid = Guid.NewGuid().ToString();
            _users.Add(guid, new User(username, password));
            return guid;
        }
        public Task<string> AddUser(string username, string password)
        {
            if (_users.Any(x => x.Value.Username == username))
            {
                return null;
            }
            var guid = Guid.NewGuid().ToString();
            _users.Add(guid, new User(username, password));
            return Task.FromResult(guid);
        }
        
        public async Task<User> GetUser(string uuid)
        {
            return _users.GetValueOrDefault(uuid, null);
        }
        
        public async Task<bool> DisconnectUser(string uuid)
        {
            if (!_users.ContainsKey(uuid))
            {
                return false;
            }
            _users.Remove(uuid);
            return true;
        }
    }
}