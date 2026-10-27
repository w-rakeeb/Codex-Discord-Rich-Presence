using System.Net;
using System.Text;
using CodexPresence;

var passed = 0;
void Check(bool ok, string name) { if (!ok) throw new Exception(name); passed++; }
string Release(string tag = "desktop-v3.4.0", bool prerelease = false, bool draft = false) => $$"""{"tag_name":"{{tag}}","prerelease":{{prerelease.ToString().ToLowerInvariant()}},"draft":{{draft.ToString().ToLowerInvariant()}},"html_url":"https://evil.invalid/ignored"}""";
async Task<UpdateResult> Query(HttpStatusCode code, string json, Action<HttpRequestMessage>? inspect = null)
{
    using var client = new HttpClient(new FakeHandler(request => { inspect?.Invoke(request); return new HttpResponseMessage(code) { Content = new StringContent(json, Encoding.UTF8, "application/json") }; }));
    return await new ReleaseUpdates(client).CheckAsync(new Version(3, 3, 0));
}
var newer = await Query(HttpStatusCode.OK, Release(), request => {
    Check(request.RequestUri!.AbsoluteUri == "https://api.github.com/repos/w-rakeeb/Codex-Discord-Rich-Presence/releases/latest", "Only the product repository is queried");
    Check(request.Headers.UserAgent.Count > 0, "GitHub request has a User-Agent");
    Check(request.Headers.Authorization == null, "No local credentials leave the machine");
});
Check(newer.Available && newer.Version == "3.4.0", "A newer stable release is available");
Check(newer.Url == "https://github.com/w-rakeeb/Codex-Discord-Rich-Presence/releases/tag/desktop-v3.4.0", "Untrusted response links are ignored");
foreach (var tag in new[] { "desktop-v3.3.0", "v3.2.9" }) Check(!(await Query(HttpStatusCode.OK, Release(tag))).Available, "Current and older releases are not updates");
foreach (var tag in new[] { "../../evil", "v3.4.0-preview", "desktop-v9999999999.0.0", "v3.4" }) Check(!(await Query(HttpStatusCode.OK, Release(tag))).Available, "Invalid version rejected: " + tag);
Check(!(await Query(HttpStatusCode.OK, Release(prerelease:true))).Available, "Preview release excluded");
Check(!(await Query(HttpStatusCode.OK, Release(draft:true))).Available, "Draft release excluded");
Check((await Query(HttpStatusCode.NotFound, "")).Message.Contains("No release"), "Empty new repository has an honest status");
foreach (var code in new[] { HttpStatusCode.Forbidden, HttpStatusCode.TooManyRequests }) Check((await Query(code, "")).Message.Contains("limit"), "Rate limit has friendly feedback");
foreach (var json in new[] { "broken", "[]", "null", "{}", new string('x', 140000) }) Check(!(await Query(HttpStatusCode.OK, json)).Available, "Malformed/oversized response handled");
using (var client = new HttpClient(new FakeHandler(_ => throw new HttpRequestException("private transport details"))))
    Check(!(await new ReleaseUpdates(client).CheckAsync(new Version(3,3,0))).Message.Contains("private"), "Network errors do not expose raw exceptions");
using (var client = new HttpClient(new FakeHandler(_ => throw new TaskCanceledException())))
    Check((await new ReleaseUpdates(client).CheckAsync(new Version(3,3,0))).Message.Contains("timed out"), "Timeout has retry guidance");
Console.WriteLine($"Update checks: {passed} passed.");
if (args.Contains("--live")) { var result = await new ReleaseUpdates().CheckAsync(new Version(3,3,0)); Console.WriteLine("Live GitHub check: " + result.Message); }
sealed class FakeHandler(Func<HttpRequestMessage,HttpResponseMessage> response) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(response(request));
}
