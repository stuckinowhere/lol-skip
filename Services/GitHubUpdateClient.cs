using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace Unqueued.Services;

internal enum UpdateCheckStatus
{
    Current,
    Available,
    Failed
}

internal sealed record UpdateCheckResult(
    UpdateCheckStatus Status,
    Version Current,
    Version? Latest,
    string? Tag,
    string? DownloadUrl,
    string? ReleaseUrl,
    string? Error);

internal static class AppVersion
{
    public static Version Current
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
            return Normalize(version);
        }
    }

    internal static Version Normalize(Version version) =>
        new(
            version.Major,
            version.Minor,
            version.Build < 0 ? 0 : version.Build,
            version.Revision < 0 ? 0 : version.Revision);

    internal static bool TryParseTag(string? tag, out Version version)
    {
        version = new Version(0, 0, 0, 0);
        if (string.IsNullOrWhiteSpace(tag))
            return false;

        var trimmed = tag.Trim();
        if (trimmed.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            trimmed = trimmed[1..];

        if (!Version.TryParse(trimmed, out var parsed))
            return false;

        version = Normalize(parsed);
        return true;
    }
}

internal sealed class GitHubUpdateClient : IDisposable
{
    public const string LatestReleaseUrl = "https://api.github.com/repos/stuckinowhere/lol-skip/releases/latest";
    internal const int MaxReleaseJsonBytes = 256 * 1024;

    private static readonly string[] AllowedHosts =
    [
        "github.com",
        "www.github.com"
    ];

    private readonly HttpClient _http;
    private readonly Version _current;
    private readonly bool _ownsHttp;

    public GitHubUpdateClient(HttpMessageHandler? handler = null, Version? current = null)
    {
        _current = AppVersion.Normalize(current ?? AppVersion.Current);
        if (handler is null)
        {
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            _ownsHttp = true;
        }
        else
        {
            _http = new HttpClient(handler, disposeHandler: false) { Timeout = TimeSpan.FromSeconds(8) };
            _ownsHttp = true;
        }

        _http.MaxResponseContentBufferSize = MaxReleaseJsonBytes;
        _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("WasdLolSkip", _current.ToString()));
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
    }

    public async Task<UpdateCheckResult> CheckAsync(CancellationToken cancellation = default)
    {
        try
        {
            using var response = await _http.GetAsync(LatestReleaseUrl, cancellation).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellation).ConfigureAwait(false);
            if (body.Length > MaxReleaseJsonBytes)
            {
                return new UpdateCheckResult(
                    UpdateCheckStatus.Failed,
                    _current,
                    null,
                    null,
                    null,
                    null,
                    "GitHub release payload was too large.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return new UpdateCheckResult(
                    UpdateCheckStatus.Failed,
                    _current,
                    null,
                    null,
                    null,
                    null,
                    $"GitHub returned {(int)response.StatusCode}.");
            }

            return Parse(body, _current);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return new UpdateCheckResult(
                UpdateCheckStatus.Failed,
                _current,
                null,
                null,
                null,
                null,
                "Could not reach GitHub Releases.");
        }
    }

    internal static UpdateCheckResult Parse(string json, Version current)
    {
        current = AppVersion.Normalize(current);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var tag = root.TryGetProperty("tag_name", out var tagEl) ? tagEl.GetString() : null;
        if (!AppVersion.TryParseTag(tag, out var latest))
        {
            return new UpdateCheckResult(
                UpdateCheckStatus.Failed,
                current,
                null,
                tag,
                null,
                null,
                "Latest release has no version tag.");
        }

        var releaseUrl = AllowedUrl(root.TryGetProperty("html_url", out var htmlEl) ? htmlEl.GetString() : null);
        string? setupUrl = null;
        string? zipUrl = null;
        if (root.TryGetProperty("assets", out var assets) && assets.ValueKind == JsonValueKind.Array)
        {
            foreach (var asset in assets.EnumerateArray())
            {
                var name = asset.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;
                var url = AllowedUrl(asset.TryGetProperty("browser_download_url", out var urlEl) ? urlEl.GetString() : null);
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
                    continue;

                if (name.EndsWith("-setup.exe", StringComparison.OrdinalIgnoreCase)
                    || name.EndsWith("setup.exe", StringComparison.OrdinalIgnoreCase))
                    setupUrl ??= url;
                else if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    zipUrl ??= url;
            }
        }

        var download = setupUrl ?? zipUrl ?? releaseUrl;
        var available = latest > current;
        return new UpdateCheckResult(
            available ? UpdateCheckStatus.Available : UpdateCheckStatus.Current,
            current,
            latest,
            tag,
            download,
            releaseUrl,
            null);
    }

    internal static string? AllowedUrl(string? url) =>
        IsAllowedReleaseUrl(url) ? url : null;

    internal static bool IsAllowedReleaseUrl(string? url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;
        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            return false;
        if (!string.IsNullOrEmpty(uri.UserInfo))
            return false;

        var host = uri.IdnHost;
        if (string.IsNullOrWhiteSpace(host))
            return false;

        if (AllowedHosts.Any(allowed => host.Equals(allowed, StringComparison.OrdinalIgnoreCase)))
            return true;

        return host.EndsWith(".githubusercontent.com", StringComparison.OrdinalIgnoreCase);
    }

    public static bool OpenUrl(string url)
    {
        if (!IsAllowedReleaseUrl(url))
            return false;

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
        return true;
    }

    public void Dispose()
    {
        if (_ownsHttp)
            _http.Dispose();
    }
}
