using System.Diagnostics;
using System.Text;

namespace iPhoneMirror.USB.Pymobiledevice3;

public sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);

/// <summary>Runs `python -m pymobiledevice3 &lt;args&gt;` and captures its output.</summary>
public static class Pymobiledevice3ProcessRunner
{
    public static async Task<ProcessResult> RunAsync(
        IEnumerable<string> arguments, CancellationToken cancellationToken = default)
    {
        var pythonExe = Pymobiledevice3Locator.FindPythonExecutable();

        var startInfo = new ProcessStartInfo
        {
            FileName = pythonExe,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add("-m");
        startInfo.ArgumentList.Add("pymobiledevice3");
        foreach (var arg in arguments)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = new Process { StartInfo = startInfo };
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        process.OutputDataReceived += (_, e) => { if (e.Data is not null) stdout.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) stderr.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        return new ProcessResult(process.ExitCode, stdout.ToString(), stderr.ToString());
    }
}
