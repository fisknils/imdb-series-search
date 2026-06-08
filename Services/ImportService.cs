using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ImdbSearch.Data;
using ImdbSearch.Models;

namespace ImdbSearch.Services
{
    public class ImdbImportService
    {
        private readonly ImdbDbContext _context;
        private readonly string _datapath;
        private const int BatchSize = 1000;

        public ImdbImportService(ImdbDbContext context, string dataPath)
        {
            _context = context;
            _datapath = dataPath;
        }

        public async Task ImportAsync()
        {
            await ImportTitlesAsync();
            await ImportRatingsAsync();
        }

        private async Task ImportTitlesAsync()
        {
            string titlesPath = Path.Combine(_datapath, "title.basics.tsv");
            var genreCache = new Dictionary<string, int>();
            var titleBatch = new List<Title>();
            var titleGenreBatch = new List<TitleGenre>();

            using StreamReader reader = new StreamReader(titlesPath);
            await reader.ReadLineAsync(); // skip header row

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                string[] fields = line.Split('\t');

                if (fields[1] != "tvSeries" && fields[1] != "tvMiniSeries")
                    continue;

                Title title = new Title
                {
                    TConst = fields[0],
                    TitleType = fields[1],
                    PrimaryTitle = fields[2],
                    OriginalTitle = fields[3],
                    IsAdult = fields[4] == "1",
                    StartYear = ParseNullableInt(fields[5]),
                    EndYear = ParseNullableInt(fields[6]),
                    RuntimeMinutes = ParseNullableInt(fields[7])
                };

                titleBatch.Add(title);

                if (fields[8] != "\\N")
                {
                    foreach (string genreName in fields[8].Split(','))
                    {
                        if (!genreCache.TryGetValue(genreName, out int genreId))
                        {
                            var genre = await _context.Genres.FirstOrDefaultAsync(g => g.Name == genreName);

                            if (genre == null)
                            {
                                genre = new Genre { Name = genreName };
                                _context.Genres.Add(genre);
                                await _context.SaveChangesAsync();
                            }

                            genreCache[genreName] = genre.Id;
                            genreId = genre.Id;
                        }

                        titleGenreBatch.Add(new TitleGenre
                        {
                            TConst = fields[0],
                            GenreId = genreId
                        });
                    }
                }

                if (titleBatch.Count >= BatchSize)
                {
                    await _context.Titles.AddRangeAsync(titleBatch);
                    await _context.TitleGenres.AddRangeAsync(titleGenreBatch);
                    await _context.SaveChangesAsync();
                    titleBatch.Clear();
                    titleGenreBatch.Clear();
                }
            }

            // flush remaining
            if (titleBatch.Count > 0)
            {
                await _context.Titles.AddRangeAsync(titleBatch);
                await _context.TitleGenres.AddRangeAsync(titleGenreBatch);
                await _context.SaveChangesAsync();
            }
        }

        private async Task ImportRatingsAsync()
        {
            string ratingsPath = Path.Combine(_datapath, "title.ratings.tsv");
            var batch = new List<Rating>();

            HashSet<string> validTConsts = await _context.Titles
                .Select(t => t.TConst)
                .ToHashSetAsync();

            using StreamReader reader = new StreamReader(ratingsPath);
            await reader.ReadLineAsync(); // skip header row

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                string[] fields = line.Split('\t');

                if (!validTConsts.Contains(fields[0]))
                    continue;

                batch.Add(new Rating
                {
                    TConst = fields[0],
                    AverageRating = double.Parse(fields[1], System.Globalization.CultureInfo.InvariantCulture),
                    NumVotes = int.Parse(fields[2])
                });

                if (batch.Count >= BatchSize)
                {
                    await _context.Ratings.AddRangeAsync(batch);
                    await _context.SaveChangesAsync();
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await _context.Ratings.AddRangeAsync(batch);
                await _context.SaveChangesAsync();
            }
        }

        private static int? ParseNullableInt(string value)
        {
            return value == "\\N" ? null : int.TryParse(value, out int result) ? result : null;
        }
    }
}