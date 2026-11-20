using Microsoft.Win32;

namespace VivLauncher.Core;

public sealed class InstallationDiscovery(SettingsStore settings)
{
    public IReadOnlyList<Installation> Scan(bool includeMachine = true)
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var removed = new HashSet<string>(settings.RemovedInstalls(), StringComparer.OrdinalIgnoreCase);
        void Add(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            try { paths.Add(Paths.InstallRoot(value)); }
            catch (Exception error) when (error is ArgumentException or NotSupportedException or IOException) { }
        }
        foreach (var value in settings.CustomPaths()) Add(value);
        foreach (var value in settings.ReadSectionValues("project_installs")) Add(value);
        foreach (var value in settings.ReadSectionValues("known_installs")) Add(value);
        foreach (var value in removed) Add(value);
        Add(settings.Read("vivlauncher", "default_install"));
        if (includeMachine)
        {
            foreach (var drive in DriveInfo.GetDrives().Where(item => item.DriveType == DriveType.Fixed))
            {
                ScanChildren(Path.Combine(drive.Name, "Xilinx", "Vivado"), "", Add);
                ScanChildren(Path.Combine(drive.Name, "Xilinx"), "Vivado", Add);
                ScanChildren(Path.Combine(drive.Name, "AMDDesignTools"), "Vivado", Add);
            }
            foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                try
                {
                    using var machine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                    using var vivado = machine.OpenSubKey(@"SOFTWARE\Xilinx\Vivado");
                    if (vivado?.GetValue("InstallPath") is string root) Add(root);
                    foreach (var name in vivado?.GetSubKeyNames() ?? [])
                    {
                        using var version = vivado!.OpenSubKey(name);
                        if (version?.GetValue("InstallPath") is string path) Add(path);
                    }
                }
                catch (System.Security.SecurityException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
        return paths.Select(path => new Installation(path, Paths.VersionLabel(path), File.Exists(Path.Combine(path, "bin", "vivado.bat"))))
            .Where(install =>
            {
                if (!removed.Contains(install.Path)) return true;
                if (!install.Available) return false;
                settings.RestoreInstall(install.Path);
                return true;
            })
            .OrderByDescending(item => Paths.VersionNumber(item.Version)).ThenBy(item => item.Path, StringComparer.OrdinalIgnoreCase).ToArray();
    }
    private static void ScanChildren(string root, string suffix, Action<string> add)
    {
        try
        {
            if (!Directory.Exists(root)) return;
            foreach (var directory in Directory.EnumerateDirectories(root))
            {
                var path = Path.Combine(directory, suffix);
                if (File.Exists(Path.Combine(path, "bin", "vivado.bat"))) add(path);
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
