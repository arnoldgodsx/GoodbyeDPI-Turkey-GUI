using System.Diagnostics;

namespace Hub;

/// <summary>Konsol penceresi açmadan harici komut (sc, schtasks) çalıştırır.</summary>
static class Proc
{
    static ProcessStartInfo Info(string file, string args) => new(file, args)
    {
        WorkingDirectory = AppContext.BaseDirectory,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
    };

    static Exception Fail(string file, string args, int code, string stderr) =>
        new InvalidOperationException(stderr.Length > 0 ? stderr
            : $"{Path.GetFileName(file)} {args.Split(' ')[0]} başarısız ({code})");

    public static string Run(string file, string args)
    {
        using var p = Process.Start(Info(file, args)) ?? throw new InvalidOperationException($"{file} başlatılamadı");
        var stderr = p.StandardError.ReadToEndAsync();
        var output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        if (p.ExitCode != 0) throw Fail(file, args, p.ExitCode, stderr.Result.Trim());
        return output.Trim();
    }
}
