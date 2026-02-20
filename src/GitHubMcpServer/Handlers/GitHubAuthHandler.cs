using System.Net.Http.Headers;
using GitHubMcpServer.Services;

namespace GitHubMcpServer.Handlers;

public class GitHubAuthHandler : DelegatingHandler
{
    private readonly IGitHubAppTokenService _tokenService;

    public GitHubAuthHandler(IGitHubAppTokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenService.GetInstallationTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
