namespace Hub.Modules.Activity;

public enum EngineState { Idle, Working, Done, Error }

public sealed record Status(EngineState State, int Today, string Message, DateTime? LastCheck, DateTime? NextCheck);

/// <summary>
/// keep-green.yml iş akışının masaüstü karşılığı. Mantık aynıdır:
/// gün UTC'ye göre sayılır, bugün main'e atılmış commit'ler düşülür, eksik kadar
/// commit tek tek push edilir. Tekrar çalıştırmak hedefi asla aşmaz.
/// </summary>
public sealed class ActivityEngine
{
    public const string Owner = "arnoldgodsx";
    public const string Repo = "activity";
    public const int Target = 45;
    // GitHub kullanıcı ID'si; commit'ler iş akışıyla aynı noreply adresinden atılır.
    const long UserId = 287445178;

    static readonly TimeSpan Interval = TimeSpan.FromMinutes(20);
    static readonly TimeSpan RetryInterval = TimeSpan.FromMinutes(2);

    static string DataDir { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Hub", "activity");
    static string RepoDir => Path.Combine(DataDir, "repo");

    readonly CancellationTokenSource _cts = new();
    readonly SemaphoreSlim _wake = new(0, 1);

    public event Action<Status>? Changed;
    public Status Current { get; private set; } = new(EngineState.Idle, 0, "Başlatılıyor…", null, null);

    public void Start() => _ = Task.Run(LoopAsync);
    public void Stop() => _cts.Cancel();

    public void CheckNow()
    {
        if (_wake.CurrentCount == 0) _wake.Release();
    }

    async Task LoopAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            var ok = await RunOnceAsync();

            var delay = ok ? Interval : RetryInterval;
            // UTC gece yarısı geçince yeni günü beklemeden doldur.
            var untilNewDay = DateTime.UtcNow.Date.AddDays(1) - DateTime.UtcNow + TimeSpan.FromSeconds(30);
            if (untilNewDay < delay) delay = untilNewDay;

            Publish(Current with { NextCheck = DateTime.Now + delay });
            try { await _wake.WaitAsync(delay, _cts.Token); }
            catch (OperationCanceledException) { return; }
        }
    }

    async Task<bool> RunOnceAsync()
    {
        var ct = _cts.Token;
        try
        {
            Publish(EngineState.Working, Current.Today, "Kontrol ediliyor…");
            await PrepareRepoAsync(ct);

            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var have = await CountAsync(today, ct);
            var need = Target - have;

            for (var i = 1; i <= need; i++)
            {
                ct.ThrowIfCancellationRequested();
                if (DateTime.UtcNow.ToString("yyyy-MM-dd") != today) break; // gün döndü, sonraki tur yeniden sayar

                var now = DateTime.UtcNow;
                await File.AppendAllTextAsync(Path.Combine(RepoDir, "activity.log"),
                    $"{now:yyyy-MM-ddTHH:mm:ssZ} #{i}\n", ct);
                await Proc.Git("add activity.log", RepoDir, ct: ct);

                // Tarih UTC damgalı atılır ki takvimde de sayıldığı UTC gününe düşsün.
                var date = $"{now:yyyy-MM-ddTHH:mm:ss} +0000";
                await Proc.Git($"commit -q -m \"activity {today} #{have + i}\"", RepoDir,
                    new Dictionary<string, string> { ["GIT_AUTHOR_DATE"] = date, ["GIT_COMMITTER_DATE"] = date }, ct);

                // Her commit ayrı push ile gider: toplu push'ta takvim commit'leri saymayabiliyor.
                await Proc.Git("push -q origin HEAD:main", RepoDir, ct: ct);

                Publish(EngineState.Working, have + i, $"Ekleniyor… {have + i}/{Target}");
                await Task.Delay(1000, ct);
            }

            var total = await CountAsync(today, ct);
            Publish(total >= Target ? EngineState.Done : EngineState.Idle, total,
                total >= Target ? "Bugünkü hedef tamamlandı" : $"Bugün {total}/{Target}",
                lastCheck: DateTime.Now);
            return true;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return true;
        }
        catch (Exception ex)
        {
            // Push reddi (ör. GitHub Actions aynı anda çalıştı) dahil: kısa süre sonra yeniden sayılır.
            var msg = ex.Message.Split('\n')[0].Trim();
            if (msg.Length > 90) msg = msg[..90] + "…";
            Publish(EngineState.Error, Current.Today, msg, lastCheck: DateTime.Now);
            return false;
        }
    }

    static async Task PrepareRepoAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(DataDir);
        if (!Directory.Exists(Path.Combine(RepoDir, ".git")))
        {
            if (Directory.Exists(RepoDir)) Directory.Delete(RepoDir, true);
            await Proc.Git($"clone -q https://github.com/{Owner}/{Repo}.git repo", DataDir, ct: ct);
        }

        await Proc.Git("config core.autocrlf false", RepoDir, ct: ct);
        await Proc.Git($"config user.name {Owner}", RepoDir, ct: ct);
        await Proc.Git($"config user.email {UserId}+{Owner}@users.noreply.github.com", RepoDir, ct: ct);

        // Yerel kopya sadece bu modülündür; her tur GitHub'daki main'e eşitlenir.
        await Proc.Git("fetch -q origin main", RepoDir, ct: ct);
        await Proc.Git("checkout -q -B main origin/main", RepoDir, ct: ct);
        await Proc.Git("reset -q --hard origin/main", RepoDir, ct: ct);
    }

    static async Task<int> CountAsync(string today, CancellationToken ct)
    {
        var n = await Proc.Git($"rev-list --count --since={today}T00:00:00Z origin/main", RepoDir, ct: ct);
        return int.Parse(n);
    }

    void Publish(EngineState state, int today, string message, DateTime? lastCheck = null)
        => Publish(new Status(state, today, message, lastCheck ?? Current.LastCheck, Current.NextCheck));

    void Publish(Status s)
    {
        Current = s;
        Changed?.Invoke(s);
    }
}

/// <summary>Repodaki keep-green.yml iş akışını (bilgisayar kapalıyken yedek) açıp kapatır.</summary>
static class CloudBackup
{
    const string Workflow = "keep-green.yml";
    static string RepoArg => $"{ActivityEngine.Owner}/{ActivityEngine.Repo}";

    public static async Task<bool> IsEnabledAsync()
        => await Proc.RunAsync(Proc.Gh, $"api repos/{RepoArg}/actions/workflows/{Workflow} --jq .state") == "active";

    public static Task SetAsync(bool enabled)
        => Proc.RunAsync(Proc.Gh, $"workflow {(enabled ? "enable" : "disable")} {Workflow} -R {RepoArg}");
}
