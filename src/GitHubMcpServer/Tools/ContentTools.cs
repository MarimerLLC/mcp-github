using System.ComponentModel;
using System.Text;
using System.Text.Json;
using GitHubMcpServer;
using ModelContextProtocol.Server;

namespace GitHubMcpServer.Tools;

[McpServerToolType]
public class ContentTools
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions PrettyPrint = new() { WriteIndented = true };

    public ContentTools(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [McpServerTool(Name = "get_file_contents"), Description("Get the decoded text contents of a file in a GitHub repository")]
    public async Task<string> GetFileContents(
        [Description("Repository owner")] string owner,
        [Description("Repository name")] string repo,
        [Description("File path within the repository")] string path,
        [Description("Branch, tag, or commit SHA (default: repo default branch)")] string? ref_ = null)
    {
        var client = _httpClientFactory.CreateClient("github");
        var url = $"/repos/{owner}/{repo}/contents/{path.TrimStart('/')}";
        if (ref_ is not null) url += $"?ref={Uri.EscapeDataString(ref_)}";

        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return await GitHubApiHelper.ReadResponseAsync(response);

        var content = await response.Content.ReadAsStringAsync();
        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (root.TryGetProperty("content", out var encodedContent) &&
                root.TryGetProperty("encoding", out var encoding) &&
                encoding.GetString() == "base64")
            {
                var base64 = encodedContent.GetString()?.Replace("\n", "") ?? string.Empty;
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                var result = new
                {
                    name = root.TryGetProperty("name", out var n) ? n.GetString() : null,
                    path = root.TryGetProperty("path", out var p) ? p.GetString() : null,
                    sha = root.TryGetProperty("sha", out var s) ? s.GetString() : null,
                    size = root.TryGetProperty("size", out var sz) ? sz.GetInt64() : 0L,
                    html_url = root.TryGetProperty("html_url", out var hu) ? hu.GetString() : null,
                    content = decoded
                };
                return JsonSerializer.Serialize(result, PrettyPrint);
            }

            return JsonSerializer.Serialize(doc, PrettyPrint);
        }
        catch
        {
            return content;
        }
    }

    [McpServerTool(Name = "list_directory"), Description("List the contents of a directory in a GitHub repository")]
    public async Task<string> ListDirectory(
        [Description("Repository owner")] string owner,
        [Description("Repository name")] string repo,
        [Description("Directory path within the repository (use empty string for root)")] string path,
        [Description("Branch, tag, or commit SHA (default: repo default branch)")] string? ref_ = null)
    {
        var client = _httpClientFactory.CreateClient("github");
        var trimmed = path.Trim('/');
        var url = $"/repos/{owner}/{repo}/contents/{trimmed}";
        if (ref_ is not null) url += $"?ref={Uri.EscapeDataString(ref_)}";

        var response = await client.GetAsync(url);
        return await GitHubApiHelper.ReadResponseAsync(response);
    }

    [McpServerTool(Name = "list_branches"), Description("List branches in a GitHub repository")]
    public async Task<string> ListBranches(
        [Description("Repository owner")] string owner,
        [Description("Repository name")] string repo,
        [Description("Number of results per page, max 100 (default: 30)")] int perPage = 30)
    {
        var client = _httpClientFactory.CreateClient("github");
        var response = await client.GetAsync($"/repos/{owner}/{repo}/branches?per_page={perPage}");
        return await GitHubApiHelper.ReadResponseAsync(response);
    }

    [McpServerTool(Name = "get_branch"), Description("Get details of a specific branch in a GitHub repository")]
    public async Task<string> GetBranch(
        [Description("Repository owner")] string owner,
        [Description("Repository name")] string repo,
        [Description("Branch name")] string branch)
    {
        var client = _httpClientFactory.CreateClient("github");
        var response = await client.GetAsync($"/repos/{owner}/{repo}/branches/{Uri.EscapeDataString(branch)}");
        return await GitHubApiHelper.ReadResponseAsync(response);
    }
}
