using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using VivLauncher.Core;

namespace VivLauncher.App;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs args)
    {
        base.OnStartup(args);
        var profile = Environment.GetEnvironmentVariable("VIVLAUNCHER_PROFILE");
        var isolated = Environment.GetEnvironmentVariable("VIVLAUNCHER_ISOLATED") == "1";
        var store = new SettingsStore(string.IsNullOrWhiteSpace(profile) ? null : Paths.Absolute(profile));
        var verb = args.Args.FirstOrDefault() ?? "";
        if (new[] { "-h", "--help", "help", "-l", "--list", "list", "-a", "--add", "add", "--licenses" }.Contains(verb))
        {
            AttachOutput();
            try
            {
                if (verb is "-h" or "--help" or "help")
                    Console.WriteLine("Vivado Launcher\nRequires .NET 10 Desktop Runtime (Windows x64).\nUsage: vivlauncher.exe [project.xpr]\n  -l, --list       List installations (including unavailable records)\n  -a, --add PATH   Add a Vivado installation directory\n  -h, --help       Show help\n      --licenses   Show bundled third-party licenses");
                else if (verb == "--licenses")
                {
                    foreach (var name in new[] { "Licenses.Notices.txt", "Licenses.fzf.txt", "Licenses.CommunityToolkit.Mvvm.txt" })
                    {
                        using var reader = new StreamReader(typeof(App).Assembly.GetManifestResourceStream(name)!);
                        Console.WriteLine(reader.ReadToEnd());
                    }
                }
                else if (verb is "-a" or "--add" or "add")
                {
                    if (args.Args.Length != 2) throw new ArgumentException("--add requires one installation path.");
                    store.AddPath(args.Args[1]);
                    Console.WriteLine("Added: " + Paths.InstallRoot(args.Args[1]));
                }
                else
                {
                    var installs = await Task.Run(() => new InstallationDiscovery(store).Scan(!isolated));
                    if (installs.Count == 0) Console.WriteLine("No Vivado installation found.");
                    foreach (var install in installs) Console.WriteLine($"[{install.Version}] {(install.Available ? "Available" : "Unavailable")}\n  {install.Path}");
                }
                Shutdown(0);
            }
            catch (Exception error) { Console.Error.WriteLine("Error: " + error.Message); Shutdown(1); }
            return;
        }
        try
        {
            if (args.Args.Length > 1 || verb.StartsWith('-')) throw new ArgumentException("Usage: vivlauncher.exe [project.xpr]");
            var launcher = new LaunchService(profile is null ? null : Path.Combine(store.DirectoryPath, "vivlauncher.log"));
            var model = new MainViewModel(store, launcher, isolated);
            ThemeService.Initialize();
            ThemeService.Apply(model.ThemeIndex);
            var window = new MainWindow(model, verb);
            MainWindow = window;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            window.Show();
        }
        catch (Exception error) { MessageBox.Show(error.Message, "Vivado Launcher", MessageBoxButton.OK, MessageBoxImage.Error); Shutdown(1); }
    }
    private static void AttachOutput()
    {
        var stdout = GetStdHandle(-11);
        var stderr = GetStdHandle(-12);
        var outputRedirected = stdout != IntPtr.Zero && stdout != new IntPtr(-1) && GetFileType(stdout) is 1 or 3;
        var errorRedirected = stderr != IntPtr.Zero && stderr != new IntPtr(-1) && GetFileType(stderr) is 1 or 3;
        if (!outputRedirected || !errorRedirected)
        {
            AttachConsole(unchecked((uint)-1));
            if (outputRedirected) SetStdHandle(-11, stdout);
            if (errorRedirected) SetStdHandle(-12, stderr);
        }
        Console.OutputEncoding = new UTF8Encoding(false);
    }
    [DllImport("kernel32")] private static extern bool AttachConsole(uint process);
    [DllImport("kernel32")] private static extern IntPtr GetStdHandle(int handle);
    [DllImport("kernel32")] private static extern bool SetStdHandle(int handle, IntPtr value);
    [DllImport("kernel32")] private static extern uint GetFileType(IntPtr handle);
}
