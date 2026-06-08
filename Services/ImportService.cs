using Microsoft.EntityFrameworkCore;
using ImdbSearch.Data;
using ImdbSearch.Models;
using Npgsql;
using NpgsqlTypes;

namespace ImdbSearch.Services
{
    public class ImdbImportService
    {
        private readonly ImdbDbContext _context;
        private readonly string _datapath;

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

            var titles = new List<Title>();
            var genreNames = new HashSet<string>();
            var titleGenreLinks = new List<(string TConst, string Genre)>();

            // Read all titles into memory first
            using (var reader = new StreamReader(titlesPath))
            {
                await reader.ReadLineAsync(); // skip header row

                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    string[] f = line.Split('\t');

                    if (f[1] != "tvSeries" && f[1] != "tvMiniSeries")
                        continue;

                    // Skip adult titles — they add no value to this app
                    if (f[4] == "1")
                        continue;

                    if (f[8] != "\\N" && f[8].Split(',').Contains("Adult"))
                        continue;

                    titles.Add(new Title
                    {
                        TConst = f[0],
                        TitleType = f[1],
                        PrimaryTitle = f[2],
                        OriginalTitle = f[3],
                        IsAdult = false,
                        StartYear = ParseNullableInt(f[5]),
                        EndYear = ParseNullableInt(f[6]),
                        RuntimeMinutes = ParseNullableInt(f[7])
                    });

                    if (f[8] != "\\N")
                    {
                        foreach (string genre in f[8].Split(','))
                        {
                            genreNames.Add(genre);
                            titleGenreLinks.Add((f[0], genre));
                        }
                    }
                }
            }

            var conn = (NpgsqlConnection)_context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            // Insert all genres at once, ignoring duplicates, then load their IDs
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO \"Genres\" (\"Name\") SELECT unnest(@names)";
                cmd.Parameters.AddWithValue("names", NpgsqlDbType.Array | NpgsqlDbType.Text, genreNames.ToArray());
                await cmd.ExecuteNonQueryAsync();
            }

            var genreCache = await _context.Genres.ToDictionaryAsync(g => g.Name, g => g.Id);

            // COPY titles
            await using (var writer = await conn.BeginBinaryImportAsync(
                "COPY \"Titles\" (\"TConst\", \"TitleType\", \"PrimaryTitle\", \"OriginalTitle\", \"IsAdult\", \"StartYear\", \"EndYear\", \"RuntimeMinutes\") FROM STDIN (FORMAT BINARY)"))
            {
                foreach (var t in titles)
                {
                    await writer.StartRowAsync();
                    await writer.WriteAsync(t.TConst, NpgsqlDbType.Text);
                    await writer.WriteAsync(t.TitleType, NpgsqlDbType.Text);
                    await writer.WriteAsync(t.PrimaryTitle, NpgsqlDbType.Text);
                    await writer.WriteAsync(t.OriginalTitle, NpgsqlDbType.Text);
                    await writer.WriteAsync(t.IsAdult, NpgsqlDbType.Boolean);
                    if (t.StartYear.HasValue) await writer.WriteAsync(t.StartYear.Value, NpgsqlDbType.Integer);
                    else await writer.WriteNullAsync();
                    if (t.EndYear.HasValue) await writer.WriteAsync(t.EndYear.Value, NpgsqlDbType.Integer);
                    else await writer.WriteNullAsync();
                    if (t.RuntimeMinutes.HasValue) await writer.WriteAsync(t.RuntimeMinutes.Value, NpgsqlDbType.Integer);
                    else await writer.WriteNullAsync();
                }
                await writer.CompleteAsync();
            }

            // COPY title-genre links
            await using (var writer = await conn.BeginBinaryImportAsync(
                "COPY \"TitleGenres\" (\"TConst\", \"GenreId\") FROM STDIN (FORMAT BINARY)"))
            {
                foreach (var (tconst, genre) in titleGenreLinks)
                {
                    if (!genreCache.TryGetValue(genre, out int genreId)) continue;
                    await writer.StartRowAsync();
                    await writer.WriteAsync(tconst, NpgsqlDbType.Text);
                    await writer.WriteAsync(genreId, NpgsqlDbType.Integer);
                }
                await writer.CompleteAsync();
            }
        }

        private async Task ImportRatingsAsync()
        {
            string ratingsPath = Path.Combine(_datapath, "title.ratings.tsv");

            HashSet<string> validTConsts = await _context.Titles
                .Select(t => t.TConst)
                .ToHashSetAsync();

            var conn = (NpgsqlConnection)_context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            await using var writer = await conn.BeginBinaryImportAsync(
                "COPY \"Ratings\" (\"TConst\", \"AverageRating\", \"NumVotes\") FROM STDIN (FORMAT BINARY)");

            using var reader = new StreamReader(ratingsPath);
            await reader.ReadLineAsync(); // skip header row

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                string[] f = line.Split('\t');

                if (!validTConsts.Contains(f[0]))
                    continue;

                await writer.StartRowAsync();
                await writer.WriteAsync(f[0], NpgsqlDbType.Text);
                await writer.WriteAsync(double.Parse(f[1], System.Globalization.CultureInfo.InvariantCulture), NpgsqlDbType.Double);
                await writer.WriteAsync(int.Parse(f[2]), NpgsqlDbType.Integer);
            }

            await writer.CompleteAsync();
        }

        private static int? ParseNullableInt(string value)
        {
            return value == "\\N" ? null : int.TryParse(value, out int result) ? result : null;
        }
    }
}
