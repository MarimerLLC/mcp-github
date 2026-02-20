namespace GitHubMcpServer.Options;

public class GitHubOptions
{
    public string Token { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.github.com";
}
