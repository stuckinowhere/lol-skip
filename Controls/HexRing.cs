using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Unqueued.Controls;

public sealed class HexRing : Control
{
    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<HexRing, IBrush?>(nameof(Stroke), new SolidColorBrush(Color.Parse("#C8AA6E")));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<HexRing, double>(nameof(StrokeThickness), 1.2);

    static HexRing()
    {
        AffectsRender<HexRing>(StrokeProperty, StrokeThicknessProperty);
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

    public override void Render(DrawingContext context)
    {
        if (Stroke is null || Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        DrawHex(context, Math.Min(Bounds.Width, Bounds.Height) / 2 - 3, StrokeThickness);
        DrawHex(context, Math.Min(Bounds.Width, Bounds.Height) / 2 - 14, 0.6);
    }

    private void DrawHex(DrawingContext context, double radius, double thickness)
    {
        if (radius <= 4)
            return;

        var cx = Bounds.Width / 2;
        var cy = Bounds.Height / 2;
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            for (var i = 0; i < 6; i++)
            {
                var angle = Math.PI / 180 * (60 * i - 90);
                var point = new Point(cx + radius * Math.Cos(angle), cy + radius * Math.Sin(angle));
                if (i == 0)
                    ctx.BeginFigure(point, false);
                else
                    ctx.LineTo(point);
            }

            ctx.EndFigure(true);
        }

        var brush = Stroke;
        if (brush is ISolidColorBrush solid)
            brush = new SolidColorBrush(solid.Color, 0.85);

        context.DrawGeometry(null, new Pen(brush, thickness), geometry);
    }
}
