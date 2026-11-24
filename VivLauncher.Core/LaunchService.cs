using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace VivLauncher.Core;

public sealed class LaunchService(string? logPath = null)
{
    public string LogPath { get; } = logPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "vivlauncher", "vivlauncher.log");
    public static IReadOnlyList<string> ParseArguments(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments)) return [];
        if (arguments.Any(character => character is '\r' or '\n' or '\0')) throw new ArgumentException("Arguments must be on a single line.");
        var pointer = CommandLineToArgvW("vivlauncher " + arguments, out var count);
        if (pointer == IntPtr.Zero) throw new System.ComponentModel.Win32Exception();
        try
        {
            return Enumerable.Range(1, count - 1).Select(index => Marshal.PtrToStringUni(Marshal.ReadIntPtr(pointer, index * IntPtr.Size))!).ToArray();
        }
        finally { LocalFree(pointer); }
    }
    public static ProcessStartInfo BuildStartInfo(Installation installation, string project, LaunchOptions options, bool? useWrapper = null)
    {
        var wrapper = Path.Combine(installation.Path, "bin", "unwrapped", "win64.o", "vvgl.exe");
        var arguments = new List<string>();
        if (options.NoLog) arguments.Add("-nolog");
        if (options.NoJournal) arguments.Add("-nojournal");
        arguments.AddRange(ParseArguments(options.ExtraArgs));
        arguments.Add(Paths.Absolute(project));
        var start = new ProcessStartInfo { UseShellExecute = false, WorkingDirectory = Path.GetDirectoryName(Paths.Absolute(project))!, CreateNoWindow = true };
        if (useWrapper ?? File.Exists(wrapper))
        {
            start.FileName = wrapper;
            arguments.Insert(0, installation.BatchPath);
#if NET48
            start.Arguments = FormatCommandLine(arguments);
#else
            foreach (var argument in arguments) start.ArgumentList.Add(argument);
#endif
        }
        else
        {
            start.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");
            arguments.Insert(0, installation.BatchPath);
            if (arguments.Any(argument => argument.Contains('"')))
                throw new ArgumentException("This installation has no vvgl.exe; embedded quotes in arguments are not supported by its batch fallback.");
            var prefix = "VIVLAUNCHER_" + Guid.NewGuid().ToString("N") + "_";
            for (var index = 0; index < arguments.Count; index++) start.EnvironmentVariables[prefix + index] = arguments[index];
            start.Arguments = "/d /s /v:off /c \"" + string.Join(" ", Enumerable.Range(0, arguments.Count).Select(index => $"\"%{prefix}{index}%\"")) + "\"";
        }
        return start;
    }
    private static readonly char[] QuotedArgumentCharacters = [' ', '\t', '"'];
    public static string FormatCommandLine(IEnumerable<string> arguments)
    {
        var builder = new StringBuilder();
        foreach (var argument in arguments) AppendArgument(builder, argument);
        return builder.ToString();
    }
    private static void AppendArgument(StringBuilder builder, string argument)
    {
        if (builder.Length > 0) builder.Append(' ');
        if (argument.Length > 0 && argument.IndexOfAny(QuotedArgumentCharacters) < 0)
        {
            builder.Append(argument);
            return;
        }
        builder.Append('"');
        for (var index = 0; index < argument.Length;)
        {
            var character = argument[index++];
            if (character == '\\')
            {
                var slashes = 1;
                while (index < argument.Length && argument[index] == '\\') { slashes++; index++; }
                if (index == argument.Length) builder.Append('\\', slashes * 2);
                else if (argument[index] == '"') { builder.Append('\\', slashes * 2 + 1).Append('"'); index++; }
                else builder.Append('\\', slashes);
            }
            else if (character == '"') builder.Append('\\').Append('"');
            else builder.Append(character);
        }
        builder.Append('"');
    }
    public void Launch(Installation installation, string project, LaunchOptions options)
    {
        try
        {
            if (!File.Exists(project)) throw new FileNotFoundException("Project is unavailable.", project);
            if (!File.Exists(installation.BatchPath)) throw new FileNotFoundException("Vivado installation is unavailable.", installation.BatchPath);
            var start = BuildStartInfo(installation, project, options);
            var command = start.Arguments;
#if !NET48
            if (start.ArgumentList.Count > 0) command = FormatCommandLine(start.ArgumentList);
#endif
            Log($"Project: {project}\nInstallation: {installation.Path}\nWorking directory: {start.WorkingDirectory}\nExecutable: {start.FileName}\nArguments: {command}\nOptions: {options}");
            using var process = Process.Start(start) ?? throw new IOException("Windows did not create a process.");
            Log($"Process created: {process.Id}");
        }
        catch (Exception error)
        {
            Log("Launch failed: " + error);
            throw;
        }
    }
    private void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            File.AppendAllText(LogPath, $"[{DateTimeOffset.Now:O}] {message}\n", Encoding.UTF8);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
    [DllImport("shell32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CommandLineToArgvW(string command, out int count);
    [DllImport("kernel32")]
    private static extern IntPtr LocalFree(IntPtr pointer);
}
