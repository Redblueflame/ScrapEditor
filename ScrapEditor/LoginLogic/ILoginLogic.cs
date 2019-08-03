using System;

namespace ScrapEditor.LoginLogic
{
    public interface ILoginLogic
    {
        /// <summary>
        /// Login the user using their username and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        string LoginUser(string username, string password);
        User GetUSer(string uuid);
    }

    public class User
    {
        public string Username { get; set; }
        public string password { get; set; }
        public int Limit { get; set; }
    }
}