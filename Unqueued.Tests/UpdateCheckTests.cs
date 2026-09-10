using System.Net;
using Unqueued.Services;

namespace Unqueued.Tests;

public class UpdateCheckTests
{
    private const string LatestJson = """
        {
          "tag_name": "v1.2.0",
          "html_url": "https://github.com/stuckinowhere/lol-skip/releases/tag/v1.2.0",
          "assets": [
            {
              "name": "wasdlol-skip-v1.2.0-win-x64.zip",
              "browser_download_url": "https://github.com/stuckinowhere/lol-skip/releases/download/v1.2.0/skip.zip"
            },
            {
              "name": "wasdlol-skip-v1.2.0-win-x64-setup.exe",
              "browser_download_url": "https://github.com/stuckinowhere/lol-skip/releases/download/v1.2.0/skip-setup.exe"
            }
          ]
        }
        """;

    [Theory]
    [InlineData("v1.2.0", 1, 2, 0)]
    [InlineData("1.2.0", 1, 2, 0)]
    [InlineData("v1.2.0.1", 1, 2, 0)]
    public void ParsesReleaseTags(string tag, int major, int minor, int build)
    {
        Assert.True(AppVersion.TryParseTag(tag, out var version));
        Assert.Equal(new Version(major, minor, build, tag == "v1.2.0.1" ? 1 : 0), version);
    }

    [Fact]
    public void ThreePartTagMatchesFourPartAssemblyVersion()
    {
        var fromTag = AppVersion.Normalize(new Version(1, 2, 0));
        var fromAssembly = new Version(1, 2, 0, 0);
        Assert.Equal(0, fromTag.CompareTo(fromAssembly));
    }

    [Fact]
    public void Parse_PrefersSetupExe_WhenNewer()
    {
        var result = GitHubUpdateClient.Parse(LatestJson, new Version(1, 1, 0, 0));

        Assert.Equal(UpdateCheckStatus.Available, result.Status);
        Assert.Equal(new Version(1, 2, 0, 0), result.Latest);
        Assert.Equal("https://github.com/stuckinowhere/lol-skip/releases/download/v1.2.0/skip-setup.exe", result.DownloadUrl);
    }

    [Fact]
    public void Parse_SameTag_IsCurrent()
    {
        var result = GitHubUpdateClient.Parse(LatestJson, new Version(1, 2, 0, 0));
        Assert.Equal(UpdateCheckStatus.Current, result.Status);
    }

    [Fact]
    public void Parse_OlderGitHubTag_IsCurrent()
    {
        var json = """{ "tag_name": "v1.0.0", "html_url": "https://github.com/stuckinowhere/lol-skip/releases/tag/v1.0.0", "assets": [] }""";
        var result = GitHubUpdateClient.Parse(json, new Version(1, 2, 0, 0));
        Assert.Equal(UpdateCheckStatus.Current, result.Status);
        Assert.Equal("https://github.com/stuckinowhere/lol-skip/releases/tag/v1.0.0", result.DownloadUrl);
    }

    [Theory]
    [InlineData("https://github.com/stuckinowhere/lol-skip/releases/tag/v1.2.1", true)]
    [InlineData("https://objects.githubusercontent.com/github-production-release-asset/foo", true)]
    [InlineData("http://github.com/stuckinowhere/lol-skip/releases/tag/v1.2.1", false)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("https://evil.example/setup.exe", false)]
    [InlineData("https://github.com.evil.test/setup.exe", false)]
    public void ReleaseUrlsMustBeGithubHttps(string url, bool allowed) =>
        Assert.Equal(allowed, GitHubUpdateClient.IsAllowedReleaseUrl(url));

    [Fact]
    public void Parse_DropsNonGithubAssetUrls()
    {
        var json = """
            {
              "tag_name": "v9.0.0",
              "html_url": "https://evil.example/pwn",
              "assets": [
                {
                  "name": "wasdlol-skip-v9.0.0-win-x64-setup.exe",
                  "browser_download_url": "https://evil.example/skip-setup.exe"
                }
              ]
            }
            """;

        var result = GitHubUpdateClient.Parse(json, new Version(1, 0, 0, 0));
        Assert.Equal(UpdateCheckStatus.Available, result.Status);
        Assert.Null(result.DownloadUrl);
        Assert.Null(result.ReleaseUrl);
    }

    [Fact]
    public async Task CheckAsync_ReadsLatestReleaseJson()
    {
        var handler = new StubHandler(LatestJson);
        using var client = new GitHubUpdateClient(handler, new Version(1, 1, 0, 0));

        var result = await client.CheckAsync();

        Assert.Equal(UpdateCheckStatus.Available, result.Status);
        Assert.Equal(GitHubUpdateClient.LatestReleaseUrl, handler.LastUrl);
        Assert.Contains("WasdLolSkip", handler.UserAgent, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CheckAsync_HttpError_IsFailed()
    {
        var handler = new StubHandler("nope", HttpStatusCode.NotFound);
        using var client = new GitHubUpdateClient(handler, new Version(1, 1, 0, 0));

        var result = await client.CheckAsync();

        Assert.Equal(UpdateCheckStatus.Failed, result.Status);
        Assert.Contains("404", result.Error);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly string _body;
        private readonly HttpStatusCode _status;

        public StubHandler(string body, HttpStatusCode status = HttpStatusCode.OK)
        {
            _body = body;
            _status = status;
        }

        public string? LastUrl { get; private set; }

        public string UserAgent { get; private set; } = "";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastUrl = request.RequestUri?.ToString();
            UserAgent = request.Headers.UserAgent.ToString();
            return Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body)
            });
        }
    }
}
