using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VivLauncher.App;

public sealed class LanguageOption(string code, string nativeName, Localizer text) : ObservableObject
{
    public string Code { get; } = code;
    public string Name => Code == Localizer.SystemLanguage ? text["FollowSystemLanguage"] : nativeName;
    public string Description => Code == Localizer.SystemLanguage ? text.SystemLanguageDescription : "";

    internal void Refresh()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Description));
    }
}

public sealed class Localizer : ObservableObject
{
    public const string SystemLanguage = "system";
    private const string FallbackLanguage = "en";

    private static readonly Dictionary<string, string> Languages = new(StringComparer.OrdinalIgnoreCase)
    {
        ["zh-CN"] = "简体中文",
        [FallbackLanguage] = "English"
    };

    private static readonly Dictionary<string, Dictionary<string, string>> Catalogs =
        Languages.Keys.ToDictionary(code => code, LoadCatalog, StringComparer.OrdinalIgnoreCase);

    private readonly string? systemLocale = ResolveSystemLocale(CultureInfo.CurrentUICulture);

    public Localizer()
    {
        Options = Array.AsReadOnly(new[] { new LanguageOption(SystemLanguage, "", this) }
            .Concat(Languages.Select(language => new LanguageOption(language.Key, language.Value, this))).ToArray());
    }

    public IReadOnlyList<LanguageOption> Options { get; }
    public string Language { get; private set; } = SystemLanguage;
    public string DisplayLanguage => Language == SystemLanguage ? systemLocale ?? FallbackLanguage : Language;
    internal string SystemLanguageDescription => systemLocale is null
        ? Format("SystemLanguageFallback", Languages[FallbackLanguage])
        : Format("SystemLanguagePreview", Languages[systemLocale]);

    public string this[string key]
    {
        get
        {
            if (Catalogs[DisplayLanguage].TryGetValue(key, out var translation)) return translation;
            return Catalogs[FallbackLanguage].TryGetValue(key, out var fallback) ? fallback : key;
        }
    }

    public bool SetLanguage(string? code)
    {
        var option = Options.FirstOrDefault(option => string.Equals(option.Code, code, StringComparison.OrdinalIgnoreCase));
        if (option is null || option.Code == Language) return false;

        Language = option.Code;
        foreach (var choice in Options) choice.Refresh();
        OnPropertyChanged(nameof(Language));
        OnPropertyChanged(nameof(DisplayLanguage));
        OnPropertyChanged("Item[]");
        return true;
    }

    public string Format(string key, params object[] args) => string.Format(CultureInfo.GetCultureInfo(DisplayLanguage), this[key], args);

    private static string? ResolveSystemLocale(CultureInfo culture)
    {
        for (var candidate = culture; candidate.Name.Length > 0; candidate = candidate.Parent)
        {
            if (Catalogs.ContainsKey(candidate.Name)) return candidate.Name;
            if (candidate.Name == "zh-Hans") return "zh-CN";
        }
        return null;
    }

    private static Dictionary<string, string> LoadCatalog(string code)
    {
        var name = $"Localization.{code}.xml";
        using var stream = typeof(Localizer).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Missing language resource: {name}");
        using var reader = XmlReader.Create(stream);
        return XDocument.Load(reader).Root!.Elements("string")
            .ToDictionary(element => (string)element.Attribute("name")!, element => element.Value, StringComparer.Ordinal);
    }
}
