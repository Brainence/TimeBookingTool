namespace TBT.App.Models.Tools
{
    public class AccessToken
    {
        private static AccessToken _instance;

        private AccessToken() { }

        public static AccessToken Instance => _instance ?? (_instance = new AccessToken());

        public string Token { get; set; }
    }
}
