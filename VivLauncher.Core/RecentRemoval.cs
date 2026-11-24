namespace VivLauncher.Core;

public sealed class RecentRemoval
{
    internal string SettingsPath { get; }
    internal IReadOnlyList<RecentProject> Before { get; }
    public IReadOnlyList<RecentProject> Remaining { get; }
    public IReadOnlyList<RecentProject> Removed { get; }

    internal RecentRemoval(string settingsPath, RecentProject[] before, RecentProject[] remaining, RecentProject[] removed)
    {
        SettingsPath = settingsPath;
        Before = Array.AsReadOnly(before);
        Remaining = Array.AsReadOnly(remaining);
        Removed = Array.AsReadOnly(removed);
    }
}
