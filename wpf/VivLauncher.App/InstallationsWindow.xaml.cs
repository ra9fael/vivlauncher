using System.Windows;
using Microsoft.Win32;

namespace VivLauncher.App;

public partial class InstallationsWindow : Window
{
    private readonly MainViewModel model;
    public InstallationsWindow(MainViewModel model) { this.model = model; DataContext = model; InitializeComponent(); }
    private async void AddClick(object sender, RoutedEventArgs args)
    {
        var picker = new OpenFolderDialog { Title = model.Text["InstallRoot"], Multiselect = false };
        if (picker.ShowDialog(this) == true) await model.AddPathAsync(picker.FolderName);
    }
}
