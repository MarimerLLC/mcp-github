using GitHubMcpServer.Handlers;
using GitHubMcpServer.Options;
using GitHubMcpServer.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GitHubOptions>(builder.Configuration.GetSection("GitHub"));
builder.Services.AddSingleton<IGitHubAppTokenService, GitHubAppTokenService>();
builder.Services.AddTransient<GitHubAuthHandler>();

// Used by GitHubAppTokenService for JWT → installation token exchange (no auth handler)
builder.Services.AddHttpClient("github-app", (sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<GitHubOptions>>().Value;
    client.BaseAddress = new Uri(opts.BaseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", "mcp-github-server/1.0");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
});

// Main API client — installation token injected per-request by GitHubAuthHandler
builder.Services.AddHttpClient("github", (sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<GitHubOptions>>().Value;
    client.BaseAddress = new Uri(opts.BaseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", "mcp-github-server/1.0");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
}).AddHttpMessageHandler<GitHubAuthHandler>();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp();

app.Run();
