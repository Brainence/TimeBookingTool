namespace TBT.App.Models.Tools
{
    public class AccessToken
    {
        private static AccessToken instance;

        private AccessToken() { }

        public static AccessToken Instance
        {
            get
            {
                if (instance == null) instance = new AccessToken();
                return instance;
            }
        }

        public string Token { get; set; }
    }
}
