using System.Diagnostics;
using System.Text;
#if NET48
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
#else
using System.Text.Json;
#endif
using VivLauncher.Core;
using Xunit;

namespace VivLauncher.Tests;

public sealed class CoreTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "vivlauncher-tests", Guid.NewGuid().ToString("N"));
    private SettingsStore Store => new(directory);
    public CoreTests() => Directory.CreateDirectory(directory);
    public void Dispose() => Directory.Delete(directory, true);

#if NET48
    [DataContract]
    public sealed class Fixture
    {
        [DataMember(Name = "text")] public string Text { get; set; } = "";
        [DataMember(Name = "query")] public string Query { get; set; } = "";
        [DataMember(Name = "score")] public int? Score { get; set; }
        [DataMember(Name = "positions")] public int[] Positions { get; set; } = [];
    }
#else
    public sealed record Fixture(string Text, string Query, int? Score, int[] Positions);
#endif
    public static IEnumerable<object[]> ParityFixtures()
    {
        using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "fzf-fixtures.json"));
#if NET48
        var fixtures = (Fixture[])new DataContractJsonSerializer(typeof(Fixture[])).ReadObject(stream);
#else
        var fixtures = JsonSerializer.Deserialize<Fixture[]>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
#endif
        return fixtures.Select(item => new object[] { item.Text, item.Query, item.Score ?? -1, item.Positions });
    }
    [Theory]
    [MemberData(nameof(ParityFixtures))]
    public void SearchMatchesPrototype(string text, string query, int score, int[] positions)
    {
        var result = FzfMatcher.Match(text, query);
        Assert.Equal(score, result?.Score ?? -1);
        Assert.Equal(positions, result?.Positions.OrderBy(position => position).ToArray() ?? []);
    }
    [Fact]
    public void BindingIdentityIsPathNotVersion()
    {
        var first = new Installation(@"C:\Xilinx\Vivado\2024.2", "2024.2", false);
        var second = new Installation(@"D:\Xilinx\Vivado\2024.2", "2024.2", true);
        var chosen = Selection.Choose(new(first.Path), second.Path, [first, second]);
        Assert.Null(chosen.Install);
        Assert.Equal("MissingBinding", chosen.Reason);
        Assert.Equal(second, Selection.Choose(new(), first.Path, [first, second]).Install);
    }
    [Fact]
    public void UnicodeLegacySettingsAndUnbindPreserveOtherSections()
    {
        var path = Path.Combine(directory, "设计", "工程 one.xpr");
        var key = Paths.ProjectKey(path);
        File.WriteAllText(Store.IniPath, $"[custom]\nretain=keep\n[project_versions]\n{key}=2023.2\n[recent_projects]\nrecent_0=2023.2|{path}\n[launch]\nno_log=0\nextra_args=-mode gui\n", Encoding.Unicode);
        Assert.Equal("2023.2", Store.Binding(path).LegacyVersion);
        Assert.Single(Store.Recent());
        Assert.False(Store.Options().NoLog);
        Store.Bind(path, @"C:\Xilinx\Vivado\2025.2");
        Assert.Equal(@"C:\Xilinx\Vivado\2025.2", Store.Binding(path).InstallPath);
        Store.Bind(path, null);
        Assert.False(Store.Binding(path).Exists);
        Assert.Equal("keep", Store.Read("custom", "retain"));
    }
    [Fact]
    public void RecentsKeepMissingFilesDeduplicateAndCapAtTwenty()
    {
        for (var index = 0; index < 24; index++) Store.Remember(Path.Combine(directory, $"project{index}.xpr"), "2024.2");
        Store.Remember(Path.Combine(directory, "PROJECT23.xpr"), "2025.2");
        Assert.Equal(20, Store.Recent().Count);
        Assert.Equal("2025.2", Store.Recent()[0].Version);
        Store.ClearRecent();
        Assert.Empty(Store.Recent());
    }
    [Fact]
    public void CustomPathsRetainSpacesAndUnavailableRecords()
    {
        var root = Path.Combine(directory, "AMD tools 设计", "2024.2", "Vivado");
        Directory.CreateDirectory(Path.Combine(root, "bin"));
        var batch = Path.Combine(root, "bin", "vivado.bat");
        File.WriteAllText(batch, "@exit /b 0");
        Store.AddPath(Path.Combine(root, "bin"));
        Assert.Equal(root, Assert.Single(Store.CustomPaths()));
        Assert.True(Assert.Single(new InstallationDiscovery(Store).Scan(false)).Available);
        File.Delete(batch);
        Assert.False(Assert.Single(new InstallationDiscovery(Store).Scan(false)).Available);
    }
    [Fact]
    public void LegacyPathFileAndDefaultVersionRemainReadable()
    {
        var root = Path.Combine(directory, "Vivado", "2023.2");
        File.WriteAllText(Path.Combine(directory, "paths.json"), root);
        Store.Write("vivlauncher", "default_version", "2023.2");
        var installs = new InstallationDiscovery(Store).Scan(false);
        Assert.Equal(root, Assert.Single(installs).Path);
        Assert.Equal(root, Store.DefaultPath(installs));
    }
    [Fact]
    public void RemovedInstallStaysHiddenUntilUndoOrSamePathRecovery()
    {
        var original = Path.Combine(directory, "Xilinx", "2023.2", "Vivado");
        var alternate = Path.Combine(directory, "AMD", "2023.2", "Vivado");
        var project = Path.Combine(directory, "工程.xpr");
        File.WriteAllText(Path.Combine(directory, "paths.txt"), original);
        Store.Bind(project, original);
        Store.Remember(project, "2023.2");
        Store.Write("vivlauncher", "default_install", original);
        var discovery = new InstallationDiscovery(Store);
        var missing = Assert.Single(discovery.Scan(false));
        Store.RemoveInstall(missing, original);
        Assert.Empty(discovery.Scan(false));
        Assert.Equal("", Store.DefaultPath([missing]));
        Assert.Equal(original, Store.Binding(project).InstallPath);
        Assert.Equal("2023.2", Assert.Single(Store.Recent()).Version);

        Store.RestoreInstall(original);
        Assert.False(Assert.Single(discovery.Scan(false)).Available);
        Assert.Equal("", Store.DefaultPath([missing]));
        Store.RemoveInstall(missing, "");
        Directory.CreateDirectory(Path.Combine(alternate, "bin"));
        File.WriteAllText(Path.Combine(alternate, "bin", "vivado.bat"), "@exit /b 0");
        Store.AddPath(alternate);
        var alternateOnly = discovery.Scan(false);
        Assert.Equal(alternate, Assert.Single(alternateOnly).Path);
        Assert.Equal("MissingBinding", Selection.Choose(Store.Binding(project), "", alternateOnly).Reason);

        Directory.CreateDirectory(Path.Combine(original, "bin"));
        File.WriteAllText(Path.Combine(original, "bin", "vivado.bat"), "@exit /b 0");
        var restored = discovery.Scan(false);
        Assert.False(Store.IsRemoved(original));
        Assert.Equal(original, Selection.Choose(Store.Binding(project), "", restored).Install?.Path);
    }
    [Fact]
    public void SettingsFailuresAreReported()
    {
        var file = Path.Combine(directory, "not-a-directory");
        File.WriteAllText(file, "blocked");
        Assert.Throws<IOException>(() => new SettingsStore(file).Write("wpf", "language", "en"));
    }
    [Fact]
    public void AnsiIniIsBackedUpBeforeUnicodeWrites()
    {
        File.WriteAllText(Store.IniPath, "[custom]\r\nretain=yes\r\n", Encoding.ASCII);
        Store.Write("project_installs", "test", "D:\\设计\\Vivado 2025.2");
        Assert.Equal("D:\\设计\\Vivado 2025.2", Store.Read("project_installs", "test"));
        Assert.Equal("yes", Store.Read("custom", "retain"));
        Assert.True(File.Exists(Store.IniPath + ".pre-wpf.bak"));
        Assert.Equal(new byte[] { 0xff, 0xfe }, File.ReadAllBytes(Store.IniPath).Take(2));
    }
    [Fact]
    public void WrapperArgumentsPreserveSpacesUnicodeAndQuotes()
    {
        var installation = new Installation(@"C:\AMD tools\2025.2\Vivado", "2025.2", true);
        var project = Path.Combine(directory, "设计 & test!%.xpr");
        var start = LaunchService.BuildStartInfo(installation, project, new(true, false, "-tclargs \"hello world\" \"quoted\\\"value\" \"\" \"C:\\with space\\\\\""), true);
        var expected = new[] { installation.BatchPath, "-nolog", "-tclargs", "hello world", "quoted\"value", "", @"C:\with space\", project };
#if NET48
        Assert.Equal(expected, LaunchService.ParseArguments(start.Arguments));
#else
        Assert.Equal(expected, start.ArgumentList);
#endif
        Assert.Equal(directory, start.WorkingDirectory);
        Assert.False(start.UseShellExecute);
    }
    [Fact]
    public void BatchFallbackPassesMetacharactersAsData()
    {
        var root = Path.Combine(directory, "install & spaced !%PATH%", "2025.2");
        var bin = Path.Combine(root, "bin");
        Directory.CreateDirectory(bin);
        var capture = Path.Combine(directory, "captured.txt");
        File.WriteAllText(Path.Combine(bin, "vivado.bat"), "@echo off\r\nsetlocal DisableDelayedExpansion\r\n>\"%VIV_TEST_CAPTURE%\" echo %1\r\n>>\"%VIV_TEST_CAPTURE%\" echo %2\r\n>>\"%VIV_TEST_CAPTURE%\" echo %3\r\n>>\"%VIV_TEST_CAPTURE%\" echo %4\r\n", Encoding.ASCII);
        var project = Path.Combine(directory, "project &!%PATH%.xpr");
        var start = LaunchService.BuildStartInfo(new(root, "2025.2", true), project, new(true, true, "\"hello &!%PATH%\""), false);
        start.EnvironmentVariables["VIV_TEST_CAPTURE"] = capture;
        using var process = Process.Start(start)!;
        if (!process.WaitForExit(10000)) { process.Kill(); throw new TimeoutException(); }
        Assert.Equal(0, process.ExitCode);
        var lines = File.ReadAllLines(capture);
        Assert.Equal(new[] { "\"-nolog\"", "\"-nojournal\"", "\"hello &!%PATH%\"", "\"" + project + "\"" }, lines);
    }
    [Fact]
    public void MissingProjectDoesNotCreateProcessAndLogsFailure()
    {
        var log = Path.Combine(directory, "test.log");
        Assert.Throws<FileNotFoundException>(() => new LaunchService(log).Launch(new(directory, "2025.2", true), Path.Combine(directory, "missing.xpr"), new()));
        Assert.Contains("Launch failed", File.ReadAllText(log));
    }
    [Fact]
    public void ProjectKeysAreStableAndCaseInsensitive()
    {
        Assert.Equal(Paths.ProjectKey(@"D:\Projects\hello.xpr"), Paths.ProjectKey(@"d:\projects\HELLO.xpr"));
        Assert.Equal("2025.2", Paths.VersionLabel(@"C:\AMDDesignTools\2025.2\Vivado"));
        Assert.True(Paths.Same(@"C:\Vivado\2025.2\", @"c:/vivado/2025.2"));
        Assert.False(Paths.Same(@"C:\", "C:"));
        Assert.True(Paths.Same(@"\\server\share\Vivado\", @"//SERVER/share/vivado"));
        Assert.True(Paths.IsProject(@"\\server\share\工程.xpr"));
        Assert.False(Paths.IsProject(@"C:工程.xpr"));
        Assert.False(Paths.IsProject(@"\工程.xpr"));
    }
}
