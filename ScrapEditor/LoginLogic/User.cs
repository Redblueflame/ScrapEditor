using System;

namespace ScrapEditor.LoginLogic
{
    public class User
    {
        public string Username { get; }
        public string Password { get; }
        public int Limit { get; }
        public int ImportedGames { get; set; }
        public int AddedInfo { get; set; }
        public Grade Role { get; set; }
        public User(string username, string password)
        {
            Username = username;
            Password = password;
            Role = Grade.User;
            ImportedGames = 0;
            AddedInfo = 0;
            Limit = 0;
        }
    }

    public enum Grade
    {
        Owner = 1024,
        Administrator = 1000,
        Moderator = 512,
        Developer = 256,
        User = 1
    }
}