namespace Base.DTOs
{
    public class APIResponse_Auth_Login : APIResponse_Auth
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string userName { get; set; }
        public string instance_url { get; set; }

        public APIResponse_Auth_Login()
        {
            access_token = "";
            token_type = "";
            expires_in = "";
            userName = "";
            instance_url = "";
        }
    }
}