using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Unqueued.Controls;

public sealed class CutCornerFrame : Decorator
{
    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<CutCornerFrame, IBrush?>(nameof(Stroke), new SolidColorBrush(Color.Parse("#0AC8B9")));

    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<CutCornerFrame, IBrush?>(nameof(Fill));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<CutCornerFrame, double>(nameof(StrokeThickness), 1.0);

    public static readonly StyledProperty<double> CutSizeProperty =
        AvaloniaProperty.Register<CutCornerFrame, double>(nameof(CutSize), 14.0);

    public static readonly StyledProperty<bool> ShowCornerMarksProperty =
        AvaloniaProperty.Register<CutCornerFrame, bool>(nameof(ShowCornerMarks), true);

    static CutCornerFrame()
    {
        AffectsRender<CutCornerFrame>(StrokeProperty, FillProperty, StrokeThicknessProperty, CutSizeProperty, ShowCornerMarksProperty);
    }

    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
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

    public bool ShowCornerMarks
    {
        get => GetValue(ShowCornerMarksProperty);
        set => SetValue(ShowCornerMarksProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0)
            return;

        var cut = Math.Min(CutSize, Math.Min(w, h) / 3);
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(cut, 0.5), true);
            ctx.LineTo(new Point(w - cut, 0.5));
            ctx.LineTo(new Point(w - 0.5, cut));
            ctx.LineTo(new Point(w - 0.5, h - cut));
            ctx.LineTo(new Point(w - cut, h - 0.5));
            ctx.LineTo(new Point(cut, h - 0.5));
            ctx.LineTo(new Point(0.5, h - cut));
            ctx.LineTo(new Point(0.5, cut));
            ctx.EndFigure(true);
        }

        if (Fill is not null)
            context.DrawGeometry(Fill, null, geometry);

        if (Stroke is not null)
            context.DrawGeometry(null, new Pen(Stroke, StrokeThickness), geometry);

        if (!ShowCornerMarks || Stroke is null)
            return;

        DrawDiamond(context, cut, cut * 0.45);
        DrawDiamond(context, w - cut, cut * 0.45);
        DrawDiamond(context, cut, h - cut * 0.45);
        DrawDiamond(context, w - cut, h - cut * 0.45);
    }

    private void DrawDiamond(DrawingContext context, double x, double y)
    {
        const double r = 3.2;
        var diamond = new StreamGeometry();
        using (var ctx = diamond.Open())
        {
            ctx.BeginFigure(new Point(x, y - r), true);
            ctx.LineTo(new Point(x + r, y));
            ctx.LineTo(new Point(x, y + r));
            ctx.LineTo(new Point(x - r, y));
            ctx.EndFigure(true);
        }

        context.DrawGeometry(Stroke, new Pen(Stroke, 0.8), diamond);
    }
}
