using Azure;
using Azure.AI.OpenAI;
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

// SEALED BOX: the chat model. Use the real Azure OpenAI (gpt-5.4) client when the
// AzureOpenAI config is present; otherwise fall back to the stub so a fresh clone
// still boots and the Chat page works with no key (SPEC §6, §8). Endpoint is the
// base host (e.g. https://<resource>.cognitiveservices.azure.com/) — the SDK adds the route.
var aoaiEndpoint = builder.Configuration["AzureOpenAI:Endpoint"];
var aoaiDeployment = builder.Configuration["AzureOpenAI:Deployment"];
var aoaiKey = builder.Configuration["AzureOpenAI:Key"];

if (!string.IsNullOrWhiteSpace(aoaiEndpoint)
    && !string.IsNullOrWhiteSpace(aoaiDeployment)
    && !string.IsNullOrWhiteSpace(aoaiKey))
{
    builder.Services.AddChatClient(_ =>
        new AzureOpenAIClient(new Uri(aoaiEndpoint), new AzureKeyCredential(aoaiKey))
            .GetChatClient(aoaiDeployment)
            .AsIChatClient());
}
else
{
    builder.Services.AddChatClient(_ => new StubChatClient());
}

// SEALED BOX: lexical retrieval over the seeded documents.
builder.Services.AddScoped<IDocumentContextProvider, LexicalDocumentContextProvider>();

// SEALED BOX: grounded-chat orchestration (retrieve → assemble prompt → stream).
builder.Services.AddScoped<IGroundedChatService, GroundedChatService>();

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
