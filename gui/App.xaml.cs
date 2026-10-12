using System.Windows;
using Hub.Modules.GoodbyeDpi;
using Hub.Modules.Settings;

namespace Hub;

public partial class App : Application
{
    const string MutexName = @"Local\GoodbyeDPI-Turkey-GUI";
    const string ShowEventName = @"Local\GoodbyeDPI-Turkey-GUI.Show";

    Mutex? _mutex;
    EventWaitHandle? _showSignal;
    MainWindow? _window;

    /// <summary>Kenar çubuğundaki uygulamalar, sırasıyla. Yeni modül buraya eklenir.</summary>
    public IReadOnlyList<IModule> Modules { get; private set; } = [];
    public SettingsModule Settings { get; } = new();
    public bool Quitting { get; private set; }

    public static new App Current => (App)Application.Current;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var background = e.Args.Contains("--background");

        // Geliştirme için: --snapshot <klasör>  her sayfayı PNG olarak kaydeder (yönetici gerekmez).
        var snap = Array.IndexOf(e.Args, "--snapshot");
        if (snap >= 0 && snap + 1 < e.Args.Length)
        {
            Modules = [new GoodbyeDpiModule()];
            _ = SnapshotAsync(e.Args[snap + 1]);
            return;
        }

        // Geliştirme için: --preview  pencereyi yönetici olmadan, modülleri başlatmadan gösterir.
        if (e.Args.Contains("--preview"))
        {
            Modules = [new GoodbyeDpiModule()];
            _window = new MainWindow();
            _window.Show();
            return;
        }

        if (!Elevation.IsAdmin)
        {
            // Kısayoldan normal yetkiyle açıldı: yönetici kopyayı UAC sormadan görevle başlat.
            // Görev yoksa (ilk açılış) bir kereliğine UAC ile yükselir; o kopya görevleri kurar.
            if (!Elevation.TryRunShowTask()) Elevation.RelaunchElevated(background ? "--background" : "--show");
            Shutdown();
            return;
        }

        // Kurulum, eski sürümde açık olan "Windows ile başlat"ı yeni görev adıyla taşımak için çağırır.
        if (e.Args.Contains("--enable-autostart"))
        {
            try { Elevation.SetAutostart(true); } catch { }
            Shutdown();
            return;
        }

        _mutex = new Mutex(true, MutexName, out var first);
        _showSignal = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName);
        if (!first)
        {
            // Zaten arka planda çalışıyor: penceresini öne getir ve çık.
            if (!background) _showSignal.Set();
            Shutdown();
            return;
        }

        try { Elevation.RegisterTasks(); } catch { /* görevler olmadan da çalışır, sadece UAC sorar */ }

        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        Modules = [new GoodbyeDpiModule()];
        foreach (var m in Modules) m.Start();

        new Thread(() =>
        {
            while (_showSignal.WaitOne()) Dispatcher.Invoke(ShowWindow);
        }) { IsBackground = true }.Start();

        if (!background) ShowWindow();
    }

    public void ShowWindow()
    {
        _window ??= new MainWindow();
        _window.Show();
        if (_window.WindowState == WindowState.Minimized) _window.WindowState = WindowState.Normal;
        // Görev Zamanlayıcı'dan başlayan süreç ön plana çıkma hakkı olmayabilir; kısa süre en üste al.
        _window.Topmost = true;
        _window.Activate();
        _window.Topmost = false;
    }

    async Task SnapshotAsync(string dir)
    {
        Directory.CreateDirectory(dir);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        try
        {
            var w = new MainWindow { WindowStartupLocation = WindowStartupLocation.Manual, Left = -4000, Top = 0, ShowActivated = false };
            w.Show();
            for (var i = 0; i < w.PageCount; i++)
            {
                w.SelectPage(i);
                await Task.Delay(1200);
                w.SaveSnapshot(Path.Combine(dir, $"page{i}.png"));
            }
        }
        catch (Exception ex)
        {
            File.WriteAllText(Path.Combine(dir, "error.txt"), ex.ToString());
        }
        Shutdown();
    }

    public void Quit()
    {
        Quitting = true;
        foreach (var m in Modules) m.Stop();
        Shutdown();
    }
}
