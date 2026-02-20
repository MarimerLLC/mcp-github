using System.ComponentModel;
using GitHubMcpServer;
using ModelContextProtocol.Server;

namespace GitHubMcpServer.Tools;

[McpServerToolType]
public class PullRequestTools
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PullRequestTools(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [McpServerTool(Name = "list_pull_requests"), Description("List pull requests for a GitHub repository")]
    public async Task<string> ListPullRequests(
        [Description("Repository owner")] string owner,
        [Description("Repository name")] string repo,
        [Description("PR state: open, closed, all (default: open)")] string state = "open",
        [Description("Number of results per page, max 100 (default: 30)")] int perPage = 30)
    {
        var client = _httpClientFactory.CreateClient("github");
        var response = await client.GetAsync($"/repos/{owner}/{repo}/pulls?state={state}&per_page={perPage}");
        return await GitHubApiHelper.ReadResponseAsync(response);
    }

    [McpServerTool(Name = "get_pull_request"), Description("Get details of a specific GitHub pull request")]
    public async Task<string> GetPullRequest(
        [Description("Repository owner")] string owner,
        [Description("Repository name")] string repo,
        [Description("Pull request number")] int pullNumber)
    {
        var client = _httpClientFactory.CreateClient("github");
        var response = await client.GetAsync($"/repos/{owner}/{repo}/pulls/{pullNumber}");
        return await GitHubApiHelper.ReadResponseAsync(response);
    }

    [McpServerTool(Name = "create_pull_request"), Description("Create a new pull request in a GitHub repository")]
    public async Task<string> CreatePullRequest(
        [Description("Repository owner")] string owner,
        [Description("Repository name")] string repo,
        [Description("Head branch name (the branch with changes)")] string head,
        [Description("Base branch name (the branch to merge into)")] string base_,
        [Description("Pull request title")] string title,
        [Description("Pull request body (markdown)")] string? body = null)
    {
        var client = _httpClientFactory.CreateClient("github");
        var payload = new Dictionary<string, object?>
        {
            ["head"] = head,
            ["base"] = base_,
            ["title"] = title
        };
        if (body is not null) payload["body"] = body;

        var response = await client.PostAsJsonAsync($"/repos/{owner}/{repo}/pulls", payload);
        return await GitHubApiHelper.ReadResponseAsync(response);
    }

    [McpServerTool(Name = "merge_pull_request"), Description("Merge a GitHub pull request")]
    public async Task<string> MergePullRequest(
        [Description("Repository owner")] string owner,
        [Description("Repository name")] string repo,
        [Description("Pull request number")] int pullNumber,
        [Description("Optional commit title for the merge commit")] string? commitTitle = null)
    {
        var client = _httpClientFactory.CreateClient("github");
        var payload = new Dictionary<string, object?>();
        if (commitTitle is not null) payload["commit_title"] = commitTitle;

        var response = await client.PutAsJsonAsync($"/repos/{owner}/{repo}/pulls/{pullNumber}/merge", payload);
        return await GitHubApiHelper.ReadResponseAsync(response);
    }

    [McpServerTool(Name = "list_pr_files"), Description("List files changed in a GitHub pull request")]
    public async Task<string> ListPRFiles(
        [Description("Repository owner")] string owner,
        [Description("Repository name")] string repo,
        [Description("Pull request number")] int pullNumber)
    {
        var client = _httpClientFactory.CreateClient("github");
        var response = await client.GetAsync($"/repos/{owner}/{repo}/pulls/{pullNumber}/files");
        return await GitHubApiHelper.ReadResponseAsync(response);
    }
}
