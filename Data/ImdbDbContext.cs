using Microsoft.EntityFrameworkCore;
using ImdbSearch.Models;

namespace ImdbSearch.Data
{

    public class ImdbDbContext : DbContext
    {
        public DbSet<Title> Titles { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<TitleGenre> TitleGenres { get; set; }

        public ImdbDbContext(DbContextOptions<ImdbDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Title>(entity =>
            {
                entity.HasKey(t => t.TConst);
                entity.HasOne(t => t.Rating)
                      .WithOne(r => r.Title)
                      .HasForeignKey<Rating>(r => r.TConst);
            });

            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasKey(r => r.TConst);
            });

            modelBuilder.Entity<Genre>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.HasIndex(g => g.Name).IsUnique();
            });

            modelBuilder.Entity<TitleGenre>(entity =>
            {
                entity.HasKey(tg => new { tg.TConst, tg.GenreId });
                entity.HasOne(tg => tg.Title)
                      .WithMany(t => t.TitleGenres)
                      .HasForeignKey(tg => tg.TConst);
                entity.HasOne(tg => tg.Genre)
                      .WithMany(g => g.TitleGenres)
                      .HasForeignKey(tg => tg.GenreId);
            });
        }
    }
}