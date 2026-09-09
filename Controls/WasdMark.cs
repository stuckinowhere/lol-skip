using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Unqueued.Controls;

/// <summary>
/// Portable WASD house mark: hex + WASD key cluster.
/// Copy this file into other apps (wasdlol, etc.) with the Brand palette.
/// </summary>
public sealed class WasdMark : Control
{
    public static readonly StyledProperty<IBrush?> HexFillProperty =
        AvaloniaProperty.Register<WasdMark, IBrush?>(nameof(HexFill), new SolidColorBrush(Color.Parse("#010A13")));

    public static readonly StyledProperty<IBrush?> AccentProperty =
        AvaloniaProperty.Register<WasdMark, IBrush?>(nameof(Accent), new SolidColorBrush(Color.Parse("#C8AA6E")));

    public static readonly StyledProperty<IBrush?> KeyFillProperty =
        AvaloniaProperty.Register<WasdMark, IBrush?>(nameof(KeyFill), new SolidColorBrush(Color.Parse("#16110A")));

    static WasdMark()
    {
        AffectsRender<WasdMark>(HexFillProperty, AccentProperty, KeyFillProperty);
        AffectsMeasure<WasdMark>(WidthProperty, HeightProperty);
    }

    public IBrush? HexFill
    {
        get => GetValue(HexFillProperty);
        set => SetValue(HexFillProperty, value);
    }

    public IBrush? Accent
    {
        get => GetValue(AccentProperty);
        set => SetValue(AccentProperty, value);
    }

    public IBrush? KeyFill
    {
        get => GetValue(KeyFillProperty);
        set => SetValue(KeyFillProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var side = double.IsNaN(Width) ? 24 : Width;
        var height = double.IsNaN(Height) ? side : Height;
        return new Size(side, height);
    }

    public override void Render(DrawingContext context)
    {
        var size = Math.Min(Bounds.Width, Bounds.Height);
        if (size <= 1)
            return;

        var cx = Bounds.Width / 2;
        var cy = Bounds.Height / 2;
        var radius = size / 2 - Math.Max(1.2, size * 0.04);
        var hex = HexGeometry(cx, cy, radius);
        context.DrawGeometry(HexFill, new Pen(Accent, Math.Max(1.1, size * 0.055)), hex);
        DrawLetterL(context, cx, cy, radius);
    }

    private void DrawLetterL(DrawingContext context, double cx, double cy, double radius)
    {
        var geo = LetterLGeometry(cx, cy, radius);
        var fill = Accent ?? new SolidColorBrush(Color.Parse("#C8AA6E"));
        var stroke = new Pen(new SolidColorBrush(Color.Parse("#F0E6D2")), Math.Max(0.6, radius * 0.035));
        context.DrawGeometry(fill, stroke, geo);
    }

    private static StreamGeometry LetterLGeometry(double cx, double cy, double radius)
    {
        var left = cx - radius * 0.30;
        var top = cy - radius * 0.40;
        var bottom = cy + radius * 0.38;
        var stem = radius * 0.24;
        var foot = radius * 0.58;
        var thick = radius * 0.22;
        var cut = radius * 0.09;

        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        ctx.BeginFigure(new Point(left, top), true);
        ctx.LineTo(new Point(left + stem, top));
        ctx.LineTo(new Point(left + stem, bottom - thick));
        ctx.LineTo(new Point(left + foot - cut, bottom - thick));
        ctx.LineTo(new Point(left + foot, bottom - thick + cut));
        ctx.LineTo(new Point(left + foot, bottom));
        ctx.LineTo(new Point(left, bottom));
        ctx.EndFigure(true);
        return geometry;
    }

    private static StreamGeometry HexGeometry(double cx, double cy, double radius)
    {
        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        for (var i = 0; i < 6; i++)
        {
            var angle = Math.PI / 180 * (60 * i - 90);
            var point = new Point(cx + radius * Math.Cos(angle), cy + radius * Math.Sin(angle));
            if (i == 0)
                ctx.BeginFigure(point, true);
            else
                ctx.LineTo(point);
        }

        ctx.EndFigure(true);
        return geometry;
    }
}
