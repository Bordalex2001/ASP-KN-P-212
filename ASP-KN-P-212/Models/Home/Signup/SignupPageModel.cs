namespace ASP_KN_P_212.Models.Home.Signup
{
    public class SignupPageModel
    {
        public SignupFormModel? FormModel { get; set; }
        public Dictionary<String,String>? ValidationErrors { get; set; }
    }
}
