using ImdbSearch.Components;
using ImdbSearch.Data;
using ImdbSearch.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddDbContext<ImdbDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<SearchService>();
builder.Services.AddMudServices();

var app = builder.Build();

if (args.Contains("--import"))
{
    int pathIndex = Array.IndexOf(args, "--import") + 1;
    if (pathIndex >= args.Length)
    {
        Console.WriteLine("Usage: dotnet run -- --import <path-to-tsv-files>");
        return;
    }
    string dataPath = args[pathIndex];

    using IServiceScope scope = app.Services.CreateScope();
    ImdbImportService importer = new ImdbImportService(
        scope.ServiceProvider.GetRequiredService<ImdbDbContext>(),
        dataPath
    );
    await importer.ImportAsync();
    return;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
