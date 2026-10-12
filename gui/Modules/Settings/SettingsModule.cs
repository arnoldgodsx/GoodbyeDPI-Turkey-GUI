using System.Windows;

namespace Hub.Modules.Settings;

/// <summary>Arayüzün kendi ayarları; kenar çubuğunun en altında durur.</summary>
public sealed class SettingsModule : IModule
{
    public string Title => "Ayarlar";

    public void Start() { }
    public void Stop() { }
    public FrameworkElement CreatePage() => new SettingsPage();
}
