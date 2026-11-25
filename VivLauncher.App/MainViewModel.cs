using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VivLauncher.Core;

namespace VivLauncher.App;

public partial class MainViewModel : ObservableObject
{
    public SettingsStore Store { get; }
    public LaunchService Launcher { get; }
    public Localizer Text { get; } = new();
    public ObservableCollection<ProjectRow> Projects { get; } = [];
    public ObservableCollection<InstallRow> Installs { get; } = [];
    private List<ProjectRow> history = [];
    private IReadOnlyList<Installation> installations = [];
    private string defaultPath = "";
    private string selectionReason = "LatestSource";
    private bool automaticSelection;
    private int queryRevision;
    private bool initialized;
    private bool processCreated;
    private bool resolvingProject;
    private string? lastRemovedPath;
    private RecentRemoval? recentRemoval;
    private readonly bool isolated;
    [ObservableProperty] private string query = "";
    [ObservableProperty] private ProjectRow? selectedProject;
    [ObservableProperty] private InstallRow? selectedInstall;
    [ObservableProperty] private bool scanning;
    [ObservableProperty] private bool launching;
    [ObservableProperty] private bool noLog = true;
    [ObservableProperty] private bool noJournal = true;
    [ObservableProperty] private string extraArgs = "";
    [ObservableProperty] private int themeIndex;
    [ObservableProperty] private string notice = "";
    public event Action? CloseRequested;
    public Func<string, string, Task<bool>> Confirm { get; set; } = (_, _) => Task.FromResult(false);

    public MainViewModel(SettingsStore store, LaunchService launcher, bool isolated = false)
    {
        Store = store;
        Launcher = launcher;
        this.isolated = isolated;
        Text.SetLanguage(Store.Read("wpf", "language", Localizer.SystemLanguage));
        themeIndex = Store.Read("wpf", "theme", "system") switch { "light" => 1, "dark" => 2, _ => 0 };
        var options = Store.Options();
        noLog = options.NoLog;
        noJournal = options.NoJournal;
        extraArgs = options.ExtraArgs;
        initialized = true;
    }
    public bool CanLaunch => !Scanning && !Launching && !resolvingProject && !processCreated && SelectedProject?.Available == true && SelectedInstall?.Available == true;
    public bool CanBind => !Launching && SelectedProject is not null && (SelectedProject.Binding.Exists || SelectedInstall?.Available == true);
    public bool HasProject => SelectedProject is not null;
    public bool IsEmpty => Projects.Count == 0;
    public bool IsIdle => !Scanning && !Launching;
    public bool CanUndoRemoval => lastRemovedPath is not null;
    public int UnavailableCount => history.Count(item => !item.Available);
    public bool CanCleanUnavailable => UnavailableCount > 0 && IsIdle;
    public bool CanUndoRecentRemoval => recentRemoval is not null && IsIdle;
    public string CleanLabel => Text.Format("CleanUnavailable", UnavailableCount);
    public string EmptyTitle => Text[Query.Length == 0 ? "Empty" : "NoResults"];
    public string EmptyHint => Text[Query.Length == 0 ? "EmptyHint" : "NoResultsHint"];
    public string ProjectCount => Text.Format(Query.Length == 0 ? "Count" : "Matches", Projects.Count);
    public string InstallCount => Text.Format("InstallCount", Installs.Count(item => item.Available));
    public string LastUsed => SelectedProject?.LastVersion.Length > 0 ? $"{Text["LastUsed"]} {SelectedProject.LastVersion}" : Text["NeverUsed"];
    public string BindLabel => Text[SelectedProject?.Binding.Exists == true ? "Unbind" : "Bind"];
    public bool SelectionWarning => SelectedProject?.Binding.Exists == true
        && (SelectedInstall is null || !Paths.Same(SelectedProject.Binding.Resolve(installations)?.Path, SelectedInstall.Path));
    public string SelectionSource => SelectionWarning ? Text[SelectedInstall is null ? "MissingBinding" : "Mismatch"]
        : Installs.All(item => !item.Available) ? Text["NoInstalls"] : Text[selectionReason];
    public string Status => Notice.Length > 0 ? Notice : Text[Launching ? "Launching" : Scanning ? "Scanning" : processCreated ? "LaunchCreated" : SelectedProject?.Available == false ? "ProjectMissing" : SelectedInstall is null && SelectedProject is not null ? "VersionRequired" : "Ready"];
    public string StatusHint => Text[SelectedProject is null ? "ChooseProject" : "ReadyHint"];
    public bool StatusWarning => Notice.Length > 0 || SelectedProject?.Available == false || SelectionWarning;

