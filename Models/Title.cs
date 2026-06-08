namespace ImdbSearch.Models
{
    public class Title
    {
        public string TConst { get; set; } = string.Empty;
        public string TitleType { get; set; } = string.Empty;
        public string PrimaryTitle { get; set; } = string.Empty;
        public string OriginalTitle { get; set; } = string.Empty;
        public bool IsAdult { get; set; }
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public int? RuntimeMinutes { get; set; }

        public ICollection<TitleGenre> TitleGenres { get; set; } = new List<TitleGenre>();
        public Rating? Rating { get; set; }
    }
}