namespace ScrapEditor.LoginLogic
{
    public class User
    {
        public string Username { get; }
        public string Password { get; }
        public int Limit { get; }

        public User(string username, string password)
        {
            Username = username;
            Password = password;
            Limit = 0;
        }
    }
}