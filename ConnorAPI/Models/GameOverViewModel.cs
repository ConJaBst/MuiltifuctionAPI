namespace ConnorAPI.Models
{
    public class GameOverViewModel
    {
        public int ArticleId { get; set; } 
        public string ArticleTitle { get; set; }
        public DateTime PublishedDate { get; set; }
        public DateTime GuessedDate { get; set; }
        public int RoundScore { get; set; }
    }


}
