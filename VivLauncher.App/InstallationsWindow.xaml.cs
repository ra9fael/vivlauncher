using System.Windows;

namespace VivLauncher.App;

public partial class InstallationsWindow : Window
{
    private readonly MainViewModel model;
    public InstallationsWindow(MainViewModel model) { this.model = model; DataContext = model; InitializeComponent(); }
    private async void RemoveClick(object sender, RoutedEventArgs args)
    {
        if (sender is FrameworkElement { DataContext: InstallRow item })
            await model.RemoveInstallAsync(item, (title, body) => Dialogs.Confirm(this, model, title, body, "RemoveInstall"));
    }
    private async void AddClick(object sender, RoutedEventArgs args)
    {
        try
        {
            var path = FolderPicker.Pick(this, model.Text["InstallRoot"]);
            if (path is not null) await model.AddPathAsync(path);
        }
        catch (Exception error) { model.Report(error); }
    }
}
