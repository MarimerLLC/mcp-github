using System.ComponentModel;
using GitHubMcpServer;
using ModelContextProtocol.Server;

namespace GitHubMcpServer.Tools;

[McpServerToolType]
public class IssueTools
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IssueTools(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [McpServerTool(Name = "list_issues"), Description("List issues for a GitHub repository")]
    public async Task<string> ListIssues(
        [Description("Repository owner")] string owner,
        [Description("Repository name")] string repo,
        [Description("Issue state: open, closed, all (default: open)")] string state = "open",
        [Description("Number of results per page, max 100 (default: 30)")] int perPage = 30)
    {
        var client = _httpClientFactory.CreateClient("github");
        var response = await client.GetAsync($"/repos/{owner}/{repo}/issues?state={state}&per_page={perPage}");
        return await GitHubApiHelper.ReadResponseAsync(response);
    }

    [McpServerTool(Name = "get_issue"), Description("Get details of a specific GitHub issue")]
    public async Task<string> GetIssue(
        [Description("Repository owner")] string owner,
        [Description("Repository name")] string repo,
        [Description("Issue number")] int issueNumber)
    {
        var client = _httpClientFactory.CreateClient("github");
        var response = await client.GetAsync($"/repos/{owner}/{repo}/issues/{issueNumber}");
        return await GitHubApiHelper.ReadResponseAsync(response);
    }

    [McpServerTool(Name = "create_issue"), Description("Create a new issue in a GitHub repository")]
    public async Task<string> CreateIssue(
        [Description("Repository owner")] string owner,
        [Description("Repository name")] string repo,
        [Description("Issue title")] string title,
        [Description("Issue body (markdown)")] string? body = null,
        [Description("Comma-separated list of label names")] string? labels = null)
    {
        var client = _httpClientFactory.CreateClient("github");
        var payload = new Dictionary<string, object?> { ["title"] = title };
        if (body is not null) payload["body"] = body;
        if (!string.IsNullOrWhiteSpace(labels))
            payload["labels"] = labels.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var response = await client.PostAsJsonAsync($"/repos/{owner}/{repo}/issues", payload);
        return await GitHubApiHelper.ReadResponseAsync(response);
    }

    [McpServerTool(Name = "update_issue"), Description("Update an existing GitHub issue")]
    public async Task<string> UpdateIssue(
        [Description("Repository owner")] string owner,
        [Description("Repository name")] string repo,
        [Description("Issue number")] int issueNumber,
        [Description("New title")] string? title = null,
        [Description("New body (markdown)")] string? body = null,
        [Description("New state: open or closed")] string? state = null)
    {
        var client = _httpClientFactory.CreateClient("github");
        var payload = new Dictionary<string, object?>();
        if (title is not null) payload["title"] = title;
        if (body is not null) payload["body"] = body;
        if (state is not null) payload["state"] = state;

        var response = await client.PatchAsJsonAsync($"/repos/{owner}/{repo}/issues/{issueNumber}", payload);
        return await GitHubApiHelper.ReadResponseAsync(response);
    }

    [McpServerTool(Name = "add_issue_comment"), Description("Add a comment to a GitHub issue")]
    public async Task<string> AddIssueComment(
        [Description("Repository owner")] string owner,
        [Description("Repository name")] string repo,
        [Description("Issue number")] int issueNumber,
        [Description("Comment body (markdown)")] string body)
    {
        var client = _httpClientFactory.CreateClient("github");
        var payload = new { body };
        var response = await client.PostAsJsonAsync($"/repos/{owner}/{repo}/issues/{issueNumber}/comments", payload);
        return await GitHubApiHelper.ReadResponseAsync(response);
    }

    [McpServerTool(Name = "list_issue_comments"), Description("List comments on a GitHub issue")]
    public async Task<string> ListIssueComments(
        [Description("Repository owner")] string owner,
        [Description("Repository name")] string repo,
        [Description("Issue number")] int issueNumber)
    {
        var client = _httpClientFactory.CreateClient("github");
        var response = await client.GetAsync($"/repos/{owner}/{repo}/issues/{issueNumber}/comments");
        return await GitHubApiHelper.ReadResponseAsync(response);
    }
}
