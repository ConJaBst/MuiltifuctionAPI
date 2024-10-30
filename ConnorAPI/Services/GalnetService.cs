using ConnorAPI.Models;
using HtmlAgilityPack;
using System;
using System.Diagnostics;
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

        //public async Task<List<GalnetArticle>> FetchArticlesUntilLatestAsync(string lastTitle)
        //{
        //    var articles = new List<GalnetArticle>();
        //    string baseUrl = "https://community.elitedangerous.com/galnet/";
        //    DateTime startDate = new DateTime(3301, 6, 5);
        //    Console.WriteLine("Start Date: " + startDate.ToString("dd-MMM-yyyy").ToUpper());
        //    int count = 12;

        //    while (count > 0)
        //    {
        //        Console.WriteLine("Fetching articles for " + startDate.ToString("dd-MMM-yyyy").ToUpper());
        //        Console.WriteLine("Count: " + count);
        //        var formattedDate = startDate.ToString("dd-MMM-yyyy").ToUpper();
        //        string url = $"{baseUrl}{formattedDate}";

        //        var htmlContent = await _httpClient.GetStringAsync(url);
        //        var htmlDoc = new HtmlDocument();
        //        htmlDoc.LoadHtml(htmlContent);

        //        var articleNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='article']");
        //        if (articleNode == null)
        //        {
        //            startDate = startDate.AddDays(1);
        //            continue;
        //        }

        //        var titleNode = articleNode.SelectSingleNode(".//h3");
        //        var contentNode = articleNode.SelectNodes(".//p")?.Skip(1).FirstOrDefault();

        //        string title = titleNode?.InnerText?.Trim();
        //        string content = contentNode?.InnerText?.Trim();

        //        if (string.IsNullOrEmpty(title) || title == lastTitle) break;

        //        articles.Add(new GalnetArticle
        //        {
        //            Title = title,
        //            Date = startDate,
        //            Content = content
        //        });

        //        startDate = startDate.AddDays(1);

        //        count--;
        //    }

        //    return articles;

        //}


        public async Task<List<GalnetArticle>> FetchArticlesUntilLatestAsync(string lastTitle)
        {
            var articles = new List<GalnetArticle>();
            string baseUrl = "https://community.elitedangerous.com/galnet/";
            DateTime startDate = new DateTime(3301, 6, 5);
            Console.WriteLine("Start Date: " + startDate.ToString("dd-MMM-yyyy").ToUpper());
            int count = 12;

            while (count > 0)
            {
                Console.WriteLine("Fetching articles for " + startDate.ToString("dd-MMM-yyyy").ToUpper());
                Console.WriteLine("Count: " + count);
                var formattedDate = startDate.ToString("dd-MMM-yyyy").ToUpper();
                string url = $"{baseUrl}{formattedDate}";

                var htmlContent = await _httpClient.GetStringAsync(url);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                var articleNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='article']");
                if (articleNodes == null)
                {
                    startDate = startDate.AddDays(1);
                    continue;
                }

                foreach (var articleNode in articleNodes)
                {
                    var titleNode = articleNode.SelectSingleNode(".//h3");
                    var contentNode = articleNode.SelectNodes(".//p")?.Skip(1).FirstOrDefault();

                    string title = titleNode?.InnerText?.Trim();
                    string content = contentNode?.InnerText?.Trim();

                    if (string.IsNullOrEmpty(title) || title == lastTitle)
                        return articles; // Exit if duplicate or previously saved title found

                    articles.Add(new GalnetArticle
                    {
                        Title = title,
                        Date = startDate,
                        Content = content
                    });
                }

                startDate = startDate.AddDays(1);
                count--;
            }

            return articles;
        }




        public async Task UpdateJsonAsync()
        {
            List<GalnetArticle> existingArticles = await LoadArticlesFromJsonAsync();
            string lastTitle = existingArticles.FirstOrDefault()?.Title;

            var newArticles = await FetchArticlesUntilLatestAsync(lastTitle);
            if (newArticles.Any())
            {
                existingArticles.InsertRange(0, newArticles);
                await File.WriteAllTextAsync(_jsonFilePath, JsonSerializer.Serialize(existingArticles));
            }
        }

        private async Task<List<GalnetArticle>> LoadArticlesFromJsonAsync()
        {
            if (!File.Exists(_jsonFilePath) || new FileInfo(_jsonFilePath).Length == 0)
            {
                return new List<GalnetArticle>();
            }

            string json = await File.ReadAllTextAsync(_jsonFilePath);
            return JsonSerializer.Deserialize<List<GalnetArticle>>(json) ?? new List<GalnetArticle>();
        }


    }

}