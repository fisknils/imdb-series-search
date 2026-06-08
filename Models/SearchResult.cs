namespace ImdbSearch.Models
{
    public class SearchResult
    {
        public string TConst { get; set; } = string.Empty;
        public string PrimaryTitle { get; set; } = string.Empty;
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public double? AverageRating { get; set; }
        public int? NumVotes { get; set; }
        public List<string> Genres { get; set; } = new();
        public string TitleType { get; set; } = string.Empty;
    }
}