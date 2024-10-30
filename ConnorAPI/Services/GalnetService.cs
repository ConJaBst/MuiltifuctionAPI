using ConnorAPI.Models;
using HtmlAgilityPack;
using System.Text.Json;

namespace ConnorAPI.Services
{
    public class GalnetService
    {
        private readonly HttpClient _httpClient;
        private readonly string _jsonFilePath = Path.Combine("wwwroot", "json", "galnetArticles.json");

        public GalnetService()
        {
            _httpClient = new HttpClient();
        }



       

        public async Task<List<GalnetArticle>> FetchArticlesUntilLatestAsync()
        {
            var articles = new List<GalnetArticle>();
            string baseUrl = "https://community.elitedangerous.com/galnet/";

            // Get the current date in universe time
            DateTime Beginning = new DateTime(3301, 1, 6);
            // Get the most recent article information from JSON
            var (mostRecentDate, lastId) = await GetMostRecentArticleInfoAsync();

            DateTime startDate = mostRecentDate ?? Beginning; // Start from today in universe time if none exists
            Console.WriteLine($"startDate: {startDate}");
            int idCounter = (lastId ?? 0) + 1; // Start ID after the last saved ID
            Console.WriteLine($"lastId: {idCounter}");
            int HtmlCounter = 0;
            int ArticleCounter = 0;
            int MaxHtmlCounter = 6;
            int MaxArticleCounter = 5;

            while (startDate < new DateTime(3301, 2, 2))
            {
                Thread.Sleep(1000);
                // Makes the URL look like "https://community.elitedangerous.com/galnet/dd-MMM-yyyy"
                var formattedDate = startDate.ToString("dd-MMM-yyyy").ToUpper();
                string url = $"{baseUrl}{formattedDate}";
                HtmlCounter++;
                Console.WriteLine($"(HTML_FETCH_COUNT: {HtmlCounter}) Attempting articles for {formattedDate}");


                var htmlContent = await _httpClient.GetStringAsync(url);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                var articleNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='article']");
                if (articleNodes == null)
                {
                    startDate = startDate.AddDays(1);
                    continue;
                }
                Console.WriteLine($"\nFound {articleNodes.Count} at {formattedDate}");
                foreach (var articleNode in articleNodes)
                {
                    var titleNode = articleNode.SelectSingleNode(".//h3[@class='hilite galnetNewsArticleTitle']");
                    var contentNode = articleNode.SelectNodes(".//p")?.Skip(1).FirstOrDefault();

                    string title = titleNode?.InnerText?.Trim();
                    string content = contentNode?.InnerText?.Trim();

                    // Use the first line of content as the title if the title is missing
                    if (string.IsNullOrEmpty(title))
                    {
                        // Assuming the first line of the content serves as a title if title is missing
                        title = content.Split('\n').FirstOrDefault()?.Trim();
                    }

                    // If the content starts with the title, remove the title from content
                    if (!string.IsNullOrEmpty(content) && content.StartsWith(title))
                    {
                        content = content.Substring(title.Length).TrimStart();
                    }

                    // Skip duplicate or previously saved articles
                    if (string.IsNullOrEmpty(title) || (mostRecentDate != null && startDate <= mostRecentDate))
                    {
                        Console.WriteLine("Skipping article: " + title);
                        continue;
                    }

                    ArticleCounter++;
                    articles.Add(new GalnetArticle
                    {
                        Id = idCounter++,
                        Title = title,
                        Date = startDate,
                        Content = content
                    });
                    Console.WriteLine($"(ARTICLE_FETCH_COUNT: {ArticleCounter}) Fetched Id: {idCounter} - Title: {title} - Date: {startDate}");
                }

                Console.WriteLine("Done. \n");
                startDate = startDate.AddDays(1); // Move to the next day
            }
            Console.WriteLine("|END|");
            return articles;
        }


        public async Task<(DateTime? Date, int? Id)> GetMostRecentArticleInfoAsync()
        {
            string filePath = Path.Combine("wwwroot", "json", "galnetArticles.json");
            if (!File.Exists(filePath)) return (null, null);

            string jsonContent = await File.ReadAllTextAsync(filePath);
            var articles = JsonSerializer.Deserialize<List<GalnetArticle>>(jsonContent);

            if (articles == null || articles.Count == 0) return (null, null);

            var mostRecentArticle = articles.OrderByDescending(a => a.Date).FirstOrDefault();
            return (mostRecentArticle?.Date, mostRecentArticle?.Id);
        }

        public async Task UpdateJsonAsync()
        {
            if (!File.Exists(_jsonFilePath) || new FileInfo(_jsonFilePath).Length == 0)
            {
                await File.WriteAllTextAsync(_jsonFilePath, "[]");
            }

            List<GalnetArticle> existingArticles = await LoadArticlesFromJsonAsync();
            var newArticles = await FetchArticlesUntilLatestAsync();

            if (newArticles.Any())
            {
                existingArticles.AddRange(newArticles);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true, // For readability, adds new lines and indentation
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Allows special characters like newlines
                };

                await File.WriteAllTextAsync(_jsonFilePath, JsonSerializer.Serialize(existingArticles, options));
            }
        }






        public async Task<List<GalnetArticle>> LoadArticlesFromJsonAsync()
        {
            if (!File.Exists(_jsonFilePath) || new FileInfo(_jsonFilePath).Length == 0)
            {
                return new List<GalnetArticle>();
            }

            try
            {
                string json = await File.ReadAllTextAsync(_jsonFilePath);
                return JsonSerializer.Deserialize<List<GalnetArticle>>(json) ?? new List<GalnetArticle>();
            }
            catch (JsonException)
            {
                // Handle invalid JSON by returning an empty list
                return new List<GalnetArticle>();
            }
        }


    }
}