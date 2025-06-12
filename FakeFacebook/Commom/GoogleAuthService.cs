using Google.Apis.Auth;
namespace FakeFacebook.Commom
{
    public class GoogleAuthService
    {
        private readonly IConfiguration _config;
        public GoogleAuthService(IConfiguration configuration) {
            _config = configuration;

        }
        public async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string idToken)
        {
            var clientID = _config["GoogleAuth:IDClientCode"];
            if (clientID != null)
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {

                    Audience = new List<string>() { clientID }
                };
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return payload;
            }
            else { 
                return null;
            }

          
        }
    }
}
