using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace VivLauncher.App;

public static class Dialogs
{
    public static bool Confirm(Window owner, MainViewModel model, string title, string body) => Create(owner, model, title, body, true).ShowDialog() == true;
    public static void Show(Window owner, MainViewModel model, string title, string body) => Create(owner, model, title, body, false).ShowDialog();
    public static void Log(Window owner, MainViewModel model)
    {
        try
        {
            var text = File.Exists(model.Launcher.LogPath) ? string.Join(Environment.NewLine, File.ReadLines(model.Launcher.LogPath).TakeLast(600)) : model.Text["LogEmpty"];
            Show(owner, model, model.Text["ViewLog"], model.Launcher.LogPath + "\n\n" + text);
        }
        catch (Exception error) { model.Report(error); }
    }
    private static Window Create(Window owner, MainViewModel model, string title, string body, bool confirm)
    {
        var window = new Window { Owner = owner, Title = title, Width = 570, Height = 390, MinWidth = 400, MinHeight = 250, WindowStartupLocation = WindowStartupLocation.CenterOwner, ShowInTaskbar = false, DataContext = model };
        var panel = new DockPanel { Margin = new Thickness(24) };
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 20, 0, 0) };
        DockPanel.SetDock(buttons, Dock.Bottom);
        panel.Children.Add(buttons);
        var cancel = new Button { IsCancel = true, Style = (Style)Application.Current.FindResource("Action") };
        cancel.SetBinding(ContentControl.ContentProperty, new Binding(confirm ? "Text[Cancel]" : "Text[Close]"));
        buttons.Children.Add(cancel);
        if (confirm)
        {
            var proceed = new Button { IsDefault = true, Style = (Style)Application.Current.FindResource("Primary"), Margin = new Thickness(10, 0, 0, 0) };
            proceed.SetBinding(ContentControl.ContentProperty, new Binding("Text[Continue]"));
            proceed.Click += (_, _) => window.DialogResult = true;
            buttons.Children.Add(proceed);
            window.Loaded += (_, _) => cancel.Focus();
        }
        panel.Children.Add(new TextBox { Text = body, IsReadOnly = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, BorderThickness = new Thickness(0), Background = System.Windows.Media.Brushes.Transparent });
        window.Content = panel;
        return window;
    }
}
