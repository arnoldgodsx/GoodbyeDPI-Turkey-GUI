using System.Windows;

namespace Hub.Modules.GoodbyeDpi;

public sealed class GoodbyeDpiModule : IModule
{
    public string Title => "GoodbyeDPI";

    public bool Active { get; private set; }
    public bool AutoStart { get; private set; } = true;
    public Profile Profile { get; private set; } = Profile.All[0];
    public bool Busy { get; private set; }
    public string? Error { get; private set; }

    public event Action? Changed;
    Timer? _poll;
    int _pending; // süren işlem varken yoklama hizmetin ara durumunu arayüze taşımasın
    readonly SemaphoreSlim _gate = new(1, 1);

    public void Start()
    {
        _ = Task.Run(() =>
        {
            Refresh();
            MigrateEnginePath();
        });
        _poll = new Timer(_ => { if (Volatile.Read(ref _pending) == 0) Refresh(); }, null, 2500, 2500);
    }

    public void Stop() => _poll?.Dispose();

    public FrameworkElement CreatePage() => new GoodbyeDpiPage(this);

    void Refresh()
    {
        bool active = Svc.IsActive(), auto = AutoStart;
        var profile = Profile;
        if (Svc.Exists())
        {
            auto = Svc.StartType() == 2;
            profile = Profile.FromArgs(Svc.ArgsOf(Svc.ImagePath()));
        }
        var changed = active != Active || auto != AutoStart || profile != Profile;
        Active = active;
        AutoStart = auto;
        Profile = profile;
        if (changed) Changed?.Invoke();
    }

    /// <summary>
    /// Hizmet eski bir klasördeki goodbyedpi.exe'yi gösteriyorsa Hub'ın kendi kopyasına yönlendirir.
    /// Çalışan hizmete dokunmaz; yeni yol bir sonraki başlatmada geçerli olur.
    /// </summary>
    void MigrateEnginePath()
    {
        try
        {
            if (!Svc.Exists() || !File.Exists(Svc.EnginePath)) return;
            if (string.Equals(Svc.ExeOf(Svc.ImagePath()), Svc.EnginePath, StringComparison.OrdinalIgnoreCase)) return;
            Svc.Configure(Profile, AutoStart);
        }
        catch { /* bir sonraki açma/kapamada Configure zaten yeniden dener */ }
    }

    public void SetActive(bool want)
    {
        var p = Profile;
        var auto = AutoStart;
        if (want) RunOp(() => { Svc.Configure(p, auto); Svc.Start(); }, busy: true);
        else RunOp(Svc.Stop, busy: true);
    }

    public void SetAutoStart(bool want)
    {
        AutoStart = want;
        var p = Profile;
        RunOp(() => Svc.Configure(p, want));
    }

    public void SetProfile(Profile p)
    {
        if (p.Args == Profile.Args) return;
        var wasActive = Active;
        var auto = AutoStart;
        Profile = p;
        // Sadece çalışan hizmeti yeniden başlatmak uzun sürer; kayıt anında biter ve arayüzde görünmez.
        RunOp(() =>
        {
            Svc.Configure(p, auto);
            if (wasActive) { Svc.Stop(); Svc.Start(); }
        }, busy: wasActive);
    }

    /// <summary>
    /// Hizmet işlemlerini sırayla çalıştırır. <paramref name="busy"/> ise işlem süresince
    /// "meşgul" gösterilir; değilse (kısa kayıtlar) arayüz hiç değişmez, titremez.
    /// </summary>
    async void RunOp(Action work, bool busy = false)
    {
        Interlocked.Increment(ref _pending);
        var hadError = Error != null;
        Error = null;
        if (busy) Busy = true;
        if (busy || hadError) Changed?.Invoke();

        await _gate.WaitAsync();
        try { await Task.Run(work); }
        catch (Exception ex) { Error = ex.Message; }
        finally { _gate.Release(); }

        if (busy) Busy = false;
        await Task.Run(Refresh);
        Interlocked.Decrement(ref _pending);
        Changed?.Invoke();
    }
}
