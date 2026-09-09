using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Unqueued.Controls;

namespace Unqueued.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AppIcons.ApplyToWindow(this);
        NativeWindowIcon.Bind(this);
        SizeChanged += (_, _) => ClipToCutCorners();
        Opened += (_, _) => ClipToCutCorners();
        PropertyChanged += OnWindowPropertyChanged;
    }

    private void ClipToCutCorners()
    {
        var cut = Shell?.CutSize ?? 22;
        Clip = CutCornerFrame.CreateGeometry(Bounds.Width, Bounds.Height, cut);
    }

    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty && WindowState == WindowState.Maximized)
            WindowState = WindowState.Normal;
    }

    private void OnMinimizeClick(object? sender, RoutedEventArgs e)
    {
        ShowInTaskbar = true;
        WindowState = WindowState.Minimized;
    }

    private void OnChromePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && e.ClickCount == 1)
            BeginMoveDrag(e);
    }
}
