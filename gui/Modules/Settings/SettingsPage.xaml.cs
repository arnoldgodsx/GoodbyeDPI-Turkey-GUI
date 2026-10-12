using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Hub.Modules.Settings;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
        _ = LoadAsync();
    }

    async Task LoadAsync()
    {
        StartupSwitch.IsChecked = await Task.Run(() => Elevation.AutostartEnabled);
        StartupSwitch.IsHitTestVisible = true;
        Motion.FadeIn(StartupSwitch);
    }

    async void OnStartupToggle(object sender, RoutedEventArgs e)
    {
        var want = StartupSwitch.IsChecked == true;
        StartupSwitch.IsHitTestVisible = false;
        ErrorText.Visibility = Visibility.Collapsed;
        try { await Task.Run(() => Elevation.SetAutostart(want)); }
        catch (Exception ex)
        {
            StartupSwitch.IsChecked = !want;
            ErrorText.Text = ex.Message;
            ErrorText.Visibility = Visibility.Visible;
        }
        StartupSwitch.IsHitTestVisible = true;
    }

    void OnOpenFolder(object sender, RoutedEventArgs e) =>
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{AppContext.BaseDirectory}\"") { UseShellExecute = true });

    void OnQuit(object sender, RoutedEventArgs e) => App.Current.Quit();
}
