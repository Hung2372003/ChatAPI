namespace FakeFacebook.ModelViewControllers
{
    public class GoogleLoginRequestModelViewcs
    {
        public string? IdToken { get; set; }
    }
    public class GoogleTokenDto
    {
        public string Token { get; set; } = string.Empty;
    }
    public class GoogleAuthRequest
    {
        public string? Code { get; set; }
    }
    public class GoogleTokenResponse
    {
        public string? access_token { get; set; }
        public string? id_token { get; set; }
        public int? expires_in { get; set; }
        public string? token_type { get; set; }
    }


}
