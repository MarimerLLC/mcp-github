using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using GitHubMcpServer.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GitHubOptions>(builder.Configuration.GetSection("GitHub"));

builder.Services.AddHttpClient("github", (sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<GitHubOptions>>().Value;
    client.BaseAddress = new Uri(opts.BaseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", "mcp-github-server/1.0");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", opts.Token);
});

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp();

app.Run();
