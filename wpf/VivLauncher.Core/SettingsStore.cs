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
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var contents = bytes.Length >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf
            ? Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3)
            : Encoding.GetEncoding((int)GetACP()).GetString(bytes);
        var backup = IniPath + ".pre-wpf.bak";
        if (!File.Exists(backup)) File.Copy(IniPath, backup);
        var temporary = IniPath + ".tmp";
        File.WriteAllText(temporary, contents, Encoding.Unicode);
        File.Move(temporary, IniPath, true);
        WritePrivateProfileString(null!, null!, null, IniPath);
    }

    public IReadOnlyList<string> ReadSectionValues(string section)
    {
        var buffer = new char[65536];
        var count = GetPrivateProfileSection(section, buffer, buffer.Length, IniPath);
        if (count >= buffer.Length - 2) throw new IOException("Settings section exceeds the supported size.");
        return new string(buffer, 0, (int)count).Split('\0', StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.Contains('=')).Select(line => line[(line.IndexOf('=') + 1)..]).ToArray();
    }

    public IReadOnlyList<RecentProject> Recent()
    {
        var result = new List<RecentProject>();
        for (var index = 0; index < 20; index++)
        {
            var value = Read("recent_projects", $"recent_{index}");
            if (value.Length == 0) continue;
            var separator = value.IndexOf('|');
            var path = separator < 0 ? value : value[(separator + 1)..];
            try
            {
                path = Paths.Absolute(path);
                if (!result.Any(item => Paths.Same(item.Path, path)))
                    result.Add(new(path, separator < 0 ? "" : value[..separator]));
            }
            catch (ArgumentException) { }
        }
        return result;
    }

    public void Remember(string path, string version)
    {
        lock (gate)
        {
            var recent = Recent().Where(item => !Paths.Same(item.Path, path)).Prepend(new(path, version)).Take(20).ToArray();
            for (var index = 0; index < 20; index++)
                Write("recent_projects", $"recent_{index}", index < recent.Length ? $"{recent[index].Version}|{recent[index].Path}" : null);
        }
    }
    public void ClearRecent()
    {
        for (var index = 0; index < 20; index++) Write("recent_projects", $"recent_{index}", null);
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
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            contents = File.ReadAllText(file, Encoding.GetEncoding((int)GetACP()));
        }
        return contents.Split('\n').Select(line => line.Trim()).Where(line => line.Length > 0 && !line.StartsWith('#')).ToArray();
    }
    public void AddPath(string path)
    {
        path = Paths.InstallRoot(path);
        if (!File.Exists(Path.Combine(path, "bin", "vivado.bat"))) throw new IOException("bin\\vivado.bat was not found: " + path);
        lock (gate)
        {
            var paths = CustomPaths().Where(item => !Paths.Same(item, path)).Append(path).ToArray();
            Directory.CreateDirectory(DirectoryPath);
            var temp = Path.Combine(DirectoryPath, "paths.txt.tmp");
            File.WriteAllLines(temp, paths, new UTF8Encoding(false));
            File.Move(temp, Path.Combine(DirectoryPath, "paths.txt"), true);
        }
    }

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
