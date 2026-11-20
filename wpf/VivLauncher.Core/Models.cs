using System.Text.RegularExpressions;

namespace VivLauncher.Core;

public sealed record Installation(string Path, string Version, bool Available)
{
    public string BatchPath => System.IO.Path.Combine(Path, "bin", "vivado.bat");
}

public sealed record RecentProject(string Path, string Version);
public sealed record LaunchOptions(bool NoLog = true, bool NoJournal = true, string ExtraArgs = "");
public sealed record ProjectBinding(string InstallPath = "", string LegacyVersion = "")
{
    public bool Exists => InstallPath.Length > 0 || LegacyVersion.Length > 0;
    public Installation? Resolve(IEnumerable<Installation> installs) => InstallPath.Length > 0
        ? installs.FirstOrDefault(item => Paths.Same(item.Path, InstallPath))
        : installs.FirstOrDefault(item => item.Version == LegacyVersion && item.Available);
}

public static class Paths
{
    public static string Absolute(string value) => Path.GetFullPath(value.Trim().Trim('"'));
    public static string InstallRoot(string value)
    {
        var path = Path.TrimEndingDirectorySeparator(Absolute(value));
        return Path.GetFileName(path).Equals("bin", StringComparison.OrdinalIgnoreCase)
            ? Path.GetDirectoryName(path)! : path;
    }
    public static bool Same(string? first, string? second) => !string.IsNullOrEmpty(first)
        && !string.IsNullOrEmpty(second)
        && string.Equals(Path.TrimEndingDirectorySeparator(first.Replace('/', '\\')),
            Path.TrimEndingDirectorySeparator(second.Replace('/', '\\')), StringComparison.OrdinalIgnoreCase);
    public static bool IsProject(string value) => Path.IsPathFullyQualified(value.Trim().Trim('"'))
        && value.Trim().Trim('"').EndsWith(".xpr", StringComparison.OrdinalIgnoreCase);
    public static string ProjectKey(string path)
    {
        var hash = 14695981039346656037UL;
        foreach (var character in Absolute(path))
        {
            hash ^= character is >= 'A' and <= 'Z' ? (char)(character + 32) : character;
            hash = unchecked(hash * 1099511628211UL);
        }
        return $"project_{hash:X16}";
    }
    public static string VersionLabel(string path)
    {
        var matches = Regex.Matches(path, @"(?:^|[\\/])(\d{4}\.\d+)(?=[\\/]|$)");
        return matches.Count > 0 ? matches[^1].Groups[1].Value : Path.GetFileName(path);
    }
    public static Version VersionNumber(string value) => Version.TryParse(value, out var version) ? version : new(0, 0);
}

public sealed record Selection(Installation? Install, string Reason)
{
    public static Selection Choose(ProjectBinding binding, string defaultPath, IReadOnlyList<Installation> installs)
    {
        if (binding.Exists)
        {
            var bound = binding.Resolve(installs);
            return bound?.Available == true ? new(bound, "BindingSource") : new(null, "MissingBinding");
        }
        var preferred = installs.FirstOrDefault(item => item.Available && Paths.Same(item.Path, defaultPath));
        return preferred is not null ? new(preferred, "DefaultSource")
            : new(installs.FirstOrDefault(item => item.Available), "LatestSource");
    }
}
