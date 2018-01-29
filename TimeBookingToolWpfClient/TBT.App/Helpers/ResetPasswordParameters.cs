namespace TBT.App.Helpers
{
    public class ResetPasswordParameters
    {
        public string TokenPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
