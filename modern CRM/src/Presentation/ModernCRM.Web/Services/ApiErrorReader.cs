using System.Text.Json;

namespace ModernCRM.Web.Services;

/// <summary>
/// Reads the most useful message from any API error response body.
/// Handles: plain string, { "message": "..." }, { "title": "..." }, and raw JSON.
/// </summary>
public static class ApiErrorReader
{
    public static async Task<string> ReadAsync(HttpResponseMessage response)
    {
        var body = (await response.Content.ReadAsStringAsync()).Trim();
        if (string.IsNullOrWhiteSpace(body))
            return $"Error {(int)response.StatusCode}";

        // Plain string (not JSON)
        if (!body.StartsWith("{") && !body.StartsWith("["))
            return body.Trim('"');

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // { "message": "..." }  ← our custom format
            if (root.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
                return msg.GetString()!;

            // { "title": "...", "errors": { "Field": ["msg"] } }  ← ProblemDetails fallback
            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var first = errors.EnumerateObject()
                    .SelectMany(p => p.Value.EnumerateArray())
                    .Select(v => v.GetString())
                    .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
                if (first != null) return first;
            }

            if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                return title.GetString()!;
        }
        catch { /* fall through to raw body */ }

        return body.Length > 200 ? body[..200] + "…" : body;
    }
}
