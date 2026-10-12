using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Hub;

/// <summary>
/// Sayfa açılış animasyonu: XAML'da <c>hub:Motion.Reveal="True"</c> verilen öğeler
/// <see cref="Play"/> ile belge sırasıyla, kademeli olarak hafifçe yukarı kayarak belirir.
/// </summary>
public static class Motion
{
    public static readonly DependencyProperty RevealProperty = DependencyProperty.RegisterAttached(
        "Reveal", typeof(bool), typeof(Motion), new PropertyMetadata(false));

    public static bool GetReveal(DependencyObject o) => (bool)o.GetValue(RevealProperty);
    public static void SetReveal(DependencyObject o, bool v) => o.SetValue(RevealProperty, v);

    public static readonly IEasingFunction Ease = new QuinticEase { EasingMode = EasingMode.EaseOut };
    static readonly TimeSpan Step = TimeSpan.FromMilliseconds(45);
    static readonly TimeSpan Length = TimeSpan.FromMilliseconds(600);

    public static void Play(DependencyObject root)
    {
        var items = new List<FrameworkElement>();
        Collect(root, items);
        for (var i = 0; i < items.Count; i++)
        {
            var el = items[i];
            if (el.RenderTransform is not TranslateTransform t)
                el.RenderTransform = t = new TranslateTransform();
            var delay = TimeSpan.FromTicks(Step.Ticks * i);
            t.BeginAnimation(TranslateTransform.YProperty, Hold(14, 0, delay));
            el.BeginAnimation(UIElement.OpacityProperty, Hold(0, 1, delay));
        }
    }

    static void Collect(DependencyObject node, List<FrameworkElement> into)
    {
        if (node is FrameworkElement fe && GetReveal(fe)) into.Add(fe);
        foreach (var child in LogicalTreeHelper.GetChildren(node))
            if (child is DependencyObject d) Collect(d, into);
    }

    /// <summary>Gecikme boyunca başlangıç değerinde bekler, sonra yumuşakça hedefe gider.</summary>
    static DoubleAnimationUsingKeyFrames Hold(double from, double to, TimeSpan delay) => new()
    {
        KeyFrames =
        {
            new DiscreteDoubleKeyFrame(from, KeyTime.FromTimeSpan(TimeSpan.Zero)),
            new DiscreteDoubleKeyFrame(from, KeyTime.FromTimeSpan(delay)),
            new EasingDoubleKeyFrame(to, KeyTime.FromTimeSpan(delay + Length), Ease),
        },
    };

    // ------------------------------------------------------------------ sayı sayacı

    static readonly DependencyProperty CountProperty = DependencyProperty.RegisterAttached(
        "Count", typeof(double), typeof(Motion),
        new PropertyMetadata(0.0, (o, e) => ((TextBlock)o).Text = Math.Round((double)e.NewValue).ToString(CultureInfo.InvariantCulture)));

    /// <summary>TextBlock'taki tamsayıyı mevcut değerden hedefe sayarak götürür.</summary>
    public static void CountTo(TextBlock tb, int value) =>
        tb.BeginAnimation(CountProperty, new DoubleAnimation(value, TimeSpan.FromMilliseconds(1000)) { EasingFunction = Ease });

    /// <summary>Durumu yüklenene kadar gizli tutulan kontrolü yumuşakça gösterir.</summary>
    public static void FadeIn(UIElement el) =>
        el.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(250)));

    // ------------------------------------------------------------------ nabız

    /// <summary>Öğenin saydamlığını yavaşça atar (ör. "çalışıyor" noktası); false durdurur.</summary>
    public static void Pulse(UIElement el, bool on)
    {
        if (!on)
        {
            el.BeginAnimation(UIElement.OpacityProperty, null);
            return;
        }
        el.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, 0.25, TimeSpan.FromMilliseconds(800))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
        });
    }
}
