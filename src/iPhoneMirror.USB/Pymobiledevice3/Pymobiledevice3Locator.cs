namespace iPhoneMirror.USB.Pymobiledevice3;

/// <summary>
/// Locates the pymobiledevice3 Python environment. During development this is the
/// venv under scripts/pmd3-venv (see docs/FEASIBILITY.md for why pymobiledevice3 is
/// invoked as an external process rather than linked in-process: it keeps its
/// GPL-3.0 license from applying to this codebase). The installer (Milestone 10)
/// will bundle a fixed-layout copy and this should be updated to point at it.
/// </summary>
public static class Pymobiledevice3Locator
{
    private const string VenvPythonRelativePath = @"scripts\pmd3-venv\Scripts\python.exe";

    public static string FindPythonExecutable()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, VenvPythonRelativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(
            "Could not find the pymobiledevice3 Python environment. Expected to find " +
            $"'{VenvPythonRelativePath}' in this repository or an ancestor of " +
            $"'{AppContext.BaseDirectory}'.");
    }
}
