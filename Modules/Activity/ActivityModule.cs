using System.Windows;

namespace Hub.Modules.Activity;

public sealed class ActivityModule : IModule
{
    public ActivityEngine Engine { get; } = new();

    public string Title => "GitHub Activity";

    public void Start() => Engine.Start();
    public void Stop() => Engine.Stop();
    public FrameworkElement CreatePage() => new ActivityPage(Engine);
}
