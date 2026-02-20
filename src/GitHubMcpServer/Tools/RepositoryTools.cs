using System.ComponentModel;
using GitHubMcpServer;
using ModelContextProtocol.Server;

namespace GitHubMcpServer.Tools;

[McpServerToolType]
public class RepositoryTools
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RepositoryTools(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [McpServerTool(Name = "list_installation_repos"), Description("List all repositories the GitHub App installation has been granted access to")]
    public async Task<string> ListInstallationRepos(
        [Description("Number of results per page, max 100 (default: 30)")] int perPage = 30)
    {
        var client = _httpClientFactory.CreateClient("github");
        var response = await client.GetAsync($"/installation/repositories?per_page={perPage}");
        return await GitHubApiHelper.ReadResponseAsync(response);
    }

    [McpServerTool(Name = "list_org_repos"), Description("List repositories for a GitHub organization")]
    public async Task<string> ListOrgRepos(
        [Description("Organization login name")] string org,
        [Description("Filter type: all, public, private, forks, sources, member (default: all)")] string? type = "all",
        [Description("Number of results per page, max 100 (default: 30)")] int perPage = 30)
    {
        var client = _httpClientFactory.CreateClient("github");
        var response = await client.GetAsync($"/orgs/{org}/repos?type={type}&per_page={perPage}");
        return await GitHubApiHelper.ReadResponseAsync(response);
    }

    [McpServerTool(Name = "get_repo"), Description("Get details of a specific GitHub repository")]
    public async Task<string> GetRepo(
        [Description("Repository owner (user or org)")] string owner,
        [Description("Repository name")] string repo)
    {
        var client = _httpClientFactory.CreateClient("github");
        var response = await client.GetAsync($"/repos/{owner}/{repo}");
        return await GitHubApiHelper.ReadResponseAsync(response);
    }
}
