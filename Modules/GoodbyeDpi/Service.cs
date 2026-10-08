using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Hub.Modules.GoodbyeDpi;

public sealed record Profile(string Name, string Hint, string Args)
{
    const string Dns = "--dns-addr 77.88.8.8 --dns-port 1253 --dnsv6-addr 2a02:6b8::feed:0ff --dnsv6-port 1253";

    // Paketteki service_install_dnsredir_turkey*.cmd dosyalarıyla birebir aynı parametreler
    public static readonly Profile[] All =
    [
        new("Standart",     "Önerilen · Yandex DNS",    "-5 --set-ttl 5 " + Dns),
        new("Alternatif 1", "Superonline",              "--set-ttl 3"),
        new("Alternatif 2", "Superonline",              "-5"),
        new("Alternatif 3", "Superonline · Yandex DNS", "--set-ttl 3 " + Dns),
        new("Alternatif 4", "Superonline · Yandex DNS", "-5 " + Dns),
        new("Alternatif 5", "Superonline · Yandex DNS", "-9 " + Dns),
        new("Alternatif 6", "Superonline",              "-9"),
    ];

    static string Normalize(string? s) => Regex.Replace(s ?? "", @"\s+", " ").Trim();

    public static Profile FromArgs(string args)
    {
        var n = Normalize(args);
        return All.FirstOrDefault(p => p.Args == n) ?? new("Özel", "Elle ayarlanmış parametreler", n);
    }
}

/// <summary>"GoodbyeDPI" Windows hizmetini yönetir (yönetici yetkisi gerekir).</summary>
static class Svc
{
    public const string Name = "GoodbyeDPI";

    public static string EnginePath => Path.Combine(AppContext.BaseDirectory, "engine", "goodbyedpi",
        Environment.Is64BitOperatingSystem ? "x86_64" : "x86", "goodbyedpi.exe");

    public static bool Exists()
    {
        try { using var sc = new ServiceController(Name); _ = sc.Status; return true; }
        catch (InvalidOperationException) { return false; }
    }

    static object? RegValue(string value)
    {
        using var k = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + Name);
        return k?.GetValue(value);
    }

    public static string? ImagePath() => RegValue("ImagePath") as string;

    // 2 = otomatik, 3 = elle, 4 = devre dışı
    public static int StartType() => RegValue("Start") is int v ? v : 3;

    static (string Exe, string Args) Split(string? imagePath)
    {
        var s = (imagePath ?? "").Trim();
        if (s.Length == 0) return ("", "");
        if (s.StartsWith('"'))
        {
            var q = s.IndexOf('"', 1);
            return q < 0 ? (s.Trim('"'), "") : (s[1..q], s[(q + 1)..].Trim());
        }
        var sp = s.IndexOf(' ');
        return sp < 0 ? (s, "") : (s[..sp], s[(sp + 1)..].Trim());
    }

    public static string ArgsOf(string? imagePath) => Split(imagePath).Args;
    public static string ExeOf(string? imagePath) => Split(imagePath).Exe;

    public static bool ServiceRunning()
    {
        try
        {
            using var sc = new ServiceController(Name);
            return sc.Status is ServiceControllerStatus.Running or ServiceControllerStatus.StartPending;
        }
        catch (InvalidOperationException) { return false; }
    }

    static Process[] Engines() => Process.GetProcessesByName("goodbyedpi");

    // Hizmet ya da elle başlatılmış bir goodbyedpi.exe çalışıyor mu?
    public static bool IsActive() => ServiceRunning() || Engines().Length > 0;

    static void KillEngines()
    {
        foreach (var p in Engines())
        {
            try { p.Kill(); p.WaitForExit(3000); } catch { }
        }
    }

    static string Quote(string s) => "\"" + s.Replace("\"", "\\\"") + "\"";

    static void Sc(string args)
    {
        var psi = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "sc.exe"), args)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
        };
        using var p = Process.Start(psi)!;
        p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        // sc.exe hata metnini stdout'a yazar; çıkış kodu Windows hata kodudur.
        if (p.ExitCode != 0) throw new Exception(new Win32Exception(p.ExitCode).Message);
    }

    // Hizmeti bu klasördeki goodbyedpi.exe'yi ve seçili profili kullanacak şekilde kurar/günceller.
    public static void Configure(Profile profile, bool autoStart)
    {
        if (!File.Exists(EnginePath)) throw new Exception("goodbyedpi.exe bulunamadı:\n" + EnginePath);

        var bin = "\"" + EnginePath + "\" " + profile.Args;
        var start = autoStart ? 2 : 3;
        var startArg = autoStart ? "auto" : "demand";

        if (!Exists())
        {
            Sc($"create {Name} binPath= {Quote(bin)} start= {startArg} DisplayName= {Name}");
            Sc($"description {Name} {Quote("Turkiye icin DNS zorlamasini kaldirir. (Hub)")}");
            return;
        }
        if (!string.Equals(ImagePath(), bin, StringComparison.OrdinalIgnoreCase))
            Sc($"config {Name} binPath= {Quote(bin)}");
        if (StartType() != start)
            Sc($"config {Name} start= {startArg}");
    }

    public static void Start()
    {
        using (var sc = new ServiceController(Name))
        {
            if (sc.Status == ServiceControllerStatus.Running) return;
            KillEngines(); // elle açılmış kopyalar hizmetle çakışır
            if (sc.Status == ServiceControllerStatus.StopPending)
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(15));
            try
            {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(15));
            }
            catch (InvalidOperationException ex) { throw new Exception("Hizmet başlatılamadı: " + (ex.InnerException ?? ex).Message); }
            catch (System.ServiceProcess.TimeoutException) { throw new Exception("Hizmet zamanında başlamadı."); }
        }
        Thread.Sleep(1200);
        if (!ServiceRunning())
            throw new Exception("GoodbyeDPI açıldı ama hemen kapandı. Başka bir profil deneyin.");
    }

    public static void Stop()
    {
        if (Exists())
        {
            using var sc = new ServiceController(Name);
            var st = sc.Status;
            if (st != ServiceControllerStatus.Stopped)
            {
                try
                {
                    if (st != ServiceControllerStatus.StopPending) sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(15));
                }
                catch (InvalidOperationException ex) { throw new Exception("Hizmet durdurulamadı: " + (ex.InnerException ?? ex).Message); }
                catch (System.ServiceProcess.TimeoutException) { }
            }
        }
        KillEngines();
    }
}
