using CommunityToolkit.Mvvm.ComponentModel;
using VivLauncher.Core;

namespace VivLauncher.App;

public sealed record Highlight(string Text, IReadOnlySet<int> Positions);

public sealed class ProjectRow(RecentProject project, ProjectBinding binding, bool available) : ObservableObject
{
    public string Path => project.Path;
    public string Name => System.IO.Path.GetFileName(Path);
    public string Directory => System.IO.Path.GetDirectoryName(Path) ?? Path;
    public string LastVersion => project.Version;
    public ProjectBinding Binding { get; set; } = binding;
    public bool Available { get; set; } = available;
    public bool Warning { get; private set; }
    public string VersionText { get; private set; } = "";
    public string WarningText { get; private set; } = "";
    public string StatusText { get; private set; } = "";
    public Highlight NameHighlight { get; private set; } = new(System.IO.Path.GetFileName(project.Path), new HashSet<int>());
    public Highlight DirectoryHighlight { get; private set; } = new(System.IO.Path.GetDirectoryName(project.Path) ?? "", new HashSet<int>());
    public void Refresh(IReadOnlyList<Installation> installs, Localizer text)
    {
        var bound = Binding.Resolve(installs);
        var bindingMissing = Binding.Exists && bound?.Available != true;
        var historyMissing = !Binding.Exists && LastVersion.Length > 0 && !installs.Any(item => item.Available && item.Version == LastVersion);
        Warning = bindingMissing || historyMissing || !Available;
        VersionText = Binding.Exists ? bound?.Version ?? (Binding.LegacyVersion.Length > 0 ? Binding.LegacyVersion : Paths.VersionLabel(Binding.InstallPath)) : LastVersion;
        WarningText = !Available ? text["UnavailableProject"] : bindingMissing ? text["UnavailableBinding"] : historyMissing ? text["UnavailableHistory"] : "";
        StatusText = (Binding.Exists ? text["BoundVersion"] : text["LastUsed"]) + " " + VersionText;
        OnPropertyChanged(string.Empty);
    }
    public void SetMatches(IReadOnlySet<int> positions)
    {
        var normalizedPath = Path.Normalize();
        var normalizedName = Name.Normalize();
        var normalizedDirectory = Directory.Normalize();
        NameHighlight = new(normalizedName, positions.Where(index => index >= normalizedPath.Length - normalizedName.Length).Select(index => index - normalizedPath.Length + normalizedName.Length).ToHashSet());
        DirectoryHighlight = new(normalizedDirectory, positions.Where(index => index < normalizedDirectory.Length).ToHashSet());
        OnPropertyChanged(nameof(NameHighlight));
        OnPropertyChanged(nameof(DirectoryHighlight));
    }
}

public sealed class InstallRow(Installation installation, Localizer text, string defaultPath)
{
    public Installation Model => installation;
    public string Path => installation.Path;
    public string Version => installation.Version;
    public bool Available => installation.Available;
    public bool Unavailable => !Available;
    public bool IsDefault => Paths.Same(Path, defaultPath);
    public string Label => $"Vivado {Version}" + (!Available ? " · " + text["Unavailable"] : IsDefault ? " (" + text["Default"] + ")" : "");
    public string Status => !Available ? "⚠ " + text["InstallMissing"] : IsDefault ? text["Default"] : text["Available"];
}
