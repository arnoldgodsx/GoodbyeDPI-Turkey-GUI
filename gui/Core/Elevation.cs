using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using System.Security.Principal;
using System.Text;

namespace Hub;

/// <summary>
/// Arayüz yönetici yetkisiyle çalışır (GoodbyeDPI hizmetini yönetmek için). Her açılışta UAC
/// sormamak için "en yüksek yetkiyle" çalışan iki zamanlanmış görev kullanılır:
///   "GoodbyeDPI-Turkey GUI"           – kısayoldan açılış (tetikleyicisiz, isteğe bağlı çalıştırılır)
///   "GoodbyeDPI-Turkey GUI Autostart" – oturum açılınca arka planda başlatma (sadece "Windows ile başlat" açıksa var)
/// </summary>
static class Elevation
{
    const string ShowTask = "GoodbyeDPI-Turkey GUI";
    const string AutostartTask = "GoodbyeDPI-Turkey GUI Autostart";

    static string Schtasks => Path.Combine(Environment.SystemDirectory, "schtasks.exe");
    static readonly string Exe = Process.GetCurrentProcess().MainModule!.FileName;

    public static bool IsAdmin =>
        new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    static bool TaskExists(string name)
    {
        try { Proc.Run(Schtasks, $"/query /tn \"{name}\""); return true; }
        catch { return false; }
    }

    /// <summary>Normal yetkili kopyadan: yönetici kopyayı UAC sormadan başlatır.</summary>
    public static bool TryRunShowTask()
    {
        try { Proc.Run(Schtasks, $"/run /tn \"{ShowTask}\""); return true; }
        catch { return false; }
    }

    /// <summary>Görevler henüz yoksa (ilk açılış) tek seferlik UAC ile yönetici olarak yeniden başlar.</summary>
    public static void RelaunchElevated(string args)
    {
        try { Process.Start(new ProcessStartInfo(Exe, args) { UseShellExecute = true, Verb = "runas" }); }
        catch (Win32Exception) { } // kullanıcı UAC'yi reddetti
    }

    /// <summary>Yönetici kopyada her açılışta: görevleri bu exe'nin güncel yoluna göre yeniler.</summary>
    public static void RegisterTasks()
    {
        Register(ShowTask, "--show", logon: false);
        if (AutostartEnabled) Register(AutostartTask, "--background", logon: true);
    }

    public static bool AutostartEnabled => TaskExists(AutostartTask);

    public static void SetAutostart(bool enabled)
    {
        if (enabled) Register(AutostartTask, "--background", logon: true);
        else if (TaskExists(AutostartTask)) Proc.Run(Schtasks, $"/delete /tn \"{AutostartTask}\" /f");
    }

    static void Register(string name, string args, bool logon)
    {
        var user = SecurityElement.Escape(WindowsIdentity.GetCurrent().Name);
        var trigger = logon
            ? $"<Triggers><LogonTrigger><UserId>{user}</UserId><Delay>PT15S</Delay></LogonTrigger></Triggers>"
            : "";
        var xml = $"""
            <?xml version="1.0" encoding="UTF-16"?>
            <Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
              <RegistrationInfo><Description>GoodbyeDPI-Turkey GUI</Description></RegistrationInfo>
              {trigger}
              <Principals>
                <Principal id="Author">
                  <UserId>{user}</UserId>
                  <LogonType>InteractiveToken</LogonType>
                  <RunLevel>HighestAvailable</RunLevel>
                </Principal>
              </Principals>
              <Settings>
                <MultipleInstancesPolicy>Parallel</MultipleInstancesPolicy>
                <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
                <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
                <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
                <Priority>4</Priority>
              </Settings>
              <Actions Context="Author">
                <Exec>
                  <Command>{SecurityElement.Escape(Exe)}</Command>
                  <Arguments>{args}</Arguments>
                  <WorkingDirectory>{SecurityElement.Escape(AppContext.BaseDirectory)}</WorkingDirectory>
                </Exec>
              </Actions>
            </Task>
            """;

        var tmp = Path.Combine(Path.GetTempPath(), $"goodbyedpi-gui-task-{Guid.NewGuid():N}.xml");
        File.WriteAllText(tmp, xml, Encoding.Unicode);
        try { Proc.Run(Schtasks, $"/create /tn \"{name}\" /xml \"{tmp}\" /f"); }
        finally { File.Delete(tmp); }
    }
}
