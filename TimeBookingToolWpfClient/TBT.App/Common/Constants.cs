using System.Text.RegularExpressions;
using System.Windows.Media;

namespace TBT.App.Common
{
    public class Constants
    {
        public static string ServerBaseUrl => "ServerBaseUrl";
        public static string LoginUrl => "LoginUrl";
        public static string EnableNotification => "EnableNotification";
        public static string Username => "Username"; 
        public static string AuthenticationUsername => "AuthenticationUsername"; 
        public static string RunOnStartup => "RunOnStartup"; 
        public static string RefreshToken => "RefreshToken";
        public static string AccessToken => "AccessToken";
        public static string RememberMe => "RememberMe";
        public static string CurrentRunningTimeEntryId => "CurrentRunningTimeEntryId";
        public static string EnableGreetingNotification => "EnableGreetingNotification";
        public static string CultureTag => "CultureTag";
        public static Regex TimeRegex => new Regex(@"^(\d{1,2}){1}([:.]{1}\d{0,2})?$");
    }

    public class MessageColors
    {
        public static Brush Error => Brushes.DarkRed;
        public static Brush Message => Brushes.DarkGreen;
    }
}
