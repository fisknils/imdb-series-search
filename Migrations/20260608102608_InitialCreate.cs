using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImdbSearch.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Titles",
                columns: table => new
                {
                    TConst = table.Column<string>(type: "TEXT", nullable: false),
                    TitleType = table.Column<string>(type: "TEXT", nullable: false),
                    PrimaryTitle = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalTitle = table.Column<string>(type: "TEXT", nullable: false),
                    IsAdult = table.Column<bool>(type: "INTEGER", nullable: false),
                    StartYear = table.Column<int>(type: "INTEGER", nullable: true),
                    EndYear = table.Column<int>(type: "INTEGER", nullable: true),
                    RuntimeMinutes = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Titles", x => x.TConst);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    TConst = table.Column<string>(type: "TEXT", nullable: false),
                    AverageRating = table.Column<decimal>(type: "TEXT", nullable: false),
                    NumVotes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.TConst);
                    table.ForeignKey(
                        name: "FK_Ratings_Titles_TConst",
                        column: x => x.TConst,
                        principalTable: "Titles",
                        principalColumn: "TConst",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TitleGenres",
                columns: table => new
                {
                    TConst = table.Column<string>(type: "TEXT", nullable: false),
                    GenreId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TitleGenres", x => new { x.TConst, x.GenreId });
                    table.ForeignKey(
                        name: "FK_TitleGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TitleGenres_Titles_TConst",
                        column: x => x.TConst,
                        principalTable: "Titles",
                        principalColumn: "TConst",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Genres_Name",
                table: "Genres",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TitleGenres_GenreId",
                table: "TitleGenres",
                column: "GenreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "TitleGenres");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Titles");
        }
    }
}
