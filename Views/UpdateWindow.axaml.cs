using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Unqueued.Controls;
using Unqueued.Services;

namespace Unqueued.Views;

public partial class UpdateWindow : Window
{
    private readonly UpdateCheckResult _result;

    public UpdateWindow() : this(new UpdateCheckResult(
        UpdateCheckStatus.Current,
        new Version(1, 0, 0, 0),
        new Version(1, 0, 0, 0),
        "v1.0.0",
        null,
        null,
        null))
    {
        // Design-time.
    }

    internal UpdateWindow(UpdateCheckResult result)
    {
        _result = result;
        InitializeComponent();
        AppIcons.ApplyToWindow(this);
        NativeWindowIcon.Bind(this);
        SizeChanged += (_, _) => ClipToCutCorners();
        Opened += (_, _) => ClipToCutCorners();
        ApplyResult();
    }

    private void ApplyResult()
    {
        switch (_result.Status)
        {
            case UpdateCheckStatus.Available:
                Eyebrow.Text = "UPDATE AVAILABLE";
                Body.Text = $"wasdlol skip {_result.Tag} is ready. Download the setup and run it — the installer will close this copy first.";
                PrimaryButton.Content = "Download";
                LaterButton.IsVisible = true;
                break;
            case UpdateCheckStatus.Failed:
                Eyebrow.Text = "UPDATE CHECK";
                Body.Text = _result.Error ?? "Could not check for updates. Try again when you are online.";
                PrimaryButton.Content = "OK";
                LaterButton.IsVisible = false;
                break;
            default:
                Eyebrow.Text = "UP TO DATE";
                Body.Text = $"You are on {_result.Current.ToString(3)}.";
                PrimaryButton.Content = "OK";
                LaterButton.IsVisible = false;
                break;
        }
    }

    private void ClipToCutCorners()
    {
        var cut = Shell?.CutSize ?? 16;
        Clip = CutCornerFrame.CreateGeometry(Bounds.Width, Bounds.Height, cut);
    }

    private void OnChromePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && e.ClickCount == 1)
            BeginMoveDrag(e);
    }

    private void OnPrimaryClick(object? sender, RoutedEventArgs e)
    {
        if (_result.Status == UpdateCheckStatus.Available)
        {
            var url = _result.DownloadUrl ?? _result.ReleaseUrl;
            if (!string.IsNullOrWhiteSpace(url))
                GitHubUpdateClient.OpenUrl(url);
        }

        Close();
    }

    private void OnLaterClick(object? sender, RoutedEventArgs e) => Close();
}
