// Services/IGuesserScoreService.cs
using ConnorAPI.Models;
using ConnorAPI.Services;

public interface IGuesserScoreService
{
    GalnetArticle GetNextArticle(int currentIndex);
    int CalculateScore(DateTime actualDate, int guessedYear, int guessedMonth, int guessedDay);
    bool IsGameOver(int currentRound, int totalRounds);
}

// Services/GuesserScoreService.cs
public class GuesserScoreService : IGuesserScoreService
{
    private readonly List<GalnetArticle> _articles;
    private readonly List<GalnetArticle> _gamearticles;
    public GuesserScoreService()
    {
        GalnetService galnetService = new GalnetService();
        _articles = galnetService.LoadArticlesFromJsonAsync().Result;
        _gamearticles = new List<GalnetArticle>(); // Initialize the list

        for (int i = 0; i < 5; i++)
        {
            Random random = new Random();
            int min = 1; // Set your minimum range value
            int max = _articles.Count; // Set your maximum range value (exclusive)

            int randomId = random.Next(min, max);
            _gamearticles.Add(_articles[randomId]);
        }

    }

    public GalnetArticle GetNextArticle(int currentIndex)
    {
        // Ensure we don't exceed the list bounds
        if (currentIndex >= 0 && currentIndex < _gamearticles.Count)
        {
            return _gamearticles[currentIndex];
        }
        return null; // Return null if the index is out of bounds
    }

    public int CalculateScore(DateTime actualDate, int guessedYear, int guessedMonth, int guessedDay)
    {
        int score = 0;
        int yearScore = 0;
        int monthScore = 0;
        int dayScore = 0;
        int bonusScore = 0;

        // Define scoring limits
        int maxBonusScore = 100;
        int maxYearScore = 500;
        int maxMonthScore = 1000;
        int maxDayScore = 3000;
        int maxYearDifference = 9; // Since there are only 10 possible years
        int maxMonthDifference = 11; // 12 months in a year
        int maxDayDifference = 30; // Maximum possible difference in days

        // Calculate year score with quadratic penalty within limited range
        int yearDifference = Math.Abs(actualDate.Year - guessedYear);
        if (yearDifference == 0)
        {
            yearScore = maxYearScore; // Full points for correct year
            bonusScore += maxBonusScore; // Bonus for correct year
        }
        else
        {
            yearScore = Math.Max(0, maxYearScore - (yearDifference * yearDifference) * (maxYearScore / (maxYearDifference * maxYearDifference)));
        }

        // Calculate month score with quadratic penalty within limited range
        int monthDifference = Math.Abs(actualDate.Month - guessedMonth);
        if (monthDifference == 0)
        {
            monthScore = maxMonthScore; // Full points for correct month
            bonusScore += maxBonusScore; // Bonus for correct month
        }
        else
        {
            monthScore = Math.Max(0, maxMonthScore - (monthDifference * monthDifference) * (maxMonthScore / (maxMonthDifference * maxMonthDifference)));
        }

        // Calculate day score with quadratic penalty within limited range
        int dayDifference = Math.Abs(actualDate.Day - guessedDay);
        if (dayDifference == 0)
        {
            dayScore = maxDayScore; // Full points for correct day
            bonusScore += maxBonusScore; // Bonus for correct day
        }
        else
        {
            dayScore = Math.Max(0, maxDayScore - (dayDifference * dayDifference) * (maxDayScore / (maxDayDifference * maxDayDifference)));
        }

        // Additional bonus if all components are correct
        if (yearDifference == 0 && monthDifference == 0 && dayDifference == 0)
        {
            bonusScore += 200;
        }

        // Calculate total score without final adjustment
        score = yearScore + monthScore + dayScore + bonusScore;

        // Apply final adjustment based on overall accuracy
        double finalMultiplier = 1.0; // Default to 100% if fully correct
        if (yearDifference <= 1 && monthDifference <= 1 && dayDifference <= 1)
        {
            if (yearDifference == 1 || monthDifference == 1 || dayDifference == 1)
            {
                finalMultiplier = 0.50; // 50% if any component is exactly 1 off
            }
        }
        else
        {
            finalMultiplier = 0.125; // 25% if any component is 2 or more off
        }

        // Apply final multiplier to score
        score = (int)(score * finalMultiplier);

        return score;
    }




    public bool IsGameOver(int currentRound, int totalRounds)
    {
        return currentRound >= totalRounds;
    }
}
