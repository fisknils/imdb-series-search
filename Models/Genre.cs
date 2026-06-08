namespace ImdbSearch.Models
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<TitleGenre> TitleGenres { get; set; } = new List<TitleGenre>();
    }
}