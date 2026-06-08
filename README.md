# IMDB Series Search

A Blazor Server application for searching TV series and mini-series from the IMDB dataset, with support for filtering by genre, rating, vote count, and year — including the ability to **exclude** specific genres from results.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) server (local or remote)
- IMDB dataset files (free, non-commercial use):
  - [title.basics.tsv.gz](https://datasets.imdbws.com/title.basics.tsv.gz)
  - [title.ratings.tsv.gz](https://datasets.imdbws.com/title.ratings.tsv.gz)

Extract both `.tsv.gz` files to a folder of your choice.

## Setup

If `dotnet ef` is not installed, install it first:

```bash
dotnet tool install --global dotnet-ef
```

Configure the connection string in `appsettings.json` to point at your PostgreSQL server:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=imdb;Username=postgres;Password=yourpassword"
}
```

Run the database migration:

```bash
dotnet ef database update
```

## Importing the Data

Run the import command, pointing it at the folder containing the extracted TSV files:

```bash
dotnet run -- --import /path/to/tsv/files
```

This will populate the database with TV series and mini-series. The import filters out movies and other title types, so it completes in a few minutes.

## Running the App

```bash
dotnet run
```

Then open the HTTPS URL shown in the terminal output in your browser.

## Tech Stack

- [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet) / [Blazor Server](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) with PostgreSQL (Npgsql)
- [MudBlazor](https://mudblazor.com/) component library
- [IMDB Non-Commercial Datasets](https://developer.imdb.com/non-commercial-datasets/)