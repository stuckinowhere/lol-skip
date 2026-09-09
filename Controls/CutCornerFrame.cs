using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Unqueued.Controls;

public sealed class CutCornerFrame : Decorator
{
    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<CutCornerFrame, IBrush?>(nameof(Stroke), new SolidColorBrush(Color.Parse("#C8AA6E")));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<CutCornerFrame, double>(nameof(StrokeThickness), 1.0);

    public static readonly StyledProperty<double> CutSizeProperty =
        AvaloniaProperty.Register<CutCornerFrame, double>(nameof(CutSize), 14.0);

    static CutCornerFrame()
    {
        AffectsRender<CutCornerFrame>(StrokeProperty, StrokeThicknessProperty, CutSizeProperty);
    }

    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public double CutSize
    {
        get => GetValue(CutSizeProperty);
        set => SetValue(CutSizeProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0 || Stroke is null)
            return;

        var cut = Math.Min(CutSize, Math.Min(w, h) / 3);
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(cut, 0.5), false);
            ctx.LineTo(new Point(w - cut, 0.5));
            ctx.LineTo(new Point(w - 0.5, cut));
            ctx.LineTo(new Point(w - 0.5, h - cut));
            ctx.LineTo(new Point(w - cut, h - 0.5));
            ctx.LineTo(new Point(cut, h - 0.5));
            ctx.LineTo(new Point(0.5, h - cut));
            ctx.LineTo(new Point(0.5, cut));
            ctx.EndFigure(true);
        }

        context.DrawGeometry(null, new Pen(Stroke, StrokeThickness), geometry);
    }
}
