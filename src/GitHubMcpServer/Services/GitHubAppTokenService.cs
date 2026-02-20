using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GitHubMcpServer.Options;
using Microsoft.Extensions.Options;

namespace GitHubMcpServer.Services;

public interface IGitHubAppTokenService
{
    Task<string> GetInstallationTokenAsync(CancellationToken ct = default);
}

public class GitHubAppTokenService : IGitHubAppTokenService
{
    private readonly GitHubOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private string? _cachedToken;
    private DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;

    public GitHubAppTokenService(IOptions<GitHubOptions> options, IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetInstallationTokenAsync(CancellationToken ct = default)
    {
        // Fast path: cached token still valid (5-minute buffer before expiry)
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiry.AddMinutes(-5))
            return _cachedToken;

        await _semaphore.WaitAsync(ct);
        try
        {
            // Re-check after acquiring lock
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiry.AddMinutes(-5))
                return _cachedToken;

            var jwt = GenerateAppJwt();
            (_cachedToken, _tokenExpiry) = await FetchInstallationTokenAsync(jwt, ct);
            return _cachedToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private string GenerateAppJwt()
    {
        var now = DateTimeOffset.UtcNow;
        var iat = now.AddSeconds(-60).ToUnixTimeSeconds();
        var exp = now.AddMinutes(9).ToUnixTimeSeconds();

        var header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(
            new { alg = "RS256", typ = "JWT" }));
        var payload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(
            new { iat, exp, iss = _options.AppId }));

        var message = $"{header}.{payload}";

        // Handle both literal \n escapes (env vars) and real newlines (files/secrets)
        var pem = _options.PrivateKeyPem.Replace("\\n", "\n");
        using var rsa = RSA.Create();
        rsa.ImportFromPem(pem);

        var signature = rsa.SignData(
            Encoding.ASCII.GetBytes(message),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return $"{message}.{Base64UrlEncode(signature)}";
    }

    private async Task<(string token, DateTimeOffset expiry)> FetchInstallationTokenAsync(
        string jwt, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("github-app");
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/app/installations/{_options.InstallationId}/access_tokens");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(content);
        var token = doc.RootElement.GetProperty("token").GetString()!;
        var expiresAt = doc.RootElement.GetProperty("expires_at").GetDateTimeOffset();

        return (token, expiresAt);
    }

    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
