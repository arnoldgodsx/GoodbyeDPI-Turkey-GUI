using System.Windows;
using System.Windows.Controls;

namespace Hub.Modules.Activity;

public partial class ActivityPage : UserControl
{
    readonly ActivityEngine _engine;

    public ActivityPage(ActivityEngine engine)
    {
        InitializeComponent();
        _engine = engine;
        TargetText.Text = $"/ {ActivityEngine.Target}";
        Ticks.Maximum = ActivityEngine.Target;

        _engine.Changed += s => Dispatcher.BeginInvoke(() => Render(s));
        Render(_engine.Current);
        _ = LoadCloudAsync();
    }

    void Render(Status s)
    {
        Motion.CountTo(CountText, s.Today);
        Ticks.AnimateTo(s.Today);
        // Çalışırken sıradaki çentik atar; ayrı bir durum yazısı yok.
        Ticks.Pulse = s.State == EngineState.Working;

        // Hata sadece bir sonraki başarılı tura kadar görünür; çalışırken önceki hata yerinde kalır.
        if (s.State != EngineState.Working)
        {
            ErrorText.Text = s.Message;
            ErrorText.Visibility = s.State == EngineState.Error ? Visibility.Visible : Visibility.Collapsed;
        }

        LastText.Text = s.LastCheck?.ToString("HH:mm") ?? "—";
        NextText.Text = s.NextCheck?.ToString("HH:mm") ?? "—";
    }

    async Task LoadCloudAsync()
    {
        try
        {
            CloudSwitch.IsChecked = await CloudBackup.IsEnabledAsync();
            CloudSwitch.IsHitTestVisible = true;
        }
        catch
        {
            CloudSwitch.ToolTip = "GitHub'a ulaşılamadı";
        }
        Motion.FadeIn(CloudSwitch);
    }

    async void OnCloudToggle(object sender, RoutedEventArgs e)
    {
        var want = CloudSwitch.IsChecked == true;
        CloudSwitch.IsHitTestVisible = false;
        try { await CloudBackup.SetAsync(want); }
        catch (Exception ex)
        {
            CloudSwitch.IsChecked = !want;
            CloudSwitch.ToolTip = ex.Message;
        }
        CloudSwitch.IsHitTestVisible = true;
    }

    void OnCheck(object sender, RoutedEventArgs e) => _engine.CheckNow();
}
