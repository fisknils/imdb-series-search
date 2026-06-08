using ImdbSearch.Data;
using ImdbSearch.Models;
using Microsoft.EntityFrameworkCore;

namespace ImdbSearch.Services
{
    public class SearchService
    {
        private readonly ImdbDbContext _context;

        public SearchService(ImdbDbContext context)
        {
            _context = context;
        }

        public async Task<SearchResultPage> SearchAsync(SearchFilter filter)
        {
            IQueryable<Title> query = _context.Titles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.TitleQuery))
                query = query.Where(t => t.PrimaryTitle.Contains(filter.TitleQuery));

            if (filter.MinStartYear.HasValue)
                query = query.Where(t => t.StartYear >= filter.MinStartYear.Value);

            if (filter.MaxStartYear.HasValue)
                query = query.Where(t => t.StartYear <= filter.MaxStartYear.Value);

            foreach (string genre in filter.RequiredGenres)
                query = query.Where(t => t.TitleGenres.Any(tg => tg.Genre.Name == genre));

            foreach (string genre in filter.ExcludedGenres)
                query = query.Where(t => !t.TitleGenres.Any(tg => tg.Genre.Name == genre));

            if (filter.MinRating.HasValue || filter.MinVotes.HasValue)
                query = query.Where(t => t.Rating != null);

            if (filter.MinRating.HasValue)
                query = query.Where(t => t.Rating!.AverageRating >= filter.MinRating.Value);

            if (filter.MinVotes.HasValue)
                query = query.Where(t => t.Rating!.NumVotes >= filter.MinVotes.Value);

            int totalCount = await query.CountAsync();

            List<SearchResult> results = await query
                .OrderByDescending(t => t.Rating != null ? t.Rating.AverageRating : 0)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(t => new SearchResult
                {
                    TConst = t.TConst,
                    PrimaryTitle = t.PrimaryTitle,
                    TitleType = t.TitleType,
                    StartYear = t.StartYear,
                    EndYear = t.EndYear,
                    AverageRating = t.Rating != null ? t.Rating.AverageRating : null,
                    NumVotes = t.Rating != null ? t.Rating.NumVotes : null,
                    Genres = t.TitleGenres.Select(tg => tg.Genre.Name).ToList()
                })
                .ToListAsync();

            return new SearchResultPage
            {
                Results = results,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<List<string>> GetGenresAsync()
        {
            return await _context.Genres
                .OrderBy(g => g.Name)
                .Select(g => g.Name)
                .ToListAsync();
        }
    }
}