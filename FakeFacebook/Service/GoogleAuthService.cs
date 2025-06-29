using Azure.Core;
using FakeFacebook.ModelViewControllers;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
namespace FakeFacebook.Service
{
    public class GoogleAuthService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public GoogleAuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _config = configuration;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<GoogleJsonWebSignature.Payload> GoogleExchangeCode(GoogleAuthRequest request)
        {
            var clientId = _config["GoogleAuth:IDClientCode"]!;
            var clientSecret = _config["GoogleAuth:ClientSecretCode"]!;
            var redirectUri = _config["GoogleAuth:RedirectUri"]!; // Phải khớp 100% với URI gửi lên Google
            var values = new Dictionary<string, string>{
                { "code", request.Code! },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            };
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(values));
                var json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Không thể lấy token từ Google: {json}");
                }
                var tokenData = JsonSerializer.Deserialize<GoogleTokenResponse>(json);
                return await GoogleJsonWebSignature.ValidateAsync(tokenData!.id_token); ;
            

        }
    }
}
