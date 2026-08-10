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

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // WaitForExitAsync does not kill the process on cancellation - without this it
            // would keep running (and, for `lockdown pair`, keep the Trust dialog logic alive)
            // after the caller has already given up and moved the UI to a "retry" state.
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
                // already exited between the cancellation and the kill attempt
            }
            throw;
        }

        return new ProcessResult(process.ExitCode, stdout.ToString(), stderr.ToString());
    }
}
