using ClaimsChat.Components;
using ClaimsChat.Data;
using ClaimsChat.Services.SealedBox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Persistence: EF Core + SQLite. Connection string is configurable; the default
// keeps a single local file so a fresh clone runs with no setup.
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=ClaimsChat.db";
builder.Services.AddDbContextFactory<ClaimsChatDbContext>(options =>
    options.UseSqlite(connectionString));

// SEALED BOX: the chat model. T1 registers a stub so the app runs without a key.
// T4 replaces this registration with a real Azure AI Foundry client.
builder.Services.AddSingleton<IChatClient, StubChatClient>();

// SEALED BOX: lexical retrieval over the seeded documents (consumed by chat in T4).
builder.Services.AddScoped<IDocumentContextProvider, LexicalDocumentContextProvider>();

var app = builder.Build();

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

// Apply any pending migrations on startup so a fresh clone creates its database
// automatically (clone -> dotnet run, no manual DB step). The factory is a
// singleton, so no DI scope is needed here.
var dbFactory = app.Services.GetRequiredService<IDbContextFactory<ClaimsChatDbContext>>();
using (var db = dbFactory.CreateDbContext())
{
    db.Database.Migrate();

    // Seed preset claims documents from the Documents folder into SQLite.
    var seedLogger = app.Services.GetRequiredService<ILoggerFactory>()
        .CreateLogger("DocumentSeeder");
    DocumentSeeder.Seed(db, app.Environment.ContentRootPath, seedLogger);
}

app.Run();
