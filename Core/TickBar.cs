using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Hub;

/// <summary>
/// Hedefi tek tek çentiklerle gösteren ilerleme çubuğu (ör. 45 commit = 45 çentik).
/// Dolu çentikler aşağıdan yukarı dolar; <see cref="Pulse"/> açıkken sıradaki çentik yanıp söner.
/// </summary>
public sealed class TickBar : FrameworkElement
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(double), typeof(TickBar),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum), typeof(int), typeof(TickBar),
        new FrameworkPropertyMetadata(45, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
        nameof(Fill), typeof(Brush), typeof(TickBar),
        new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TrackProperty = DependencyProperty.Register(
        nameof(Track), typeof(Brush), typeof(TickBar),
        new FrameworkPropertyMetadata(Brushes.DimGray, FrameworkPropertyMetadataOptions.AffectsRender));

    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public int Maximum { get => (int)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    public Brush Fill { get => (Brush)GetValue(FillProperty); set => SetValue(FillProperty, value); }
    public Brush Track { get => (Brush)GetValue(TrackProperty); set => SetValue(TrackProperty, value); }

    public double Gap { get; set; } = 5;

    double _target;
    AnimationClock? _pulse;

    public bool Pulse
    {
        get => _pulse != null;
        set
        {
            if (value == Pulse) return;
            _pulse?.Controller?.Stop();
            _pulse = value
                ? new DoubleAnimation(0.9, 0.12, TimeSpan.FromMilliseconds(700))
                {
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
                }.CreateClock()
                : null;
            InvalidateVisual();
        }
    }

    public void AnimateTo(double value, double ms = 1300)
    {
        _target = value;
        BeginAnimation(ValueProperty, new DoubleAnimation(value, TimeSpan.FromMilliseconds(ms)) { EasingFunction = Motion.Ease });
        if (Pulse) InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        var n = Math.Max(1, Maximum);
        var h = ActualHeight;
        var w = (ActualWidth - Gap * (n - 1)) / n;
        if (w <= 0 || h <= 0) return;
        var r = Math.Min(2, w / 2);

        for (var i = 0; i < n; i++)
        {
            var x = i * (w + Gap);
            dc.DrawRoundedRectangle(Track, null, new Rect(x, 0, w, h), r, r);

            var fill = Math.Clamp(Value - i, 0, 1);
            if (fill > 0)
                dc.DrawRoundedRectangle(Fill, null, new Rect(x, h * (1 - fill), w, h * fill), r, r);
            else if (_pulse != null && i == (int)Math.Round(_target))
            {
                dc.PushOpacity(0, _pulse);
                dc.DrawRoundedRectangle(Fill, null, new Rect(x, 0, w, h), r, r);
                dc.Pop();
            }
        }
    }
}
