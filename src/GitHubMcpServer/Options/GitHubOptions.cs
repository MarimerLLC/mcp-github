namespace GitHubMcpServer.Options;

public class GitHubOptions
{
    public string AppId { get; set; } = string.Empty;
    public string PrivateKeyPem { get; set; } = string.Empty;
    public string InstallationId { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.github.com";
}
