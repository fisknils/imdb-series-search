namespace ImdbSearch.Models
{
    public class Rating
    {
        public string TConst { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int NumVotes { get; set; }

        public Title? Title { get; set; }
    }
}