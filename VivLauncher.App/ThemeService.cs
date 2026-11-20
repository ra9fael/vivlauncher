using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace VivLauncher.App;

public static class ThemeService
{
    private static int selected;
    public static void Initialize()
    {
#if NET48
        var theme = new ResourceDictionary { Source = new Uri("Themes/Compatible.xaml", UriKind.Relative) };
        Application.Current.Resources.MergedDictionaries.Add(theme);
        var button = (Style)theme[typeof(Button)];
#else
        Application.Current.ThemeMode = ThemeMode.System;
        var button = (Style)Application.Current.FindResource("DefaultButtonStyle");
#endif
        var resources = Application.Current.Resources;
        var action = new Style(typeof(Button), button);
        action.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(16, 9, 16, 9)));
        action.Setters.Add(new Setter(FrameworkElement.MinHeightProperty, 36.0));
        var primary = new Style(typeof(Button), action);
        primary.Setters.Add(new Setter(Control.BackgroundProperty, new DynamicResourceExtension("Accent")));
        primary.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
        primary.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.SemiBold));
        primary.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
        var link = new Style(typeof(Button), button);
        link.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
        link.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
        link.Setters.Add(new Setter(Control.ForegroundProperty, new DynamicResourceExtension("AccentInk")));
        link.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(0, 8, 0, 8)));
        link.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left));
        resources["Action"] = action;
        resources["Primary"] = primary;
        resources["LinkButton"] = link;
        resources["InstallOption"] = new Style(typeof(ComboBoxItem), Application.Current.TryFindResource(typeof(ComboBoxItem)) as Style);
        SystemEvents.UserPreferenceChanged += (_, _) => Application.Current.Dispatcher.InvokeAsync(() => Apply(selected));
    }
    public static void Apply(int theme)
    {
        selected = theme;
        var dark = theme == 2;
        if (theme == 0)
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            dark = key?.GetValue("AppsUseLightTheme") is int value && value == 0;
        }
#if !NET48
        Application.Current.ThemeMode = theme switch { 1 => ThemeMode.Light, 2 => ThemeMode.Dark, _ => ThemeMode.System };
#endif
        var colors = new Dictionary<string, string>
        {
            ["Surface"] = dark ? "#202020" : "#F7F7F7", ["Panel"] = dark ? "#2B2B2B" : "#FFFFFF",
            ["Ink"] = dark ? "#F3F3F3" : "#1F1F1F", ["MutedInk"] = dark ? "#BBBBBB" : "#5E6B7C",
            ["Line"] = dark ? "#454545" : "#DDE2E8", ["Accent"] = "#0067C0", ["AccentInk"] = dark ? "#78BEFF" : "#005FB8",
            ["Selected"] = dark ? "#213F58" : "#E6F0FB", ["WarningSurface"] = dark ? "#3B3020" : "#FFF4DC",
            ["WarningInk"] = dark ? "#FFD28B" : "#83520A", ["WarningLine"] = dark ? "#806137" : "#E7C78B",
            ["MatchSurface"] = dark ? "#554919" : "#FFE8A3", ["MatchInk"] = dark ? "#FFF1B9" : "#533900"
        };
        foreach (var pair in colors)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(pair.Value));
            brush.Freeze();
            Application.Current.Resources[pair.Key] = brush;
        }
    }
}
