using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace VivLauncher.Core;

public sealed class SettingsStore
{
    private readonly object gate = new();
    public string DirectoryPath { get; }
    public string IniPath => Path.Combine(DirectoryPath, "settings.ini");
    public SettingsStore(string? directory = null) => DirectoryPath = directory
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vivlauncher");

    public string Read(string section, string key, string fallback = "")
    {
        var buffer = new StringBuilder(32768);
        GetPrivateProfileString(section, key, fallback, buffer, buffer.Capacity, IniPath);
        return buffer.ToString();
    }

    public void Write(string section, string key, string? value)
    {
        lock (gate)
        {
            Directory.CreateDirectory(DirectoryPath);
            if (!File.Exists(IniPath)) File.WriteAllText(IniPath, "", Encoding.Unicode);
            else EnsureUnicodeIni();
            if (!WritePrivateProfileString(section, key, value, IniPath))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    private void EnsureUnicodeIni()
    {
        var bytes = File.ReadAllBytes(IniPath);
        if (bytes.Length >= 2 && bytes[0] == 0xff && bytes[1] == 0xfe) return;
        var contents = bytes.Length >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf
            ? Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3)
            : AnsiEncoding().GetString(bytes);
        var backup = IniPath + ".pre-wpf.bak";
        if (!File.Exists(backup)) File.Copy(IniPath, backup);
        var temporary = IniPath + ".tmp";
        File.WriteAllText(temporary, contents, Encoding.Unicode);
        ReplaceFile(temporary, IniPath);
        WritePrivateProfileString(null!, null!, null, IniPath);
    }

    public IReadOnlyList<string> ReadSectionValues(string section)
    {
        var buffer = new char[65536];
        var count = GetPrivateProfileSection(section, buffer, buffer.Length, IniPath);
        if (count >= buffer.Length - 2) throw new IOException("Settings section exceeds the supported size.");
        return new string(buffer, 0, (int)count).Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.Contains('=')).Select(line => line.Substring(line.IndexOf('=') + 1)).ToArray();
    }

    public IReadOnlyList<RecentProject> Recent()
    {
        var result = new List<RecentProject>();
        for (var index = 0; index < 20; index++)
        {
            var value = Read("recent_projects", $"recent_{index}");
            if (value.Length == 0) continue;
            var separator = value.IndexOf('|');
            var path = separator < 0 ? value : value.Substring(separator + 1);
            try
            {
                path = Paths.Absolute(path);
                if (!result.Any(item => Paths.Same(item.Path, path)))
                    result.Add(new(path, separator < 0 ? "" : value.Substring(0, separator)));
            }
            catch (ArgumentException) { }
        }
        return result;
    }

