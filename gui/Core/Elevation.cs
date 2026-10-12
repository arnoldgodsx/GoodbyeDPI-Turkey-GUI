using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using System.Security.Principal;
using System.Text;

namespace Hub;

/// <summary>
/// Arayüz yönetici yetkisiyle çalışır (GoodbyeDPI hizmetini yönetmek için). Her açılışta UAC
/// sormamak için "en yüksek yetkiyle" çalışan, tetikleyicisiz bir zamanlanmış görev
/// ("GoodbyeDPI-Turkey GUI") kullanılır; kısayol programı bu görev üzerinden açar.
/// </summary>
static class Elevation
{
    const string ShowTask = "GoodbyeDPI-Turkey GUI";

    static string Schtasks => Path.Combine(Environment.SystemDirectory, "schtasks.exe");
    static readonly string Exe = Process.GetCurrentProcess().MainModule!.FileName;

    public static bool IsAdmin =>
        new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    /// <summary>Normal yetkili kopyadan: yönetici kopyayı UAC sormadan başlatır.</summary>
    public static bool TryRunShowTask()
    {
        try { Proc.Run(Schtasks, $"/run /tn \"{ShowTask}\""); return true; }
        catch { return false; }
    }

    /// <summary>Görev henüz yoksa (ilk açılış) tek seferlik UAC ile yönetici olarak yeniden başlar.</summary>
    public static void RelaunchElevated(string args)
    {
        try { Process.Start(new ProcessStartInfo(Exe, args) { UseShellExecute = true, Verb = "runas" }); }
        catch (Win32Exception) { } // kullanıcı UAC'yi reddetti
    }

    /// <summary>Yönetici kopyada her açılışta: görevi bu exe'nin güncel yoluna göre yeniler.</summary>
    public static void RegisterTask()
    {
        // Görev exe'yi UAC sormadan yönetici olarak açar; exe'yi başkası değiştirebiliyorsa
        // kurma, varsa eskisini de sil (UAC her açılışta sorar).
        if (Acl.AdminOnly([AppContext.BaseDirectory.TrimEnd('\\'), Exe])) Register(ShowTask, "--show");
        else try { Proc.Run(Schtasks, $"/delete /tn \"{ShowTask}\" /f"); } catch { }
    }

    static void Register(string name, string args)
    {
        var user = SecurityElement.Escape(WindowsIdentity.GetCurrent().Name);
        var xml = $"""
            <?xml version="1.0" encoding="UTF-16"?>
            <Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
              <RegistrationInfo><Description>GoodbyeDPI-Turkey GUI</Description></RegistrationInfo>
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

        // %TEMP% kullanıcının yükseltilmemiş süreçlerince de yazılabilir. Dosya schtasks okuyana kadar
        // değiştirilemesin diye yazma/silme paylaşımı olmadan açık tutulur ve içeriği doğrulanır.
        var tmp = Path.Combine(Path.GetTempPath(), $"goodbyedpi-gui-task-{Guid.NewGuid():N}.xml");
        var bytes = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes(xml)).ToArray();
        try
        {
            using (var w = new FileStream(tmp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                w.Write(bytes, 0, bytes.Length);
            using var r = new FileStream(tmp, FileMode.Open, FileAccess.Read, FileShare.Read);
            var read = new byte[bytes.Length + 1];
            int n = 0, k;
            while (n < read.Length && (k = r.Read(read, n, read.Length - n)) > 0) n += k;
            if (n != bytes.Length || !read.Take(n).SequenceEqual(bytes))
                throw new InvalidOperationException("Görev dosyası değiştirilmiş");
            Proc.Run(Schtasks, $"/create /tn \"{name}\" /xml \"{tmp}\" /f");
        }
        finally { try { File.Delete(tmp); } catch { } }
    }
}
