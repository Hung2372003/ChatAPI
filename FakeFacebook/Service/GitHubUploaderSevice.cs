using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace FakeFacebook.Service
{
    public class GitHubUploaderService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _ownerGit;
        private readonly string? _repoGit;
        private readonly string? _branchGit;
        private readonly string? _tokenGit;
        private readonly string? _getImageDataLink;
        public GitHubUploaderService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("FakeFacebookApp"); // bắt buộc với GitHub API
            _ownerGit = configuration["Git:Owner"];
            _repoGit = configuration["Git:Repositories"];
            _branchGit = configuration["Git:Branch"];
            _tokenGit = configuration["Git:TokenGitChatApiFile"];
            _getImageDataLink = configuration["Git:GetImageDataLink"];
        }

        public async Task<string> UploadFileAsync(string path, IFormFile file, string message)
        {
            var checkUrl = $"https://api.github.com/repos/{_ownerGit}/{_repoGit}/contents/{path}";
            var checkRequest = new HttpRequestMessage(HttpMethod.Get, checkUrl);
            checkRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenGit);
            checkRequest.Headers.UserAgent.ParseAdd("FakeFacebookApp");
            var checkResponse = await _httpClient.SendAsync(checkRequest);
            if (checkResponse.IsSuccessStatusCode)
            {
                return $"{_getImageDataLink}/{path}";
            }

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileBytes = ms.ToArray();
            string base64Content = Convert.ToBase64String(fileBytes);
            var payload = new
            {
                message,
                content = base64Content,
                branch = _branchGit
            };

            var uploadRequest = new HttpRequestMessage(HttpMethod.Put, checkUrl);
            uploadRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenGit);
            uploadRequest.Headers.UserAgent.ParseAdd("FakeFacebookApp");
            uploadRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var uploadResponse = await _httpClient.SendAsync(uploadRequest);
            if (!uploadResponse.IsSuccessStatusCode)
            {
                string error = await uploadResponse.Content.ReadAsStringAsync();
                throw new Exception($"GitHub upload failed: {error}");
            }

            return $"{_getImageDataLink}/{path}";
        }

    }
}
