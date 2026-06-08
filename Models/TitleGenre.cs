namespace ImdbSearch.Models
{
    public class TitleGenre
    {
        public string TConst { get; set; } = string.Empty;
        public int GenreId { get; set; }

        public Title Title { get; set; } = null!;
        public Genre Genre { get; set; } = null!;
    }
}