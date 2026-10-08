using System.Diagnostics;

namespace Hub;

/// <summary>Konsol penceresi açmadan harici komut (git, gh, sc, schtasks) çalıştırır.</summary>
static class Proc
{
    public const string Gh = @"C:\Program Files\GitHub CLI\gh.exe";

    static ProcessStartInfo Info(string file, string args, string? cwd, IDictionary<string, string>? env)
    {
        var psi = new ProcessStartInfo(file, args)
        {
            WorkingDirectory = cwd ?? AppContext.BaseDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        // Arka planda asla kimlik bilgisi sorusu bekleyip takılmasın.
        psi.Environment["GIT_TERMINAL_PROMPT"] = "0";
        psi.Environment["GCM_INTERACTIVE"] = "never";
        if (env != null)
            foreach (var (k, v) in env) psi.Environment[k] = v;
        return psi;
    }

    static Exception Fail(string file, string args, int code, string stderr) =>
        new InvalidOperationException(stderr.Length > 0 ? stderr
            : $"{Path.GetFileName(file)} {args.Split(' ')[0]} başarısız ({code})");

    public static async Task<string> RunAsync(string file, string args, string? cwd = null,
        IDictionary<string, string>? env = null, CancellationToken ct = default)
    {
        using var p = Process.Start(Info(file, args, cwd, env)) ?? throw new InvalidOperationException($"{file} başlatılamadı");
        var stdout = p.StandardOutput.ReadToEndAsync(ct);
        var stderr = p.StandardError.ReadToEndAsync(ct);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromMinutes(3));
        try { await p.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException)
        {
            try { p.Kill(true); } catch { }
            throw;
        }

        var output = await stdout;
        if (p.ExitCode != 0) throw Fail(file, args, p.ExitCode, (await stderr).Trim());
        return output.Trim();
    }

    public static string Run(string file, string args)
    {
        using var p = Process.Start(Info(file, args, null, null)) ?? throw new InvalidOperationException($"{file} başlatılamadı");
        var stderr = p.StandardError.ReadToEndAsync();
        var output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        if (p.ExitCode != 0) throw Fail(file, args, p.ExitCode, stderr.Result.Trim());
        return output.Trim();
    }

    public static Task<string> Git(string args, string cwd, IDictionary<string, string>? env = null, CancellationToken ct = default)
        => RunAsync("git", args, cwd, env, ct);
}
