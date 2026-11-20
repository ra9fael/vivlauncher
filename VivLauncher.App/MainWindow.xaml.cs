using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using VivLauncher.Core;

namespace VivLauncher.App;

public partial class MainWindow : Window
{
    public MainViewModel Model { get; }
    private bool composing;
    private long compositionEnded;
    public MainWindow(MainViewModel model, string project)
    {
        Model = model;
        DataContext = model;
        InitializeComponent();
        Model.Confirm = (title, body) => Task.FromResult(Dialogs.Confirm(this, model, title, body));
        Model.CloseRequested += Close;
        TextCompositionManager.AddPreviewTextInputStartHandler(this, (_, _) => composing = true);
        TextCompositionManager.AddPreviewTextInputHandler(this, (_, _) => { if (composing) compositionEnded = MonotonicMilliseconds; composing = false; });
        Loaded += async (_, _) =>
        {
            SearchBox.Focus();
            try { await model.InitializeAsync(project); }
            catch (Exception error) { model.Report(error); }
        };
        Closing += (_, args) =>
        {
            if (model.Launching) return;
            try { model.Store.SaveOptions(new(model.NoLog, model.NoJournal, model.ExtraArgs)); }
            catch (Exception error)
            {
                model.Report(error);
                args.Cancel = !Dialogs.Confirm(this, model, model.Text["Error"], error.Message + "\n\n" + model.Text["Close"] + "?");
            }
        };
    }
    private void OnWindowKeyDown(object sender, KeyEventArgs args)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control) return;
        if (args.Key == Key.F) { SearchBox.Focus(); SearchBox.SelectAll(); args.Handled = true; }
        else if (args.Key == Key.O) { BrowseClick(sender, args); args.Handled = true; }
    }
    private async void BrowseClick(object sender, RoutedEventArgs args)
    {
        if (Model.Launching) return;
        var picker = new OpenFileDialog { Title = Model.Text["Browse"], Filter = "Vivado project (*.xpr)|*.xpr", CheckFileExists = true };
        if (picker.ShowDialog(this) == true) await Model.SelectPathAsync(picker.FileName);
    }
    private void OnSearchKeyDown(object sender, KeyEventArgs args)
    {
        if (composing || args.Key == Key.ImeProcessed) return;
        if (args.Key is Key.Up or Key.Down)
        {
            var index = ProjectList.SelectedIndex;
            ProjectList.SelectedIndex = Math.Max(0, Math.Min(index + (args.Key == Key.Down ? 1 : -1), Model.Projects.Count - 1));
            if (ProjectList.SelectedItem is not null) ProjectList.ScrollIntoView(ProjectList.SelectedItem);
            args.Handled = true;
        }
        else if (args.Key == Key.Enter && MonotonicMilliseconds - compositionEnded > 200)
        {
            Model.LaunchCommand.Execute(null);
            args.Handled = true;
        }
    }
    private static long MonotonicMilliseconds => (long)(System.Diagnostics.Stopwatch.GetTimestamp() * (1000.0 / System.Diagnostics.Stopwatch.Frequency));
    private void OnListKeyDown(object sender, KeyEventArgs args)
    {
        if (args.Key == Key.Enter && !composing) { Model.LaunchCommand.Execute(null); args.Handled = true; }
    }
    private void OnProjectDoubleClick(object sender, MouseButtonEventArgs args)
    {
        if (ItemsControl.ContainerFromElement(ProjectList, args.OriginalSource as DependencyObject) is ListBoxItem)
            Model.LaunchCommand.Execute(null);
    }
    private async void OnDrop(object sender, DragEventArgs args)
    {
        args.Handled = true;
        if (Model.Launching) return;
        if (args.Data.GetData(DataFormats.FileDrop) is string[] { Length: 1 } paths && Paths.IsProject(paths[0]))
            await Model.SelectPathAsync(paths[0]);
        else Model.Notice = Model.Text["InvalidDrop"];
    }
    private void OnDragOver(object sender, DragEventArgs args)
    {
        args.Effects = !Model.Launching && args.Data.GetData(DataFormats.FileDrop) is string[] { Length: 1 } paths && Paths.IsProject(paths[0]) ? DragDropEffects.Copy : DragDropEffects.None;
        args.Handled = true;
    }
    private void CopyClick(object sender, RoutedEventArgs args)
    {
        if (Model.SelectedProject is null) return;
        try { Clipboard.SetText(Model.SelectedProject.Path); Model.Notice = Model.Text["Copied"]; }
        catch (Exception error) { Model.Report(error); }
    }
    private void ManageClick(object sender, RoutedEventArgs args) => new InstallationsWindow(Model) { Owner = this }.ShowDialog();
    private void SettingsClick(object sender, RoutedEventArgs args) => new SettingsWindow(Model) { Owner = this }.ShowDialog();
    private void LogClick(object sender, RoutedEventArgs args) => Dialogs.Log(this, Model);
}
