using System;

namespace ScrapEditor.LoginLogic
{
    public interface ILoginLogic
    {
        /// <summary>
        /// Login the user using their username and password
        /// </summary>
        /// <param name="username">The username of the user to login</param>
        /// <param name="password">The password of the user</param>
        /// <returns>A string guid representing user's credentials</returns>
        string LoginUser(string username, string password);
        /// <summary>
        /// Get the user from the uuid provided
        /// </summary>
        /// <param name="uuid">A string guid representing user's credential</param>
        /// <returns>A <see cref="User"/> object</returns>
        User GetUser(string uuid);
        /// <summary>
        /// Remove the user from the login credentials store. This invalidates the guid.
        /// </summary>
        /// <param name="uuid">The uuid of the user</param>
        void DisconnectUser(string uuid);
    }
}