    public void Remember(string path, string version)
    {
        lock (gate)
        {
            var recent = new[] { new RecentProject(path, version) }.Concat(Recent().Where(item => !Paths.Same(item.Path, path))).Take(20).ToArray();
            SaveRecent(recent);
        }
    }
    public void ClearRecent()
    {
        lock (gate) SaveRecent([]);
    }
    public RecentRemoval? RemoveUnavailableRecent(IEnumerable<string> paths)
    {
        var candidates = paths.Select(Paths.Absolute).ToArray();
        lock (gate)
        {
            var before = Recent().ToArray();
            var removed = before.Where(item => candidates.Any(path => Paths.Same(path, item.Path)) && !File.Exists(item.Path)).ToArray();
            if (removed.Length == 0) return null;
            var remaining = before.Where(item => !removed.Contains(item)).ToArray();
            SaveRecent(remaining);
            return new(IniPath, before, remaining, removed);
        }
    }
    public bool RestoreRecent(RecentRemoval removal)
    {
        lock (gate)
        {
            if (!Paths.Same(IniPath, removal.SettingsPath)) throw new ArgumentException("The removal belongs to another settings file.", nameof(removal));
            var current = Recent();
            if (current.Count != removal.Remaining.Count || current.Where((item, index) =>
                !Paths.Same(item.Path, removal.Remaining[index].Path) || item.Version != removal.Remaining[index].Version).Any()) return false;
            SaveRecent(removal.Before);
            return true;
        }
    }
    private void SaveRecent(IReadOnlyList<RecentProject> recent)
    {
        Directory.CreateDirectory(DirectoryPath);
        if (!File.Exists(IniPath)) File.WriteAllText(IniPath, "", Encoding.Unicode);
        else EnsureUnicodeIni();
        var original = File.ReadAllBytes(IniPath);
        var temporary = IniPath + ".recent-" + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllBytes(temporary, original);
            for (var index = 0; index < 20; index++)
                if (!WritePrivateProfileString("recent_projects", $"recent_{index}", index < recent.Count ? $"{recent[index].Version}|{recent[index].Path}" : null, temporary))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            WritePrivateProfileString(null!, null!, null, temporary);
            if (!File.ReadAllBytes(IniPath).SequenceEqual(original)) throw new IOException("Settings changed while saving recent projects. Please retry.");
            ReplaceFile(temporary, IniPath);
            WritePrivateProfileString(null!, null!, null, IniPath);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
    public ProjectBinding Binding(string path)
    {
        var key = Paths.ProjectKey(path);
        return new(Read("project_installs", key), Read("project_versions", key));
    }
    public void Bind(string projectPath, string? installPath)
    {
        var key = Paths.ProjectKey(projectPath);
        Write("project_versions", key, null);
        Write("project_installs", key, installPath);
    }
    public string DefaultPath(IEnumerable<Installation> installs)
    {
        var path = Read("vivlauncher", "default_install");
        return path.Length > 0 ? path : installs.FirstOrDefault(item => item.Version == Read("vivlauncher", "default_version"))?.Path ?? "";
    }
    public IReadOnlyList<string> RemovedInstalls() => ReadSectionValues("removed_installs");
    public bool IsRemoved(string path) => Read("removed_installs", Paths.ProjectKey(path)).Length > 0;
    public void RemoveInstall(Installation installation, string defaultPath)
    {
        if (installation.Available || installation.Path.Length == 0) throw new InvalidOperationException("Only unavailable installation paths can be removed.");
        lock (gate)
        {
            if (File.Exists(installation.BatchPath)) throw new InvalidOperationException("The installation is available again. Scan for installations first.");
            if (Paths.Same(defaultPath, installation.Path))
            {
                Write("vivlauncher", "default_version", null);
                Write("vivlauncher", "default_install", null);
            }
            Write("removed_installs", Paths.ProjectKey(installation.Path), installation.Path);
        }
    }
    public void RestoreInstall(string path)
    {
        lock (gate)
        {
            Write("known_installs", Paths.ProjectKey(path), path);
            Write("removed_installs", Paths.ProjectKey(path), null);
        }
    }
    public LaunchOptions Options() => new(Read("launch", "no_log", "1") != "0",
        Read("launch", "no_journal", "1") != "0", Read("launch", "extra_args"));
    public void SaveOptions(LaunchOptions options)
    {
        Write("launch", "no_log", options.NoLog ? "1" : "0");
        Write("launch", "no_journal", options.NoJournal ? "1" : "0");
        Write("launch", "extra_args", options.ExtraArgs);
    }
    public IReadOnlyList<string> CustomPaths()
    {
        var file = Path.Combine(DirectoryPath, "paths.txt");
        if (!File.Exists(file)) file = Path.Combine(DirectoryPath, "paths.json");
        if (!File.Exists(file)) return [];
        string contents;
        try { contents = File.ReadAllText(file, new UTF8Encoding(false, true)); }
        catch (DecoderFallbackException)
        {
            contents = File.ReadAllText(file, AnsiEncoding());
        }
        return contents.Split('\n').Select(line => line.Trim()).Where(line => line.Length > 0 && !line.StartsWith('#')).ToArray();
    }
    public void AddPath(string path)
    {
        path = Paths.InstallRoot(path);
        if (!File.Exists(Path.Combine(path, "bin", "vivado.bat"))) throw new IOException("bin\\vivado.bat was not found: " + path);
        lock (gate)
        {
            var paths = CustomPaths().Where(item => !Paths.Same(item, path)).Concat(new[] { path }).ToArray();
            Directory.CreateDirectory(DirectoryPath);
            var temp = Path.Combine(DirectoryPath, "paths.txt.tmp");
            File.WriteAllLines(temp, paths, new UTF8Encoding(false));
            ReplaceFile(temp, Path.Combine(DirectoryPath, "paths.txt"));
            if (IsRemoved(path)) RestoreInstall(path);
        }
    }

    private static Encoding AnsiEncoding()
    {
#if !NET48
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
        return Encoding.GetEncoding((int)GetACP());
    }

    private static void ReplaceFile(string source, string destination)
    {
#if NET48
        const int replaceExisting = 1;
        if (!MoveFileEx(source, destination, replaceExisting)) throw new Win32Exception(Marshal.GetLastWin32Error());
#else
        File.Move(source, destination, true);
#endif
    }

#if NET48
    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MoveFileEx(string source, string destination, int flags);
#endif

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetPrivateProfileString(string section, string key, string fallback, StringBuilder value, int size, string file);
    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WritePrivateProfileString(string section, string key, string? value, string file);
    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    private static extern uint GetPrivateProfileSection(string section, [Out] char[] value, int size, string file);
    [DllImport("kernel32")]
    private static extern uint GetACP();
}
