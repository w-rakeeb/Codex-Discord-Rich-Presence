using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CodexPresence;

public sealed record UpdateResult(string Message, bool Available = false, string? Version = null, string? Url = null);

public sealed class ReleaseUpdates
{
    public const string Repository = "w-rakeeb/Codex-Discord-Rich-Presence";
    public const string ProjectUrl = "https://github.com/" + Repository;
    private static readonly HttpClient SharedClient = new(new HttpClientHandler { AllowAutoRedirect = false });
    private readonly HttpClient client;

    public ReleaseUpdates(HttpClient? client = null) => this.client = client ?? SharedClient;

    public async Task<UpdateResult> CheckAsync(Version installed)
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(12));
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/" + Repository + "/releases/latest");
            request.Headers.UserAgent.ParseAdd("Codex-Discord-Rich-Presence/" + installed.ToString(3));
            request.Headers.Accept.ParseAdd("application/vnd.github+json");
            request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound) return new("No release is published yet. Try again later.");
            if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests) return new("GitHub's request limit was reached. Try again later.");
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength > 131072) return new("GitHub returned an unexpected response. Try again later.");
            using var input = await response.Content.ReadAsStreamAsync(timeout.Token).ConfigureAwait(false);
            using var output = new MemoryStream();
            var buffer = new byte[8192];
            int count;
            while ((count = await input.ReadAsync(buffer, timeout.Token).ConfigureAwait(false)) > 0)
            {
                if (output.Length + count > 131072) return new("GitHub returned an unexpected response. Try again later.");
                await output.WriteAsync(buffer.AsMemory(0, count), timeout.Token).ConfigureAwait(false);
            }
            using var json = JsonDocument.Parse(output.ToArray());
            var release = json.RootElement;
            if (release.GetProperty("draft").GetBoolean() || release.GetProperty("prerelease").GetBoolean()) return new("No stable update is available.");
            var tag = release.GetProperty("tag_name").GetString() ?? "";
            var number = tag.StartsWith("desktop-v", StringComparison.Ordinal) ? tag[9..] : tag.StartsWith('v') ? tag[1..] : "";
            if (!System.Version.TryParse(number, out var version) || version.Build < 0 || version.Revision >= 0) return new("GitHub returned an unrecognized version. Open the project page to review releases.");
            if (version <= new Version(installed.Major, installed.Minor, installed.Build)) return new("You're up to date. Version " + installed.ToString(3) + " is installed.");
            return new("Version " + version.ToString(3) + " is available. Review the release before updating.", true, version.ToString(3), ProjectUrl + "/releases/tag/" + tag);
        }
        catch (OperationCanceledException) { return new("The update check timed out. Check your connection and try again."); }
        catch (HttpRequestException) { return new("Couldn't reach GitHub. Check your connection and try again."); }
        catch (Exception error) when (error is JsonException or InvalidOperationException or System.Collections.Generic.KeyNotFoundException or FormatException or IOException)
        { return new("GitHub returned an unexpected response. Try again later."); }
    }
}
