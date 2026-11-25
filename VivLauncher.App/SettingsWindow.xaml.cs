using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Win32;

namespace VivLauncher.App;

public partial class SettingsWindow : Window
{
    private readonly MainViewModel model;
    public SettingsWindow(MainViewModel model) { this.model = model; DataContext = model; InitializeComponent(); }
    private void LogClick(object sender, RoutedEventArgs args) => Dialogs.Log(this, model);
    private void HelpClick(object sender, RoutedEventArgs args) => Dialogs.Show(this, model, model.Text["Help"], model.Text["HelpBody"]);
    private void ConfigClick(object sender, RoutedEventArgs args)
    {
        try { Directory.CreateDirectory(model.Store.DirectoryPath); Process.Start(new ProcessStartInfo(model.Store.DirectoryPath) { UseShellExecute = true }); }
        catch (Exception error) { model.Report(error); }
    }
    private async void ClearClick(object sender, RoutedEventArgs args)
    {
        var confirm = model.Confirm;
        model.Confirm = (title, body) => Task.FromResult(Dialogs.Confirm(this, model, title, body));
        try { await model.ClearRecentCommand.ExecuteAsync(null); }
        finally { model.Confirm = confirm; }
    }
    private async void CleanUnavailableClick(object sender, RoutedEventArgs args)
    {
        var confirm = model.Confirm;
        model.Confirm = (title, body) => Task.FromResult(Dialogs.Confirm(this, model, title, body));
        try { await model.CleanUnavailableRecentCommand.ExecuteAsync(null); }
        finally { model.Confirm = confirm; }
    }
    private void AssociateClick(object sender, RoutedEventArgs args)
    {
        if (!Dialogs.Confirm(this, model, model.Text["AssociateTitle"], model.Text["AssociateBody"])) return;
        try
        {
            using var extension = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.xpr");
            extension.SetValue("", "VivLauncher.Project");
            using var program = Registry.CurrentUser.CreateSubKey(@"Software\Classes\VivLauncher.Project");
            program.SetValue("", "Vivado project");
            using var command = program.CreateSubKey(@"shell\open\command");
            using var process = Process.GetCurrentProcess();
            var executable = process.MainModule?.FileName ?? throw new IOException("Cannot locate the launcher executable.");
            command.SetValue("", $"\"{executable}\" \"%1\"");
            using var icon = program.CreateSubKey("DefaultIcon");
            icon.SetValue("", $"\"{executable}\",0");
            SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero);
            model.Notice = model.Text["Saved"];
        }
        catch (Exception error) { model.Report(error); }
    }
    [DllImport("shell32")] private static extern void SHChangeNotify(uint eventId, uint flags, IntPtr first, IntPtr second);
}
