using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ConnorAPI.Services
{
    public class GithubService
    {
        private readonly HttpClient _httpClient;
        private readonly string _repoOwner = "ConJaBst"; // GitHub username
        private readonly string _repoName = "Galnet-Articles"; // Repository name
        private readonly string _filePath = "galnetArticles.json"; // Path within the repository
        private readonly string _token; // GitHub personal access token


        public GithubService()
        {
            _httpClient = new HttpClient();

            // Retrieve the GitHub token from environment variables
            _token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            if (string.IsNullOrEmpty(_token))
            {
                throw new InvalidOperationException("GitHub token is not set in environment variables.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

        public async Task BackupJsonToFileAsync(string localFilePath)
        {
            Console.WriteLine("Attempting Backup");
            try
            {
                // Read the local JSON file content
                var fileContent = await System.IO.File.ReadAllTextAsync(localFilePath);
                var base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));

                // Get the SHA of the existing file (if any) to update it
                string sha = await GetFileShaAsync();

                // Prepare the content for GitHub API
                var requestBody = new
                {
                    message = "Automated backup of galnetArticles.json",
                    content = base64Content,
                    sha = sha // Needed for updates
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                // Make the request to create/update the file
                var url = $"https://api.github.com/repos/{_repoOwner}/{_repoName}/contents/{_filePath}";
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("BackupApp"); // GitHub requires a User-Agent header

                var response = await _httpClient.PutAsync(url, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("File uploaded successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to upload file: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async Task<string> GetFileShaAsync()
        {
            var url = $"https://api.github.com/repos/{_repoOwner}/{_repoName}/contents/{_filePath}";
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("BackupApp");

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseBody);
                return jsonDocument.RootElement.GetProperty("sha").GetString();
            }

            // Return null if the file doesn't exist yet
            return null;
        }

    }


}
