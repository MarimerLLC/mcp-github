using System.Text.Json;

namespace GitHubMcpServer;

internal static class GitHubApiHelper
{
    private static readonly JsonSerializerOptions PrettyPrint = new() { WriteIndented = true };

    public static async Task<string> ReadResponseAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return JsonSerializer.Serialize(new
            {
                error = true,
                status = (int)response.StatusCode,
                message = content
            }, PrettyPrint);
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            return JsonSerializer.Serialize(doc, PrettyPrint);
        }
        catch
        {
            return content;
        }
    }
}
