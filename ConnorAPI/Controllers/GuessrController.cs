using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using ConnorAPI.Models;
using ConnorAPI.Extension;

public class GuessrController : Controller
{
    private readonly IGuesserScoreService _gameService;
    private const int TotalRounds = 5;

    public GuessrController(IGuesserScoreService gameService)
    {
        _gameService = gameService;
    }

    private int GetSessionScore()
    {
        return HttpContext.Session.GetInt32("Score") ?? 0;
    }

    private void SetSessionScore(int score)
    {
        HttpContext.Session.SetInt32("Score", score);
    }

    private int GetSessionRound()
    {
        return HttpContext.Session.GetInt32("Round") ?? 1;
    }

    private void SetSessionRound(int round)
    {
        HttpContext.Session.SetInt32("Round", round);
    }

    private List<GameOverViewModel> GetSessionResults()
    {
        var resultData = HttpContext.Session.GetObject<List<GameOverViewModel>>("Results") ?? new List<GameOverViewModel>();
        return resultData;
    }

    private void SetSessionResults(List<GameOverViewModel> results)
    {
        HttpContext.Session.SetObject("Results", results);
    }

    [HttpGet]
    public IActionResult Play()
    {
        int round = GetSessionRound();
        int score = GetSessionScore();

        if (round > TotalRounds)
        {
            // Redirect to Result (GameOver) if all rounds are completed
            return RedirectToAction("Result");
        }

        var article = _gameService.GetNextArticle(round - 1);
        if (article == null)
        {
            // If no article is found, end the game
            return RedirectToAction("Result");
        }

        ViewData["Round"] = round;
        ViewData["Score"] = score;
        return View(article);
    }

    [HttpPost]
    public IActionResult SubmitGuess(int articleId, int yearGuess, int monthGuess, int dayGuess)
    {
        int round = GetSessionRound();
        int score = GetSessionScore();

        // Fetch the article for the correct answer
        var article = _gameService.GetNextArticle(round - 1);
        if (article == null)
        {
            return RedirectToAction("Result");
        }

        // Calculate score for this round
        int roundScore = _gameService.CalculateScore(article.Date, yearGuess, monthGuess, dayGuess);
        int newTotalScore = score + roundScore;

        // Update session values for score and round
        SetSessionScore(newTotalScore);
        SetSessionRound(round + 1);

        // Store the result of the current round
        var results = GetSessionResults();
        results.Add(new GameOverViewModel
        {
            ArticleId = article.Id,
            ArticleTitle = article.Title,
            PublishedDate = article.Date,
            GuessedDate = new DateTime(yearGuess, monthGuess, dayGuess),
            RoundScore = roundScore
        });
        SetSessionResults(results);

        // Redirect to RoundSummary to show the score from the current round and the cumulative score
        return RedirectToAction("RoundSummary");
    }

    public IActionResult RoundSummary()
    {
        int roundNumber = GetSessionRound() - 1; // Previous round
        int totalScore = GetSessionScore();
        int previousRoundScore = totalScore - (HttpContext.Session.GetInt32("PreviousTotalScore") ?? 0);

        // Update PreviousTotalScore for the next calculation
        HttpContext.Session.SetInt32("PreviousTotalScore", totalScore);

        ViewData["RoundNumber"] = roundNumber;
        ViewData["TotalScore"] = totalScore;
        ViewData["PreviousRoundScore"] = previousRoundScore;

        return View();
    }

    [HttpPost]
    public IActionResult NextRound()
    {
        // Redirect to Play with the session-stored round and score
        return RedirectToAction("Play");
    }

    public IActionResult Result()
    {
        // Retrieve the game results from session
        var results = GetSessionResults();
        int totalScore = GetSessionScore();

        // Pass results and total score to the GameOver view
        ViewData["TotalScore"] = totalScore;

        // Clear session data at the end of the game
        HttpContext.Session.Clear();

        return View("GameOver", results); // Pass results to GameOver view
    }
}
