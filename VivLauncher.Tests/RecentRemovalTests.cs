using System.Text;
using VivLauncher.Core;
using Xunit;

namespace VivLauncher.Tests;

public sealed class RecentRemovalTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "vivlauncher-tests", Guid.NewGuid().ToString("N"));
    private readonly SettingsStore store;

    public RecentRemovalTests()
    {
        Directory.CreateDirectory(directory);
        store = new(directory);
    }

    public void Dispose() => Directory.Delete(directory, true);

    [Fact]
    public void RemovalUsesFullPathAndPreservesFilesBindingsVersionsAndOtherSettings()
    {
        var missing = Path.Combine(directory, "设计", "project_1.xpr");
        var available = Path.Combine(directory, "zu15", "project_1.xpr");
        Directory.CreateDirectory(Path.GetDirectoryName(available)!);
        File.WriteAllText(available, "<Project />");
        var install = Path.Combine(directory, "missing Vivado", "2023.2");
        store.Bind(missing, install);
        store.Bind(available, install);
        store.Write("custom", "retain", "保留");
        store.Write("recent_projects", "custom", "keep");
        store.Remember(available, "2022.2");
        store.Remember(missing, "2023.2");
        var before = store.Recent().ToArray();

        var removal = store.RemoveUnavailableRecent([missing.ToUpperInvariant(), available]);

        Assert.NotNull(removal);
        Assert.Equal(missing, Assert.Single(removal.Removed).Path);
        Assert.Equal(available, Assert.Single(store.Recent()).Path);
        Assert.Equal("2022.2", Assert.Single(store.Recent()).Version);
        Assert.True(File.Exists(available));
        Assert.Equal(install, store.Binding(missing).InstallPath);
        Assert.Equal(install, store.Binding(available).InstallPath);
        Assert.Equal("保留", store.Read("custom", "retain"));
        Assert.Equal("keep", store.Read("recent_projects", "custom"));
        Assert.True(store.RestoreRecent(removal));
        Assert.Equal(before, store.Recent());
        Assert.Empty(Directory.GetFiles(directory, "*.tmp"));
    }

    [Fact]
    public void BatchRemovalAndUndoPreserveOriginalOrderAndLegacyBindings()
    {
        var paths = Enumerable.Range(0, 5).Select(index => Path.Combine(directory, $"project{index}.xpr")).ToArray();
        foreach (var path in paths) store.Remember(path, Path.GetFileNameWithoutExtension(path));
        store.Write("project_versions", Paths.ProjectKey(paths[2]), "2023.2");
        var before = store.Recent().ToArray();

        var removal = store.RemoveUnavailableRecent([paths[0], paths[2], paths[4]]);

        Assert.NotNull(removal);
        Assert.Equal(new[] { paths[3], paths[1] }, store.Recent().Select(item => item.Path));
        store.Write("wpf", "language", "en");
        Assert.True(store.RestoreRecent(removal));
        Assert.Equal(before, store.Recent());
        Assert.Equal("2023.2", store.Binding(paths[2]).LegacyVersion);
        Assert.Equal("en", store.Read("wpf", "language"));
    }

    [Fact]
    public void RecoveredAndUnknownPathsAreSkippedWithoutChangingSettings()
    {
        var path = Path.Combine(directory, "recovered.xpr");
        store.Remember(path, "2025.2");
        var original = File.ReadAllBytes(store.IniPath);
        File.WriteAllText(path, "<Project />");

        Assert.Null(store.RemoveUnavailableRecent([path, Path.Combine(directory, "never-remembered.xpr")]));
        Assert.Equal(original, File.ReadAllBytes(store.IniPath));
        Assert.Single(store.Recent());
        Assert.Null(store.RemoveUnavailableRecent([]));
    }

    [Fact]
    public void UndoRejectsHistoryChangedByAnotherStoreAndKeepsNewRecords()
    {
        var path = Path.Combine(directory, "missing.xpr");
        store.Remember(path, "2023.2");
        var removal = store.RemoveUnavailableRecent([path]);
        Assert.NotNull(removal);
        var otherStore = new SettingsStore(directory);
        var added = Path.Combine(directory, "new.xpr");
        otherStore.Remember(added, "2025.2");

        Assert.False(store.RestoreRecent(removal));
        Assert.Equal(added, Assert.Single(store.Recent()).Path);
        otherStore.ClearRecent();
        otherStore.Remember(path, "2024.2");
        Assert.False(store.RestoreRecent(removal));
        Assert.Equal("2024.2", Assert.Single(store.Recent()).Version);
    }

    [Fact]
    public void RemovalFromReadOnlySettingsFailsWithoutPartialChanges()
    {
        var path = Path.Combine(directory, "missing.xpr");
        store.Remember(path, "2023.2");
        var original = File.ReadAllBytes(store.IniPath);
        File.SetAttributes(store.IniPath, FileAttributes.ReadOnly);
        try
        {
            Assert.ThrowsAny<Exception>(() => store.RemoveUnavailableRecent([path]));
            Assert.Equal(original, File.ReadAllBytes(store.IniPath));
            Assert.Single(store.Recent());
            Assert.Empty(Directory.GetFiles(directory, "*.tmp"));
        }
        finally { File.SetAttributes(store.IniPath, FileAttributes.Normal); }
    }

    [Fact]
    public void AnsiHistoryRemovalKeepsBackupAndUnrelatedValues()
    {
        var path = Path.Combine(directory, "missing.xpr");
        File.WriteAllText(store.IniPath, $"[custom]\r\nretain=yes\r\n[recent_projects]\r\nrecent_0=2024.2|{path}\r\n", Encoding.ASCII);

        var removal = store.RemoveUnavailableRecent([path]);

        Assert.NotNull(removal);
        Assert.Empty(store.Recent());
        Assert.True(File.Exists(store.IniPath + ".pre-wpf.bak"));
        Assert.Equal("yes", store.Read("custom", "retain"));
        Assert.True(store.RestoreRecent(removal));
        Assert.Equal(path, Assert.Single(store.Recent()).Path);
    }
}
