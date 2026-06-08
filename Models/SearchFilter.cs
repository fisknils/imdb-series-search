namespace ImdbSearch.Models
{
    public class SearchFilter
    {
        public string? TitleQuery { get; set; }
        public List<string> RequiredGenres { get; set; } = new();
        public List<string> ExcludedGenres { get; set; } = new();
        public double? MinRating { get; set; }
        public int? MinVotes { get; set; }
        public int? MinStartYear { get; set; }
        public int? MaxStartYear { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 100;
    }
}