    public async Task InitializeAsync(string? project)
    {
        var recent = await Task.Run(() => Store.Recent().Select(item => new ProjectRow(item, Store.Binding(item.Path), File.Exists(item.Path))).ToList());
        history = recent;
        ApplyFilter();
        if (project is { Length: > 0 }) await SelectPathAsync(project);
        await ScanAsync();
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        if (Scanning) return;
        Scanning = true;
        Notice = "";
        try
        {
            var paths = history.Concat(Projects).Select(item => item.Path).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            var result = await Task.Run(() =>
            {
                var installs = new InstallationDiscovery(Store).Scan(!isolated);
                return (Installs: installs, Availability: paths.ToDictionary(path => path, File.Exists, StringComparer.OrdinalIgnoreCase));
            });
            var removedVersions = new HashSet<string>(Store.RemovedInstalls().Select(Paths.VersionLabel));
            if (lastRemovedPath is not null && !Store.IsRemoved(lastRemovedPath)) lastRemovedPath = null;
            var historicalVersions = history.Concat(Projects).SelectMany(item => new[] { item.LastVersion, item.Binding.LegacyVersion })
                .Where(version => version.Length > 0 && !removedVersions.Contains(version) && !result.Installs.Any(install => install.Version == version)).Distinct();
            installations = result.Installs.Concat(historicalVersions.Select(version => new Installation("", version, false)))
                .OrderByDescending(item => Paths.VersionNumber(item.Version)).ThenBy(item => item.Path, StringComparer.OrdinalIgnoreCase).ToArray();
            defaultPath = Store.DefaultPath(installations);
            foreach (var item in history.Concat(Projects).Distinct())
                if (result.Availability.TryGetValue(item.Path, out var exists)) item.Available = exists;
            foreach (var install in installations.Where(item => item.Available)) Store.Write("known_installs", Paths.ProjectKey(install.Path), install.Path);
            RefreshInstallRows(true);
            RefreshProjects();
        }
        catch (Exception error) { Report(error); }
        finally { Scanning = false; Refresh(); }
    }
    partial void OnScanningChanged(bool value) => Refresh();
    partial void OnLaunchingChanged(bool value) => Refresh();
    partial void OnNoticeChanged(string value) => Refresh();
    partial void OnQueryChanged(string value)
    {
        queryRevision++;
        Notice = "";
        resolvingProject = Paths.IsProject(value);
        if (resolvingProject)
        {
            Projects.Clear();
            SelectedProject = null;
            Refresh();
            _ = SelectTypedPathAsync(value, queryRevision);
        }
        else ApplyFilter();
    }
    private async Task SelectTypedPathAsync(string path, int revision)
    {
        try
        {
            var item = await LoadProjectAsync(path);
            if (revision != queryRevision) return;
            item.Refresh(installations, Text);
            Projects.Clear();
            Projects.Add(item);
            SelectedProject = item;
            Refresh();
        }
        catch (Exception error) { if (revision == queryRevision) Report(error); }
        finally { if (revision == queryRevision) { resolvingProject = false; Refresh(); } }
    }
    private Task<ProjectRow> LoadProjectAsync(string path)
    {
        path = Paths.Absolute(path);
        if (!Paths.IsProject(path)) throw new ArgumentException(Text["InvalidDrop"]);
        var version = history.FirstOrDefault(item => Paths.Same(item.Path, path))?.LastVersion ?? "";
        return Task.Run(() => new ProjectRow(new(path, version), Store.Binding(path), File.Exists(path)));
    }
    public async Task SelectPathAsync(string path)
    {
        try
        {
            var item = await LoadProjectAsync(path);
            Query = "";
            if (!history.Any(previous => Paths.Same(previous.Path, item.Path))) history.Insert(0, item);
            else
            {
                var previous = history.First(previous => Paths.Same(previous.Path, item.Path));
                previous.Available = item.Available;
                previous.Binding = item.Binding;
                item = previous;
            }
            ApplyFilter();
            SelectedProject = item;
        }
        catch (Exception error) { Report(error); }
    }
    private void ApplyFilter()
    {
        var results = history.Select(item => (Item: item, Match: FzfMatcher.Match(item.Path, Query)))
            .Where(result => result.Match is not null).OrderByDescending(result => result.Match!.Score).ToArray();
        Projects.Clear();
        foreach (var result in results)
        {
            result.Item.SetMatches(result.Match!.Positions);
            result.Item.Refresh(installations, Text);
            Projects.Add(result.Item);
        }
        SelectedProject = Projects.FirstOrDefault();
        Refresh();
    }
    partial void OnSelectedProjectChanged(ProjectRow? value)
    {
        PickInstallation();
        Refresh();
    }
    partial void OnSelectedInstallChanged(InstallRow? value)
    {
        if (!automaticSelection) selectionReason = "ManualSource";
        Refresh();
    }
    private void PickInstallation()
    {
        var selected = Selection.Choose(SelectedProject?.Binding ?? new(), defaultPath, installations);
        selectionReason = selected.Reason;
        automaticSelection = true;
        SelectedInstall = Installs.FirstOrDefault(item => Paths.Same(item.Path, selected.Install?.Path));
        automaticSelection = false;
    }
    private void RefreshInstallRows(bool preserveManual)
    {
        var current = SelectedInstall?.Path;
        var manual = preserveManual && selectionReason == "ManualSource";
        automaticSelection = true;
        Installs.Clear();
        foreach (var install in installations) Installs.Add(new(install, Text, defaultPath));
        if (manual) SelectedInstall = Installs.FirstOrDefault(item => item.Available && Paths.Same(item.Path, current));
        else PickInstallation();
        automaticSelection = false;
        Refresh();
    }
    private void RefreshProjects()
    {
        foreach (var item in history.Concat(Projects).Distinct()) item.Refresh(installations, Text);
        Refresh();
    }
    [RelayCommand]
    private void Bind()
    {
        if (!CanBind || SelectedProject is null) return;
        try
        {
            Store.Bind(SelectedProject.Path, SelectedProject.Binding.Exists ? null : SelectedInstall!.Path);
            var binding = Store.Binding(SelectedProject.Path);
            foreach (var project in history.Concat(Projects).Where(item => Paths.Same(item.Path, SelectedProject.Path)).Distinct())
                project.Binding = binding;
            RefreshProjects();
            PickInstallation();
            Notice = Text["Saved"];
        }
        catch (Exception error) { Report(error); }
    }
    [RelayCommand]
    private void SetDefault(InstallRow? item)
    {
        if (item?.Available != true) return;
        try
        {
            Store.Write("vivlauncher", "default_install", item.Path);
            defaultPath = item.Path;
            RefreshInstallRows(true);
            Notice = Text["Saved"];
        }
        catch (Exception error) { Report(error); }
    }
    public async Task AddPathAsync(string path)
    {
        try { await Task.Run(() => Store.AddPath(path)); await ScanAsync(); }
        catch (Exception error) { Report(error); }
    }
    public async Task RemoveInstallAsync(InstallRow item, Func<string, string, bool> confirm)
    {
        if (!IsIdle || !item.Removable) return;
        try
        {
            var bound = Store.ReadSectionValues("project_installs").Count(path => Paths.Same(path, item.Path));
            var recent = Store.Recent().Count(project => project.Version == item.Version);
            if (!confirm(Text["RemoveTitle"], Text.Format("RemoveBody", item.Version, item.Path, bound, recent))) return;
            Store.RemoveInstall(item.Model, defaultPath);
            lastRemovedPath = item.Path;
            await ScanAsync();
        }
        catch (Exception error) { Report(error); }
    }
    [RelayCommand]
    private async Task UndoRemovalAsync()
    {
        if (!IsIdle || lastRemovedPath is null) return;
        try
        {
            Store.RestoreInstall(lastRemovedPath);
            lastRemovedPath = null;
            await ScanAsync();
        }
        catch (Exception error) { Report(error); }
    }
    [RelayCommand]
    private async Task ClearRecentAsync()
    {
        if (!await Confirm(Text["ClearTitle"], Text["ClearBody"])) return;
        try { Store.ClearRecent(); history.Clear(); Query = ""; ApplyFilter(); }
        catch (Exception error) { Report(error); }
    }
    [RelayCommand]
    private async Task CleanUnavailableRecentAsync()
    {
        if (!CanCleanUnavailable) return;
        var paths = history.Where(item => !item.Available).Select(item => item.Path).ToArray();
        if (paths.Length == 0) { Notice = Text["CleanUnavailableNone"]; return; }
        var preview = string.Join("\n", paths.Take(3));
        if (paths.Length > 3) preview += "\n" + Text.Format("CleanUnavailableMore", paths.Length - 3);
        if (!await Confirm(Text["CleanUnavailableTitle"], Text.Format("CleanUnavailableBody", paths.Length, preview))) return;
        RemoveUnavailable(paths);
    }
    [RelayCommand]
    private async Task RemoveUnavailableRecentAsync(ProjectRow? item)
    {
        if (item is null || item.Available || !IsIdle) return;
        if (!await Confirm(Text["RemoveProjectTitle"], Text.Format("CleanUnavailableBody", 1, item.Path))) return;
        RemoveUnavailable([item.Path]);
    }
    [RelayCommand]
    private void UndoRecentRemoval()
    {
        if (!CanUndoRecentRemoval || recentRemoval is null) return;
        var removal = recentRemoval;
        try
        {
            recentRemoval = null;
            if (!Store.RestoreRecent(removal)) { Notice = Text["CleanRestoreFailed"]; Refresh(); return; }
            history = removal.Before.Select(item => new ProjectRow(item, Store.Binding(item.Path), File.Exists(item.Path))).ToList();
            Query = "";
            ApplyFilter();
            Notice = Text["CleanRestored"];
        }
        catch (Exception error) { Report(error); }
    }
    private void RemoveUnavailable(IReadOnlyList<string> paths)
    {
        try
        {
            var removal = Store.RemoveUnavailableRecent(paths);
            if (removal is null) { Notice = Text["CleanUnavailableNone"]; Revalidate(); return; }
            recentRemoval = removal;
            var removed = removal.Removed.Select(item => item.Path).ToArray();
            history.RemoveAll(item => removed.Any(path => Paths.Same(path, item.Path)));
            Query = "";
            ApplyFilter();
            Notice = Text.Format("CleanUnavailableDone", removed.Length);
        }
        catch (Exception error) { Report(error); }
    }
    private void Revalidate()
    {
        foreach (var item in history) item.Available = File.Exists(item.Path);
        ApplyFilter();
    }
    [RelayCommand]
    private async Task LaunchAsync()
    {
        if (!CanLaunch || SelectedProject is null || SelectedInstall is null) return;
        Launching = true;
        Notice = "";
        var project = SelectedProject;
        var install = SelectedInstall.Model;
        try
        {
            if (SelectionWarning && !await Confirm(Text["MismatchTitle"], $"{Text["MismatchBody"]}\n\n{project.StatusText}\n→ {install.Version}\n{install.Path}")) return;
            var options = new LaunchOptions(NoLog, NoJournal, ExtraArgs);
            Store.SaveOptions(options);
            await Task.Run(() => Launcher.Launch(install, project.Path, options));
            processCreated = true;
            try { Store.Remember(project.Path, install.Version); }
            catch (Exception error) { Notice = Text["SaveFailed"] + "\n" + error.Message; return; }
            CloseRequested?.Invoke();
        }
        catch (Exception error) { Notice = Text["LaunchFailed"] + "\n" + error.Message; }
        finally { Launching = false; Refresh(); }
    }
    public string Language
    {
        get => Text.Language;
        set
        {
            if (!Text.SetLanguage(value)) return;
            OnPropertyChanged();
            SavePreference("language", Text.Language);
            RefreshInstallRows(true);
            RefreshProjects();
        }
    }
    partial void OnThemeIndexChanged(int value)
    {
        ThemeService.Apply(value);
        SavePreference("theme", value switch { 1 => "light", 2 => "dark", _ => "system" });
    }
    private void SavePreference(string key, string value)
    {
        if (!initialized) return;
        try { Store.Write("wpf", key, value); }
        catch (Exception error) { Report(error); }
    }
    public void Report(Exception error) => Notice = Text["Error"] + ": " + error.Message;
    private void Refresh()
    {
        OnPropertyChanged(nameof(CanUndoRemoval));
        OnPropertyChanged(nameof(UnavailableCount));
        OnPropertyChanged(nameof(CanCleanUnavailable));
        OnPropertyChanged(nameof(CanUndoRecentRemoval));
        OnPropertyChanged(nameof(CleanLabel));
        foreach (var name in new[] { nameof(CanLaunch), nameof(CanBind), nameof(HasProject), nameof(IsEmpty), nameof(IsIdle), nameof(EmptyTitle), nameof(EmptyHint), nameof(ProjectCount), nameof(InstallCount), nameof(LastUsed), nameof(BindLabel), nameof(SelectionWarning), nameof(SelectionSource), nameof(Status), nameof(StatusHint), nameof(StatusWarning) }) OnPropertyChanged(name);
    }
}
