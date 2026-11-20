using CommunityToolkit.Mvvm.ComponentModel;

namespace VivLauncher.App;

public sealed class Localizer : ObservableObject
{
    private bool english;
    public bool English { get => english; set { if (SetProperty(ref english, value)) OnPropertyChanged("Item[]"); } }
    public string this[string key] => Words.TryGetValue(key, out var word) ? English ? word.En : word.Zh : key;
    private static readonly Dictionary<string, (string Zh, string En)> Words = new()
    {
        ["Subtitle"] = ("工程启动器", "Project launcher"),
        ["Manage"] = ("安装管理", "Installations"), ["Settings"] = ("设置", "Settings"),
        ["Search"] = ("模糊搜索工程，或粘贴 .xpr 路径…", "Fuzzy search projects or paste an .xpr path…"),
        ["SearchHint"] = ("fzf 模糊匹配 · 例如 pjt1、zu15 xpr · ↑↓ 选择，Enter 打开", "fzf fuzzy search · Try pjt1 or zu15 xpr · ↑↓ select, Enter opens"),
        ["Browse"] = ("浏览…", "Browse…"), ["Recent"] = ("最近工程", "Recent projects"),
        ["DropHint"] = ("＋ 也可以将 .xpr 文件拖到这里", "＋ Or drop an .xpr file here"),
        ["Current"] = ("当前工程", "Selected project"), ["Copy"] = ("复制完整路径", "Copy full path"),
        ["OpenWith"] = ("打开方式", "Open with"), ["Options"] = ("启动选项", "Launch options"),
        ["NoLog"] = ("不生成日志", "No log file"), ["NoJournal"] = ("不生成 journal", "No journal file"),
        ["Arguments"] = ("附加参数", "Additional arguments"), ["ViewLog"] = ("查看日志", "View log"),
        ["Open"] = ("打开工程", "Open project"), ["Empty"] = ("从第一个工程开始", "Start with your first project"),
        ["EmptyHint"] = ("浏览或拖入 .xpr 文件，下次就能在这里找到它。", "Browse or drop an .xpr file to find it here next time."),
        ["NoResults"] = ("没有找到匹配工程", "No matching projects"), ["NoResultsHint"] = ("换一个名称，或输入完整的 .xpr 路径。", "Try another name or enter a full .xpr path."),
        ["ChooseProject"] = ("选择一个工程，准备继续设计。", "Select a project to pick up where you left off."),
        ["LastUsed"] = ("上次使用", "Last used"), ["NeverUsed"] = ("尚未启动", "Not launched yet"),
        ["UnavailableBinding"] = ("绑定失效", "Binding unavailable"), ["UnavailableHistory"] = ("版本失效", "Version unavailable"),
        ["UnavailableProject"] = ("路径失效", "Path unavailable"), ["BoundVersion"] = ("绑定版本", "Bound version"),
        ["BindingIssue"] = ("绑定的 Vivado {0} 不可用，请选择可用的替代版本。", "The bound Vivado {0} is unavailable. Choose an available replacement."),
        ["HistoryIssue"] = ("上次使用的 Vivado {0} 不可用，可以选择其他可用版本。", "Vivado {0}, the last version used, is unavailable. Choose another available version."),
        ["Unavailable"] = ("失效 · 不可选择", "Unavailable · cannot select"), ["Available"] = ("可用", "Available"),
        ["Default"] = ("默认", "Default"), ["SetDefault"] = ("设为默认", "Set default"),
        ["Bind"] = ("记住此工程的版本", "Remember version for this project"), ["Unbind"] = ("已绑定 · 点击解除", "Bound · click to unbind"),
        ["BindingSource"] = ("已使用此工程绑定的版本", "Using this project's bound installation"),
        ["DefaultSource"] = ("来自全局默认设置", "From the global default"), ["LatestSource"] = ("已选择最新的可用安装", "Using the newest available installation"),
        ["ManualSource"] = ("本次使用手动选择的版本", "Using your selection for this launch"),
        ["MissingBinding"] = ("绑定的版本不可用，请选择替代版本。", "The bound installation is unavailable. Choose a replacement."),
        ["Mismatch"] = ("当前选择与工程绑定不同，打开前将确认。", "Selection differs from the binding. You will confirm before opening."),
        ["NoInstalls"] = ("未发现可用安装，请在安装管理中添加路径。", "No available installation. Add a path in Installations."),
        ["Ready"] = ("准备就绪", "Ready"), ["ReadyHint"] = ("双击工程或按 Enter 打开", "Double-click a project or press Enter"),
        ["Scanning"] = ("正在扫描安装…", "Scanning installations…"), ["Launching"] = ("正在启动…", "Starting Vivado…"),
        ["ProjectMissing"] = ("工程路径不可访问，请检查磁盘或共享目录。", "The project is inaccessible. Check the drive or network share."),
        ["VersionRequired"] = ("请选择可用的 Vivado 版本", "Choose an available Vivado installation"),
        ["ChooseVersion"] = ("选择可用版本…", "Choose an available version…"),
        ["Error"] = ("操作未完成", "Action could not be completed"), ["LaunchFailed"] = ("启动失败，请查看日志或选择其他版本。", "Launch failed. View the log or choose another installation."),
        ["SaveFailed"] = ("进程已创建，但最近工程未保存。请勿重复启动。", "Process created, but recent history could not be saved. Do not launch again."),
        ["LaunchCreated"] = ("Vivado 进程已创建", "Vivado process created"), ["Saved"] = ("已保存", "Saved"),
        ["Copied"] = ("已复制完整路径", "Full path copied"), ["Scan"] = ("重新扫描", "Scan again"),
        ["AddPath"] = ("添加路径", "Add path"), ["InstallRoot"] = ("选择 Vivado 安装目录", "Choose a Vivado installation directory"),
        ["ManageHint"] = ("工程绑定优先于默认版本。失效安装保留记录，不能设为默认。", "Project bindings take priority. Unavailable installations stay visible and cannot be defaults."),
        ["RemoveInstall"] = ("移除记录", "Remove record"), ["UndoRemoval"] = ("撤销上次移除", "Undo last removal"),
        ["RemoveTitle"] = ("移除失效的安装记录？", "Remove unavailable installation record?"),
        ["RemoveBody"] = ("Vivado {0}\n{1}\n\n{2} 个工程绑定到此安装，{3} 条最近记录使用过此版本。移除仅隐藏安装记录，工程文件、绑定和历史记录都会保留。", "Vivado {0}\n{1}\n\n{2} project bindings and {3} recent entries using this version. Removing the record hides the installation and keeps project files, bindings, and history."),
        ["Language"] = ("语言", "Language"), ["Appearance"] = ("外观", "Appearance"),
        ["System"] = ("跟随系统", "System"), ["Light"] = ("浅色", "Light"), ["Dark"] = ("深色", "Dark"),
        ["PreferencesHint"] = ("立即切换，保留当前输入。", "Changes apply instantly without losing your input."),
        ["Diagnostics"] = ("诊断与帮助", "Diagnostics & help"), ["Associate"] = ("关联 .xpr", "Associate .xpr"),
        ["Config"] = ("配置目录", "Configuration folder"), ["Clear"] = ("清空最近工程", "Clear recent projects"),
        ["Help"] = ("快捷键与使用说明", "Keyboard shortcuts & help"),
        ["HelpBody"] = ("Ctrl+F 搜索 · Ctrl+O 浏览\n↑↓ 选择 · Enter 打开 · Esc 关闭对话框\n\nfzf：空格分隔条件，! 排除，' 精确匹配，| 或，^ 前缀，$ 后缀。大写查询区分大小写。\n\n完整 .xpr 路径可直接粘贴，也可拖入文件。绑定失效时需明确选择替代版本。", "Ctrl+F search · Ctrl+O browse\n↑↓ select · Enter open · Esc close dialog\n\nfzf: spaces join terms, ! excludes, ' matches exactly, | means OR, ^ anchors the start, $ the end. Uppercase queries are case sensitive.\n\nPaste a full .xpr path or drop a file. Missing bindings require choosing a replacement."),
        ["ClearTitle"] = ("清空最近工程？", "Clear recent projects?"), ["ClearBody"] = ("只清除历史记录，不删除工程文件和版本绑定。", "Only history is cleared. Project files and bindings are kept."),
        ["MismatchTitle"] = ("使用不同版本打开？", "Open with a different version?"),
        ["MismatchBody"] = ("使用不同版本可能升级工程。原绑定将保留。", "A different version may upgrade the project. The original binding will be kept."),
        ["AssociateTitle"] = ("注册 .xpr 打开方式？", "Register the .xpr file handler?"),
        ["AssociateBody"] = ("为当前用户注册此启动器。Windows 可能仍需在默认应用中选择它。", "Register this launcher for the current user. Windows may still require choosing it in Default apps."),
        ["InvalidDrop"] = ("请一次拖入一个 .xpr 文件。", "Drop one .xpr file at a time."),
        ["Close"] = ("关闭", "Close"), ["Continue"] = ("继续", "Continue"), ["Cancel"] = ("取消", "Cancel"),
        ["LogEmpty"] = ("暂无启动日志。", "No launch log yet."), ["InstallMissing"] = ("原安装路径不可用，已保留记录。", "Original installation is unavailable; its record is retained."),
        ["Count"] = ("个工程", "projects"), ["Matches"] = ("个匹配 · 按相关度", "matches · by relevance"),
        ["InstallCount"] = ("个可用安装", "available installations")
    };
}
