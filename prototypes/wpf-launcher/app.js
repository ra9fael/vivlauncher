"use strict";

const translations = {
  "zh-CN": {
    previewTitle: "Vivado Launcher / 界面预览", language: "语言", theme: "外观", systemTheme: "跟随系统", lightTheme: "浅色", darkTheme: "深色",
    brandSubtitle: "工程启动器", tools: "工具", manage: "安装管理", settings: "设置", searchLabel: "搜索工程或输入完整 .xpr 路径",
    searchPlaceholder: "模糊搜索工程，或粘贴 .xpr 路径…", browse: "浏览…", searchHint: "fzf 模糊匹配 · 例如 pjt1、zu15 xpr · ↑↓ 选择，Enter 打开",
    recent: "最近工程", browseProject: "浏览工程", dropHint: "也可以将 .xpr 文件拖到这里", currentProject: "当前工程", copyPath: "复制完整路径",
    openWith: "打开方式", launchOptions: "启动选项", noLog: "不生成日志", noJournal: "不生成 journal", extraArgs: "附加参数",
    selectProject: "选择一个工程，准备继续设计。", viewLog: "查看日志", openProject: "打开工程", dropRelease: "松开以选择工程",
    prototype: "交互原型", simulationNote: "所有扫描与启动均为演示，不操作本机文件。", scenario: "演示场景", scenarioNormal: "日常使用",
    scenarioEmpty: "首次使用", scenarioNoInstalls: "没有安装", scenarioMissingBinding: "绑定版本缺失", scenarioMissingProject: "工程路径失效",
    scenarioScan: "扫描中", scenarioLaunchError: "启动失败", reset: "重置", localTools: "本机工具", close: "关闭",
    manageDescription: "设置默认版本，或添加其他位置的安装。工程绑定优先于默认版本。", rescan: "重新扫描", addPath: "添加路径",
    installRoot: "Vivado 安装目录", installPathHint: "原型仅保存演示路径，不验证本机安装。", add: "添加",
    languageHint: "立即切换，保留当前输入。", themeHint: "使用系统主题，或选择你习惯的外观。", diagnostics: "诊断与帮助",
    associate: "关联 .xpr", clearRecent: "清空最近工程", shortcuts: "快捷键与使用说明", shortcutSearch: "搜索最近工程", shortcutSelect: "选择工程",
    shortcutLaunch: "在搜索框或列表中打开所选工程", shortcutClose: "关闭对话框", previewHelp: "这是界面演示。工程、安装和绑定仅保留在当前页面；语言与外观偏好保存在浏览器中。",
    chooseProject: "选择工程", browseDescription: "演示文件选择：使用示例工程，或输入一个完整的 .xpr 路径。", projectPath: "工程路径",
    useSample: "使用示例", noFileRead: "不会读取本机文件内容。", select: "选择", cancel: "取消", launchLog: "启动日志",
    logDescription: "演示记录，不来自真实 Vivado 进程。", projectCount: "{count} 个工程", noResults: "没有找到匹配工程", noResultsHint: "换一个名称，或输入完整的 .xpr 路径。",
    firstProject: "从第一个工程开始", firstProjectHint: "浏览或拖入 .xpr 文件，下次就能在这里找到它。", openedToday: "今天打开", openedYesterday: "昨天打开",
    openedEarlier: "更早打开", lastVersion: "上次使用 {version}", neverOpened: "尚未启动 · 演示路径", missingProjectLabel: "路径不可访问", installedDefault: "默认",
    setDefault: "设为默认", unknownVersion: "自定义安装", noInstalls: "未发现 Vivado 安装", noInstallsHint: "打开安装管理，添加一个安装路径。",
    chooseVersion: "选择替代版本…", bindingSource: "已使用此工程绑定的版本", defaultSource: "来自全局默认设置", latestSource: "已选择最新的可用安装",
    manualSource: "本次使用手动选择的版本", missingBinding: "绑定的 {version} 不可用，请选择替代版本。", mismatchSource: "此工程绑定了 {version}，当前选择不同。",
    bind: "记住此工程的版本", bound: "已绑定此版本 · 点击解除", bindSaved: "已记住此工程的 Vivado {version}", bindRemoved: "已解除此工程的版本绑定",
    defaultsSaved: "Vivado {version} 已设为默认", optionsCount: "{count} 项已启用", ready: "准备就绪", readyHint: "双击工程或按 Enter 打开",
    scanStatus: "正在扫描安装…", scanHint: "后台扫描中，你仍可搜索和选择工程。", scanDone: "扫描完成，发现 {count} 个安装（演示）",
    launching: "正在启动…", launchHint: "正在模拟创建 Vivado 进程，请稍候。", launched: "已模拟启动 Vivado {version}", launchedHint: "正式版此时关闭启动器；原型保留以便继续体验。",
    launchFailed: "启动失败：无法创建 Vivado 进程", launchFailedHint: "演示错误 2：找不到指定文件。可查看日志或更换版本。",
    projectUnavailable: "工程路径不可访问", projectUnavailableHint: "检查磁盘或共享目录，或浏览其他工程。", versionRequired: "请选择可用的 Vivado 版本",
    invalidProject: "请输入完整的 Windows .xpr 路径，例如 D:\\FPGA\\project.xpr。", invalidInstall: "请输入完整的 Windows 安装目录。",
    duplicateInstall: "该安装路径已经存在。", installAdded: "已添加演示安装：{version}", copied: "已复制工程完整路径", copyFallback: "浏览器未允许复制；可在详情区选择路径手动复制。",
    confirmMismatch: "使用不同版本打开？", confirmMismatchBody: "此工程绑定了 Vivado {bound}，本次选择 Vivado {selected}。正式版中使用不同版本可能触发工程升级。",
    continueLaunch: "继续打开", confirmClear: "清空最近工程？", confirmClearBody: "只清除本页的演示历史，不删除任何工程文件。", clear: "清空列表",
    cleared: "已清空演示历史", associateDemo: "已演示关联操作；未修改 Windows 注册表。", invalidDrop: "请一次拖入一个 .xpr 文件。",
    dropDemo: "浏览器无法提供真实完整路径，请确认演示路径后选择。", logEmpty: "暂无启动记录。\n在主界面点击“打开工程”体验模拟启动。",
    logSimulation: "[演示记录] 未创建真实进程", logSuccess: "结果：已模拟创建进程", logFailure: "结果：创建进程失败（模拟）\nWindows 错误：2 / 找不到指定文件",
    logProject: "工程", logInstall: "安装", logArgs: "参数", logDirectory: "工作目录", noMatchStatus: "选择或浏览一个工程", scanManaged: "正在扫描示例安装…",
    browsingPath: "使用输入的工程路径", available: "可用", countInstalls: "{count} 个安装", logNoneArgs: "无",
    rankedCount: "{count} 个匹配 · 按相关度", unavailableVersion: "版本失效", unavailableBinding: "绑定失效", unavailableHistory: "上次版本不可用",
    unavailableInstall: "安装失效", unavailableInstallHint: "原安装路径不可用，已保留记录；请选择其他可用安装。",
    unavailableVersionOption: "失效 · 不可选择", availableCount: "{count} 个可用安装", bindingLabel: "绑定版本 {version}", historyUnavailableDescription: "上次使用的版本 {version} 已不可用",
    bindingUnavailableDescription: "绑定版本 {version} 已失效",
    removeRecord: "移除记录", removeRecordTitle: "移除 Vivado {version} 的记录？", removeProjectTitle: "移除这条失效工程记录？",
    removeRecordBody: "{path}\n\n只从安装管理和版本列表移除，不删除磁盘文件。\n\n{bindings} 个工程绑定此安装，{history} 个工程曾使用此版本。工程记录和绑定保留，失效提示仍会显示。{defaultNote}\n\n可以撤销移除；同一路径恢复可用后也可重新发现。",
    removeDefaultNote: "\n此失效安装还是默认版本，移除后会清除该默认设置。",
    recordRemoved: "已移除 Vivado {version} 的失效记录。工程记录与绑定保留。",
    removedRecords: "已移除 {count} 条失效记录。", undoRemove: "撤销上次移除", recordRestored: "已恢复 Vivado {version} 的记录，仍标记为失效。",
    installRecovered: "已模拟恢复同一路径的 Vivado {version}；原绑定重新可用。",
    removeHint: "失效安装可移除记录；不会删除文件，也不会自动改绑工程。",
    projectPathHelp: "检查磁盘或共享目录是否已连接，或浏览其他工程。此工程仍保留在最近列表中。",
    pathUnavailableLabel: "路径失效", versionUnavailableLabel: "版本失效",
    bindingUnavailableHelp: "绑定的 Vivado {version} 不可用，请选择可用的替代版本。",
    versionUnavailableHelp: "上次使用的 Vivado {version} 不可用，本次可选择其他可用版本。",
    cleanUnavailable: "清理失效路径（{count}）", cleanUnavailableSettings: "清理失效路径", cleanUnavailableTitle: "清理失效的工程路径？",
    cleanUnavailableBody: "将移除 {count} 条已不存在的最近工程记录：\n{list}{more}\n\n不会删除任何工程文件，绑定和安装记录保持不变。",
    cleanUnavailableMore: "\n…另有 {count} 条", cleanNow: "清理", cleanUnavailableDone: "已清理 {count} 条失效路径",
    cleanNone: "没有可清理的失效路径", undoClean: "撤销清理", cleanRestored: "已恢复上次清理的工程记录",
    scenarioStalePaths: "清理失效路径", cleanHint: "清理只针对本页内存中的失效工程，不读写本机文件。"
  },
  en: {
    previewTitle: "Vivado Launcher / Design preview", language: "Language", theme: "Appearance", systemTheme: "System", lightTheme: "Light", darkTheme: "Dark",
    brandSubtitle: "Project launcher", tools: "Tools", manage: "Installations", settings: "Settings", searchLabel: "Search projects or enter a full .xpr path",
    searchPlaceholder: "Fuzzy search projects or paste an .xpr path…", browse: "Browse…", searchHint: "fzf fuzzy search · Try pjt1 or zu15 xpr · ↑↓ select, Enter opens",
    recent: "Recent projects", browseProject: "Browse projects", dropHint: "Or drop an .xpr file here", currentProject: "Selected project", copyPath: "Copy full path",
    openWith: "Open with", launchOptions: "Launch options", noLog: "No log file", noJournal: "No journal file", extraArgs: "Additional arguments",
    selectProject: "Select a project to pick up where you left off.", viewLog: "View log", openProject: "Open project", dropRelease: "Drop to select this project",
    prototype: "Prototype", simulationNote: "Scanning and launching are simulated. No local files are changed.", scenario: "Scenario", scenarioNormal: "Everyday use",
    scenarioEmpty: "First launch", scenarioNoInstalls: "No installations", scenarioMissingBinding: "Missing bound version", scenarioMissingProject: "Unavailable project",
    scenarioScan: "Scanning", scenarioLaunchError: "Launch failure", reset: "Reset", localTools: "Local tools", close: "Close",
    manageDescription: "Choose a default or add an installation elsewhere. Project bindings take priority over the default.", rescan: "Scan again", addPath: "Add path",
    installRoot: "Vivado installation directory", installPathHint: "Only a demo path is stored. No local installation is inspected.", add: "Add",
    languageHint: "Switch instantly without losing your input.", themeHint: "Follow your system or choose an appearance.", diagnostics: "Diagnostics & help",
    associate: "Associate .xpr", clearRecent: "Clear recent", shortcuts: "Keyboard shortcuts & help", shortcutSearch: "Search recent projects", shortcutSelect: "Select a project",
    shortcutLaunch: "Open the selected project from search or the list", shortcutClose: "Close the dialog", previewHelp: "This is a UI preview. Projects, installations and bindings last for this page only. Language and appearance are saved in your browser.",
    chooseProject: "Choose a project", browseDescription: "File picker preview: use a sample project or enter a full .xpr path.", projectPath: "Project path",
    useSample: "Use sample", noFileRead: "No local file contents are read.", select: "Select", cancel: "Cancel", launchLog: "Launch log",
    logDescription: "A simulated record, not output from a real Vivado process.", projectCount: "{count} projects", noResults: "No matching projects", noResultsHint: "Try another name or enter a full .xpr path.",
    firstProject: "Start with your first project", firstProjectHint: "Browse or drop an .xpr file to find it here next time.", openedToday: "Opened today", openedYesterday: "Opened yesterday",
    openedEarlier: "Opened earlier", lastVersion: "Last used {version}", neverOpened: "Not launched yet · Demo path", missingProjectLabel: "Unavailable path", installedDefault: "Default",
    setDefault: "Set default", unknownVersion: "Custom installation", noInstalls: "No Vivado installations found", noInstallsHint: "Open Installations to add an installation path.",
    chooseVersion: "Select a replacement…", bindingSource: "Using this project's bound version", defaultSource: "Using the global default", latestSource: "Using the newest available installation",
    manualSource: "Manually selected for this launch", missingBinding: "Bound version {version} is unavailable. Select a replacement.", mismatchSource: "This project is bound to {version}, a different version.",
    bind: "Remember this version for the project", bound: "Version bound · Click to unbind", bindSaved: "Project bound to Vivado {version}", bindRemoved: "Project version binding removed",
    defaultsSaved: "Vivado {version} is now the default", optionsCount: "{count} enabled", ready: "Ready to launch", readyHint: "Double-click a project or press Enter to open",
    scanStatus: "Scanning installations…", scanHint: "Scanning in the background. You can still search and select projects.", scanDone: "Scan complete: {count} installations found (demo)",
    launching: "Launching…", launchHint: "Simulating Vivado process creation. One moment.", launched: "Simulated launch of Vivado {version}", launchedHint: "The desktop app would close now. This preview stays open for review.",
    launchFailed: "Launch failed: could not create the Vivado process", launchFailedHint: "Demo error 2: file not found. View the log or choose another version.",
    projectUnavailable: "Project path is unavailable", projectUnavailableHint: "Check the drive or shared folder, or browse for another project.", versionRequired: "Choose an available Vivado version",
    invalidProject: "Enter a full Windows .xpr path, such as D:\\FPGA\\project.xpr.", invalidInstall: "Enter a full Windows installation directory.",
    duplicateInstall: "This installation path already exists.", installAdded: "Demo installation added: {version}", copied: "Full project path copied", copyFallback: "Clipboard access was unavailable. Select and copy the path in the details pane.",
    confirmMismatch: "Open with a different version?", confirmMismatchBody: "This project is bound to Vivado {bound}. You selected Vivado {selected}. In the desktop app, this could trigger a project upgrade.",
    continueLaunch: "Open anyway", confirmClear: "Clear recent projects?", confirmClearBody: "This only clears the demo history on this page. No project files are deleted.", clear: "Clear list",
    cleared: "Demo history cleared", associateDemo: "Association demonstrated. The Windows registry was not changed.", invalidDrop: "Drop one .xpr file at a time.",
    dropDemo: "Browsers cannot expose the real full path. Confirm the demo path before selecting.", logEmpty: "No launch records yet.\nSelect Open project to try a simulated launch.",
    logSimulation: "[DEMO RECORD] No real process created", logSuccess: "Result: process creation simulated", logFailure: "Result: process creation failed (simulated)\nWindows error: 2 / File not found",
    logProject: "Project", logInstall: "Installation", logArgs: "Arguments", logDirectory: "Working directory", noMatchStatus: "Select or browse for a project", scanManaged: "Scanning sample installations…",
    browsingPath: "Using the entered project path", available: "Available", countInstalls: "{count} installations", logNoneArgs: "None",
    rankedCount: "{count} matches · Ranked", unavailableVersion: "Version unavailable", unavailableBinding: "Binding unavailable", unavailableHistory: "Last version unavailable",
    unavailableInstall: "Unavailable", unavailableInstallHint: "The original installation path is unavailable. Its record is kept; choose an available installation.",
    unavailableVersionOption: "Unavailable · Cannot select", availableCount: "{count} available installations", bindingLabel: "Bound version {version}", historyUnavailableDescription: "Last used version {version} is unavailable",
    bindingUnavailableDescription: "Bound version {version} is unavailable",
    removeRecord: "Remove record", removeRecordTitle: "Remove the Vivado {version} record?", removeProjectTitle: "Remove this unavailable project record?",
    removeRecordBody: "{path}\n\nRemove it from installation management and the version picker. No files are deleted.\n\n{bindings} projects are bound to this installation; {history} projects last used this version. Project records and bindings stay intact, including unavailable warnings.{defaultNote}\n\nYou can undo removal. The same path can be rediscovered once it becomes available.",
    removeDefaultNote: "\nThis is also the default installation; removing it clears that default.",
    recordRemoved: "Removed the unavailable Vivado {version} record. Project records and bindings are kept.",
    removedRecords: "{count} unavailable records removed.", undoRemove: "Undo last removal", recordRestored: "Restored the Vivado {version} record, still marked unavailable.",
    installRecovered: "Simulated recovery of Vivado {version} at the same path. Existing bindings are available again.",
    removeHint: "Remove unavailable installation records without deleting files or changing project bindings.",
    projectPathHelp: "Check the drive or network share, or browse for another project. This project stays in the recent list.",
    pathUnavailableLabel: "Path unavailable", versionUnavailableLabel: "Version unavailable",
    bindingUnavailableHelp: "Bound Vivado {version} is unavailable. Choose an available replacement.",
    versionUnavailableHelp: "Last used Vivado {version} is unavailable. You can choose another available version.",
    cleanUnavailable: "Clean unavailable paths ({count})", cleanUnavailableSettings: "Clean unavailable paths", cleanUnavailableTitle: "Remove unavailable project paths?",
    cleanUnavailableBody: "Removes {count} recent project records whose files are missing:\n{list}{more}\n\nNo project files are deleted; bindings and installations stay unchanged.",
    cleanUnavailableMore: "\n…and {count} more", cleanNow: "Clean", cleanUnavailableDone: "Removed {count} unavailable paths",
    cleanNone: "No unavailable paths to clean", undoClean: "Undo cleanup", cleanRestored: "Restored the last cleaned project records",
    scenarioStalePaths: "Clean up stale paths", cleanHint: "Cleanup only affects missing projects in this page's memory. No local files are read or changed."
  }
};

const element = id => document.getElementById(id);
const preferenceKey = "vivlauncher-prototype-preferences";
const mediaTheme = window.matchMedia("(prefers-color-scheme: dark)");
const initialPreferences = (() => {
  try { return JSON.parse(localStorage.getItem(preferenceKey)) || {}; }
  catch { return {}; }
})();
const preferences = {
  language: translations[initialPreferences.language] ? initialPreferences.language : (navigator.language.startsWith("zh") ? "zh-CN" : "en"),
  theme: ["light", "dark", "system"].includes(initialPreferences.theme) ? initialPreferences.theme : "system"
};
const baseInstalls = [
  { id: "amd-2025", version: "2025.2", path: "C:\\AMDDesignTools\\2025.2\\Vivado" },
  { id: "xilinx-2024", version: "2024.2", path: "C:\\Xilinx\\Vivado\\2024.2" }
];
const baseProjects = [
  { id: "zu24", name: "project_1.xpr", path: "D:\\vm\\vmshared\\linux\\impcas\\mw\\fpga\\gui\\impcas_mw_zux_viv24\\project\\hifi\\project_1.xpr", lastVersion: "2024.2", opened: "openedToday", bound: "xilinx-2024", available: true },
  { id: "zu15", name: "project_1.xpr", path: "D:\\vm\\vmshared\\linux\\impcas\\mw\\fpga\\gui\\impcas_mw_zu15_viv24\\project_1.xpr", lastVersion: "2024.2", opened: "openedToday", bound: null, available: true },
  { id: "zu9", name: "impcas_hw_fpga_zu9_viv24.xpr", path: "D:\\vm\\vmshared\\linux\\impcas_hw_fpga_zu9_viv24\\impcas_hw_fpga_zu9_viv24.xpr", lastVersion: "2024.2", opened: "openedYesterday", bound: "xilinx-2024", available: true },
  { id: "blm", name: "project.xpr", path: "D:\\vm\\vmshared\\linux\\impcas\\blm\\impcas_blm_zu9_viv21_260910\\project\\project.xpr", lastVersion: "2025.2", opened: "openedYesterday", bound: null, available: true },
  { id: "pynq", name: "pynq_base_zu15.xpr", path: "D:\\vm\\vmshared\\linux\\pynq\\pynq_base_zux\\build\\pynq_base_zu15\\pynq_base_zu15.xpr", lastVersion: "2024.2", opened: "openedEarlier", bound: null, available: true },
  { id: "blm-old", name: "project.xpr", path: "D:\\vm\\vmshared\\linux\\impcas\\blm\\impcas_blm_zu9_viv21\\project\\project.xpr", lastVersion: "2024.2", opened: "openedEarlier", bound: null, available: true }
];
const state = {
  projects: [], installs: [], selectedProject: null, selectedInstall: null, defaultInstall: "xilinx-2024", source: "bindingSource",
  query: "", scenario: "normal", scanning: false, launching: false, status: null, log: null, scanTimer: null, launchTimer: null,
  noLog: true, noJournal: true, extraArgs: "", confirmAction: null, searchTimer: null, removedInstalls: [], cleanedBatches: []
};

function translate(key, values = {}) {
  const message = translations[preferences.language][key] || translations.en[key] || key;
  return message.replace(/\{(\w+)\}/g, (match, name) => values[name] ?? match);
}

function text(id, key, values) { element(id).textContent = translate(key, values); }
function make(tag, className, content) {
  const node = document.createElement(tag);
  if (className) node.className = className;
  if (content !== undefined) node.textContent = content;
  return node;
}
function icon(name) {
  const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
  svg.setAttribute("class", "icon");
  svg.setAttribute("aria-hidden", "true");
  const use = document.createElementNS("http://www.w3.org/2000/svg", "use");
  use.setAttribute("href", `#icon-${name}`);
  svg.append(use);
  return svg;
}
function normalizePath(path) { return path.trim().replace(/^"|"$/g, "").replaceAll("/", "\\"); }
function pathKey(path) { return normalizePath(path).replace(/\\+$/, "").toLowerCase(); }
function isAbsolutePath(path) { return /^(?:[a-z]:\\[^<>:"|?*\r\n]+|\\\\[^\\<>:"|?*\r\n]+\\[^<>:"|?*\r\n]+)$/i.test(path); }
function isProjectPath(path) { return isAbsolutePath(path) && /\.xpr$/i.test(path); }
function parentPath(path) { return path.slice(0, path.lastIndexOf("\\")); }
function availableInstalls() { return state.installs.filter(install => install.available !== false); }
function currentInstall() { return availableInstalls().find(install => install.id === state.selectedInstall); }
function boundVersion(project) {
  return state.installs.find(install => install.id === project?.bound)?.version || project?.boundVersion || baseInstalls.find(install => install.id === project?.bound)?.version || "—";
}
function rankedProjects() {
  const query = state.query.trim();
  if (isProjectPath(normalizePath(query))) return state.selectedProject ? [{ item: state.selectedProject, positions: new Set() }] : [];
  if (!query) return state.projects.map(project => ({ item: project, positions: new Set() }));
  return new fzf.Fzf(state.projects, {
    selector: project => project.path,
    fuzzy: "v2",
    match: fzf.extendedMatch,
    casing: "smart-case",
    tiebreakers: [],
    sort: true
  }).find(query);
}
function filteredProjects() { return rankedProjects().map(result => result.item); }
function projectVersionState(project) {
  if (project.bound) {
    return { version: boundVersion(project), unavailable: !availableInstalls().some(install => install.id === project.bound), label: "bindingLabel", warning: "unavailableBinding", description: "bindingUnavailableDescription" };
  }
  return { version: project.lastVersion || "—", unavailable: Boolean(project.lastVersion && !availableInstalls().some(install => install.version === project.lastVersion)), label: "lastVersion", warning: "unavailableHistory", description: "historyUnavailableDescription" };
}
function knownInstalls() {
  const installs = [...state.installs];
  const projects = state.selectedProject ? [...state.projects, state.selectedProject] : state.projects;
  for (const project of projects) {
    if (!project.bound || installs.some(install => install.id === project.bound)) continue;
    const original = baseInstalls.find(install => install.id === project.bound);
    installs.push({ id: project.bound, version: boundVersion(project), path: project.boundPath || original?.path || "—", available: false });
  }
  return installs.filter(install => !state.removedInstalls.some(removed => removed.id === install.id));
}
function highlightedText(className, value, positions, offset = 0) {
  const node = make("span", className);
  const characters = Array.from(value.normalize());
  let segment = "";
  let matched = false;
  const appendSegment = () => {
    if (segment) node.append(matched ? make("mark", "search-match", segment) : document.createTextNode(segment));
  };
  characters.forEach((character, index) => {
    const nextMatch = positions.has(index + offset);
    if (nextMatch !== matched) { appendSegment(); segment = ""; matched = nextMatch; }
    segment += character;
  });
  appendSegment();
  return node;
}

function applyPreferences() {
  document.documentElement.lang = preferences.language;
  document.documentElement.dataset.theme = preferences.theme === "system" ? (mediaTheme.matches ? "dark" : "light") : preferences.theme;
  document.title = `Vivado Launcher · ${translate("prototype")}`;
  document.querySelectorAll("[data-i18n]").forEach(node => { node.textContent = translate(node.dataset.i18n); });
  document.querySelectorAll("[data-i18n-aria]").forEach(node => { node.setAttribute("aria-label", translate(node.dataset.i18nAria)); });
  document.querySelectorAll("[data-i18n-placeholder]").forEach(node => { node.placeholder = translate(node.dataset.i18nPlaceholder); });
  for (const id of ["language", "settings-language"]) element(id).value = preferences.language;
  for (const id of ["theme", "settings-theme"]) element(id).value = preferences.theme;
  document.querySelectorAll("[data-message]").forEach(node => { node.textContent = translate(node.dataset.message, JSON.parse(node.dataset.values || "{}")); });
  render();
  renderLog();
  try { localStorage.setItem(preferenceKey, JSON.stringify(preferences)); }
  catch { }
}

function messageAt(id, key, values = {}) {
  const node = element(id);
  node.dataset.message = key;
  node.dataset.values = JSON.stringify(values);
  node.textContent = translate(key, values);
}
function clearMessage(id) {
  const node = element(id);
  delete node.dataset.message;
  delete node.dataset.values;
  node.textContent = "";
}

function pickInstall() {
  const project = state.selectedProject;
  const installs = availableInstalls();
  if (project?.bound) {
    state.selectedInstall = installs.some(install => install.id === project.bound) ? project.bound : null;
    state.source = state.selectedInstall ? "bindingSource" : "missingBinding";
  } else {
    state.selectedInstall = installs.find(install => install.id === state.defaultInstall)?.id || installs[0]?.id || null;
    state.source = state.selectedInstall === state.defaultInstall ? "defaultSource" : "latestSource";
  }
}
function selectProject(project) {
  if (state.launching || state.selectedProject === project) return;
  state.selectedProject = project;
  state.status = null;
  pickInstall();
  render();
}

function renderProjects() {
  const list = element("project-list");
  const unifiedWarnings = Boolean(element("project-issues"));
  const results = rankedProjects();
  const projects = results.map(result => result.item);
  const signature = JSON.stringify([preferences.language, state.query, projects.map(project => [project.id, project.path, project.lastVersion, project.available, project.bound, projectVersionState(project)])]);
  const rebuild = list.dataset.signature !== signature;
  if (rebuild) {
    list.replaceChildren();
    list.dataset.signature = signature;
  }
  list.hidden = projects.length === 0;
  element("empty-state").hidden = projects.length !== 0;
  const searching = Boolean(state.query.trim() && !isProjectPath(normalizePath(state.query)));
  text("project-count", searching ? "rankedCount" : "projectCount", { count: projects.length });
  text("empty-title", state.query ? "noResults" : "firstProject");
  text("empty-description", state.query ? "noResultsHint" : "firstProjectHint");
  list.removeAttribute("aria-activedescendant");
  projects.forEach((project, index) => {
    if (!rebuild) {
      const existing = list.children[index];
      existing.setAttribute("aria-selected", String(state.selectedProject?.id === project.id));
      if (state.selectedProject?.id === project.id) list.setAttribute("aria-activedescendant", existing.id);
      return;
    }
    const row = make("div", "project-row");
    row.id = `project-option-${index}`;
    row.dataset.projectId = project.id;
    const versionState = projectVersionState(project);
    row.classList.toggle("has-unavailable-version", versionState.unavailable);
    row.classList.toggle("has-unavailable-project", !project.available);
    const hasWarning = versionState.unavailable || !project.available;
    row.classList.toggle("has-warning", unifiedWarnings && hasWarning);
    row.setAttribute("role", "option");
    row.setAttribute("aria-selected", String(state.selectedProject?.id === project.id));
    row.setAttribute("aria-label", `${project.name}, ${parentPath(project.path)}, ${translate(versionState.unavailable ? versionState.description : versionState.label, { version: versionState.version })}${project.available ? "" : `, ${translate("missingProjectLabel")}`}`);
    const fileIcon = make("span", "project-file-icon");
    fileIcon.append(icon(unifiedWarnings || project.available ? "file" : "warning"));
    const description = make("span", "project-row-description");
    const parent = parentPath(project.path);
    description.append(highlightedText("project-row-title", project.name, results[index].positions, Array.from(parent.normalize()).length + 1), highlightedText("project-row-path", parent, results[index].positions));
    const version = make("span", "project-row-version");
    const chip = make("span", `version-chip${hasWarning ? (unifiedWarnings ? " warning-state" : " unavailable-version") : ""}`);
    const versionNumber = make("span", "version-number");
    if (hasWarning) versionNumber.append(icon("warning"));
    versionNumber.append(document.createTextNode(versionState.version));
    chip.append(versionNumber);
    const appendLabel = (modifier, key, hint) => {
      const label = make("span", `version-warning-label row-warning-label${modifier ? " " + modifier : ""}`, translate(key));
      if (hint) label.title = hint;
      chip.append(label);
    };
    if (!project.available) appendLabel("project-path-warning", "pathUnavailableLabel");
    if (versionState.unavailable) {
      if (project.bound) appendLabel("binding-warning", "unavailableBinding");
      else appendLabel("history-warning", unifiedWarnings ? "versionUnavailableLabel" : "unavailableHistory", translate("historyUnavailableDescription", { version: versionState.version }));
    }
    version.append(chip);
    if (!hasWarning && project.bound) version.append(icon("link"));
    if (!project.available) version.append(makeRemoveButton(project));
    row.append(fileIcon, description, version);
    list.append(row);
    if (state.selectedProject?.id === project.id) list.setAttribute("aria-activedescendant", row.id);
    row.addEventListener("click", () => {
      const current = filteredProjects().find(candidate => candidate.id === row.dataset.projectId);
      if (current) selectProject(current);
      list.focus({ preventScroll: true });
    });
    row.addEventListener("dblclick", requestLaunch);
  });
  list.setAttribute("aria-busy", String(state.launching));
}

function renderDetails() {
  const project = state.selectedProject;
  const issues = element("project-issues");
  if (issues) { issues.replaceChildren(); issues.hidden = true; }
  element("project-details").classList.toggle("has-unavailable-project", Boolean(project && !project.available));
  element("project-details").hidden = !project;
  element("detail-empty").hidden = Boolean(project);
  if (!project) return;
  element("project-name").textContent = project.name;
  element("project-path").textContent = project.path;
  element("last-opened").textContent = project.available || issues ? (project.lastVersion ? `${translate(project.opened)} · ${translate("lastVersion", { version: project.lastVersion })}` : translate("neverOpened")) : translate("missingProjectLabel");
  const version = element("version");
  version.replaceChildren();
  if (!state.selectedInstall) {
    const option = make("option", "", translate(availableInstalls().length ? "chooseVersion" : "noInstalls"));
    option.value = "";
    option.disabled = true;
    version.append(option);
  }
  knownInstalls().forEach(install => {
    const unavailable = install.available === false;
    const option = make("option", unavailable ? "unavailable-option" : "", `${unavailable ? "⚠ " : ""}Vivado ${install.version}${unavailable ? ` (${translate("unavailableVersionOption")})` : install.id === state.defaultInstall ? ` (${translate("installedDefault")})` : ""}`);
    option.value = install.id;
    option.disabled = unavailable;
    version.append(option);
  });
  version.value = state.selectedInstall || "";
  version.disabled = !availableInstalls().length || state.launching;
  element("version-path").textContent = currentInstall()?.path || "";
  let source = state.source;
  if (!availableInstalls().length) source = "noInstallsHint";
  else if (project.bound && !state.selectedInstall) source = "missingBinding";
  else if (project.bound && project.bound !== state.selectedInstall) source = "mismatchSource";
  text("selection-source", source, { version: boundVersion(project) });
  element("selection-source").classList.toggle("warning", ["missingBinding", "mismatchSource", "noInstallsHint"].includes(source));
  if (issues) {
    const versionState = projectVersionState(project);
    const appendIssue = (label, description) => {
      const issue = make("div", "project-issue");
      const content = make("div", "");
      content.append(make("strong", "", translate(label)), make("p", "", translate(description, { version: versionState.version })));
      issue.append(icon("warning"), content);
      issues.append(issue);
    };
    if (!project.available) appendIssue("pathUnavailableLabel", "projectPathHelp");
    if (versionState.unavailable) appendIssue(project.bound ? "unavailableBinding" : "versionUnavailableLabel", project.bound ? "bindingUnavailableHelp" : "versionUnavailableHelp");
    issues.hidden = issues.childElementCount === 0;
    element("selection-source").hidden = versionState.unavailable;
  }
  const isBound = Boolean(state.selectedInstall && project.bound === state.selectedInstall);
  text("binding-text", isBound ? "bound" : "bind");
  element("bind-button").setAttribute("aria-pressed", String(isBound));
  element("bind-button").disabled = !currentInstall() || state.launching || !project.available;
  const count = Number(state.noLog) + Number(state.noJournal) + Number(Boolean(state.extraArgs.trim()));
  text("options-summary", "optionsCount", { count });
  for (const id of ["no-log", "no-journal", "extra-args"]) element(id).disabled = state.launching;
}

function renderStatus() {
  let key = "ready";
  let hint = "readyHint";
  let tone = "";
  let values = {};
  if (!state.selectedProject) { key = "noMatchStatus"; hint = "dropHint"; tone = "warning"; }
  else if (!state.selectedProject.available) { key = element("project-issues") ? "pathUnavailableLabel" : "projectUnavailable"; hint = "projectUnavailableHint"; tone = "warning"; }
  else if (!availableInstalls().length) { key = "noInstalls"; hint = "noInstallsHint"; tone = "warning"; }
  else if (!currentInstall()) { key = "versionRequired"; hint = "readyHint"; tone = "warning"; }
  if (state.status) { ({ key, hint = "readyHint", tone = "", values = {} } = state.status); }
  if (state.scanning) { key = "scanStatus"; hint = "scanHint"; tone = "busy"; }
  if (state.launching) { key = "launching"; hint = "launchHint"; tone = "busy"; }
  text("status", key, values);
  text("footer-hint", hint);
  element("status-dot").className = `status-dot ${tone}`;
  element("view-error").hidden = key !== "launchFailed";
  element("launch-button").disabled = state.launching || state.scanning || !state.selectedProject?.available || !currentInstall();
  text("launch-label", state.launching ? "launching" : "openProject");
  element("search").disabled = state.launching;
  element("browse-button").disabled = state.launching;
  element("manage-button").disabled = state.launching;
}

function renderInstalls() {
  const list = element("install-list");
  const focusedInstall = document.activeElement?.dataset.defaultId;
  const focusedRemoval = document.activeElement?.dataset.removeId;
  list.replaceChildren();
  element("install-count").textContent = availableInstalls().length;
  element("install-count").setAttribute("aria-label", translate("availableCount", { count: availableInstalls().length }));
  const installs = knownInstalls();
  if (!availableInstalls().length) {
    const empty = make("div", "empty-state");
    empty.append(icon("grid"), make("h3", "", translate("noInstalls")), make("p", "", translate("noInstallsHint")));
    list.append(empty);
  }
  installs.forEach(install => {
    const unavailable = install.available === false;
    const card = make("div", `install-card${unavailable ? " unavailable" : install.id === state.defaultInstall ? " default" : ""}`);
    card.dataset.installId = install.id;
    const heading = make("div", "install-card-heading");
    heading.append(make("h3", "", `Vivado ${install.version}`));
    if (unavailable) {
      const badge = make("span", "unavailable-badge");
      badge.append(icon("warning"), document.createTextNode(translate("unavailableInstall")));
      heading.append(badge);
    }
    else if (install.id === state.defaultInstall) heading.append(make("span", "default-badge", translate("installedDefault")));
    else {
      const button = make("button", "secondary-button", translate("setDefault"));
      button.type = "button";
      button.dataset.defaultId = install.id;
      button.setAttribute("aria-label", `${translate("setDefault")} Vivado ${install.version}`);
      button.disabled = state.launching;
      button.addEventListener("click", () => {
        state.defaultInstall = install.id;
        if (!state.selectedProject?.bound && state.source !== "manualSource") pickInstall();
        messageAt("manage-status", "defaultsSaved", { version: install.version });
        render();
        element("refresh-button").focus();
      });
      heading.append(button);
    }
    card.append(heading, make("p", "", install.path));
    if (unavailable) {
      card.append(make("p", "install-warning", translate("unavailableInstallHint")));
      if (element("removed-install-actions")) {
        const actions = make("div", "install-record-actions");
        const remove = make("button", "secondary-button remove-record", translate("removeRecord"));
        remove.type = "button";
        remove.dataset.removeId = install.id;
        remove.setAttribute("aria-label", `${translate("removeRecord")} Vivado ${install.version}, ${install.path}`);
        remove.disabled = state.scanning || state.launching;
        remove.addEventListener("click", () => requestRemoveInstall(install));
        actions.append(remove);
        card.append(actions);
      }
    }
    list.append(card);
  });
  element("refresh-button").disabled = state.scanning || state.launching;
  if (element("removed-install-actions")) {
    element("removed-install-actions").hidden = state.removedInstalls.length === 0;
    text("removed-install-count", "removedRecords", { count: state.removedInstalls.length });
    element("undo-remove").disabled = state.scanning || state.launching;
  }
  if (focusedInstall) list.querySelector(`[data-default-id="${focusedInstall}"]`)?.focus();
  if (focusedRemoval) Array.from(list.querySelectorAll("[data-remove-id]")).find(button => button.dataset.removeId === focusedRemoval)?.focus();
}

function requestRemoveInstall(install) {
  if (state.scanning || state.launching || install.available !== false) return;
  const projects = [...new Map([...state.projects, ...(state.selectedProject ? [state.selectedProject] : [])].map(project => [project.id, project])).values()];
  const values = {
    version: install.version, path: install.path,
    bindings: projects.filter(project => project.bound === install.id).length,
    history: projects.filter(project => project.lastVersion === install.version).length,
    defaultNote: install.id === state.defaultInstall ? translate("removeDefaultNote") : ""
  };
  confirmAction("removeRecordTitle", "removeRecordBody", "removeRecord", values, () => {
    if (state.scanning || state.launching || state.removedInstalls.some(removed => removed.id === install.id)) return;
    state.removedInstalls.push({ ...install });
    for (const project of projects.filter(project => project.bound === install.id)) {
      project.boundVersion = install.version;
      project.boundPath = install.path;
    }
    if (state.defaultInstall === install.id) state.defaultInstall = null;
    if (state.selectedInstall === install.id || state.source !== "manualSource") pickInstall();
    messageAt("manage-status", "recordRemoved", { version: install.version });
    render();
    element("undo-remove").focus();
  });
}

function undoRemoveInstall() {
  if (state.scanning || state.launching) return;
  const install = state.removedInstalls.pop();
  if (!install) return;
  messageAt("manage-status", "recordRestored", { version: install.version });
  render();
  Array.from(element("install-list").querySelectorAll("[data-remove-id]")).find(button => button.dataset.removeId === install.id)?.focus();
}

function render() { renderProjects(); renderDetails(); renderStatus(); renderInstalls(); renderCleanEntry(); }

function unavailableProjects() { return state.projects.filter(project => !project.available); }

function makeRemoveButton(project) {
  const remove = make("button", "text-button row-remove", translate("removeRecord"));
  remove.type = "button";
  remove.dataset.removeId = project.id;
  remove.disabled = state.scanning || state.launching;
  remove.setAttribute("aria-label", `${translate("removeRecord")} ${project.path}`);
  remove.addEventListener("click", event => { event.stopPropagation(); requestRemoveProject(project); });
  remove.addEventListener("dblclick", event => event.stopPropagation());
  return remove;
}

function renderCleanEntry() {
  const count = unavailableProjects().length;
  const idle = !state.scanning && !state.launching;
  const button = element("clean-unavailable");
  if (button) {
    button.hidden = count === 0;
    button.disabled = !idle;
    text("clean-unavailable", "cleanUnavailable", { count });
  }
  const settings = element("clean-unavailable-settings");
  if (settings) settings.disabled = count === 0 || !idle;
  for (const remove of document.querySelectorAll("#project-list .row-remove")) remove.disabled = !idle;
  const undo = element("undo-clean");
  if (undo) {
    undo.hidden = state.cleanedBatches.length === 0;
    undo.disabled = !idle;
    text("undo-clean", "undoClean");
  }
}

function requestCleanUnavailable() {
  if (state.scanning || state.launching) return;
  const projects = unavailableProjects();
  if (!projects.length) {
    state.status = { key: "cleanNone", tone: "warning" };
    renderStatus();
    return;
  }
  const values = {
    count: projects.length,
    list: projects.slice(0, 3).map(project => project.path).join("\n"),
    more: projects.length > 3 ? translate("cleanUnavailableMore", { count: projects.length - 3 }) : ""
  };
  confirmAction("cleanUnavailableTitle", "cleanUnavailableBody", "cleanNow", values, () => removeUnavailable(projects));
}

function requestRemoveProject(project) {
  if (state.scanning || state.launching || project.available) return;
  confirmAction("removeProjectTitle", "cleanUnavailableBody", "removeRecord", { count: 1, list: project.path, more: "" }, () => removeUnavailable([project]));
}

function removeUnavailable(projects) {
  const batch = projects.filter(project => !project.available).map(project => ({ project, index: state.projects.indexOf(project) }));
  if (!batch.length) return;
  state.cleanedBatches.push(batch);
  state.projects = state.projects.filter(project => !batch.some(record => record.project === project));
  if (state.selectedProject && !state.selectedProject.available) {
    state.selectedProject = state.projects[0] || null;
    pickInstall();
  }
  state.status = { key: "cleanUnavailableDone", hint: "cleanHint", values: { count: batch.length } };
  if (element("settings-dialog")?.open) messageAt("settings-status", "cleanUnavailableDone", { count: batch.length });
  render();
  element("undo-clean")?.focus();
}

function undoClean() {
  if (state.scanning || state.launching || !state.cleanedBatches.length) return;
  const batch = state.cleanedBatches.pop();
  for (const record of batch.sort((first, second) => first.index - second.index))
    state.projects.splice(Math.min(record.index, state.projects.length), 0, record.project);
  if (!state.selectedProject) {
    state.selectedProject = state.projects[0] || null;
    pickInstall();
  }
  state.status = { key: "cleanRestored" };
  if (element("settings-dialog")?.open) messageAt("settings-status", "cleanRestored");
  render();
  element("clean-unavailable")?.focus();
}

function resetScenario(scenario) {
  clearTimeout(state.scanTimer);
  clearTimeout(state.launchTimer);
  clearTimeout(state.searchTimer);
  element("search-announcement").textContent = "";
  state.projects = baseProjects.map(project => ({ ...project }));
  state.installs = baseInstalls.map(install => ({ ...install }));
  Object.assign(state, { selectedProject: state.projects[0], defaultInstall: "xilinx-2024", scenario, query: "", scanning: false, launching: false, status: null, log: null, noLog: true, noJournal: true, extraArgs: "", removedInstalls: [], cleanedBatches: [] });
  element("search").value = "";
  element("no-log").checked = true;
  element("no-journal").checked = true;
  element("extra-args").value = "";
  element("launch-options").open = false;
  element("scenario").value = scenario;
  element("add-path-form").hidden = true;
  for (const id of ["manage-status", "settings-status", "browse-error", "install-error"]) clearMessage(id);
  if (scenario === "empty") { state.projects = []; state.selectedProject = null; }
  if (scenario === "no-installs") state.installs = [];
  if (["missing-binding", "missing-both", "warning-comparison"].includes(scenario)) {
    const missing = { id: "missing-2023", version: "2023.2", path: "D:\\EDA Tools\\Vivado\\2023.2", available: false };
    state.installs.push(missing);
    state.selectedProject.bound = missing.id;
    state.selectedProject.boundVersion = missing.version;
    state.selectedProject.boundPath = missing.path;
    state.projects[3].lastVersion = missing.version;
  }
  if (["missing-project", "missing-both"].includes(scenario)) state.selectedProject.available = false;
  if (scenario === "warning-comparison") state.projects[2].available = false;
  if (scenario === "stale-paths") {
    [1, 3, 5].forEach(index => { state.projects[index].available = false; });
    state.installs.push({ id: "missing-2023", version: "2023.2", path: "D:\\EDA Tools\\Vivado\\2023.2", available: false });
  }
  pickInstall();
  render();
  if (scenario === "scan") scan();
}

function projectFromPath(path) {
  const existing = state.projects.find(project => pathKey(project.path) === pathKey(path));
  if (existing) return existing;
  return { id: `path-${pathKey(path)}`, name: path.split("\\").at(-1), path, lastVersion: null, opened: "openedToday", available: true, bound: null };
}

function useProjectPath(path) {
  state.query = path;
  element("search").value = path;
  state.selectedProject = projectFromPath(path);
  state.status = null;
  pickInstall();
  render();
}

function handleSearch() {
  state.query = element("search").value;
  state.status = null;
  const path = normalizePath(state.query);
  if (isProjectPath(path)) {
    const project = pathKey(state.selectedProject?.path || "") === pathKey(path) ? state.selectedProject : projectFromPath(path);
    if (state.selectedProject !== project) { state.selectedProject = project; pickInstall(); }
  } else {
    const projects = filteredProjects();
    state.selectedProject = projects[0] || null;
    pickInstall();
  }
  render();
  clearTimeout(state.searchTimer);
  state.searchTimer = setTimeout(() => {
    if (!document.querySelector("dialog[open]")) element("search-announcement").textContent = element("project-count").textContent;
  }, 180);
}

function showBrowse() {
  if (state.launching) return;
  clearMessage("browse-error");
  element("browse-path").value = isProjectPath(normalizePath(state.query)) ? normalizePath(state.query) : "";
  element("browse-dialog").showModal();
}

function confirmAction(titleKey, descriptionKey, actionKey, values, action) {
  messageAt("confirm-title", titleKey, values);
  messageAt("confirm-description", descriptionKey, values);
  messageAt("confirm-action", actionKey);
  state.confirmAction = action;
  element("confirm-dialog").returnValue = "";
  element("confirm-dialog").showModal();
}

function requestLaunch() {
  if (element("launch-button").disabled || element("confirm-dialog").open) return;
  const project = state.selectedProject;
  if (project.bound && project.bound !== state.selectedInstall) {
    confirmAction("confirmMismatch", "confirmMismatchBody", "continueLaunch", { bound: boundVersion(project), selected: currentInstall().version }, launch);
  } else launch();
}

function launch() {
  if (element("launch-button").disabled) return;
  const project = state.selectedProject;
  const install = currentInstall();
  state.launching = true;
  const args = [state.noLog ? "-nolog" : "", state.noJournal ? "-nojournal" : "", state.extraArgs.trim(), `"${project.path}"`].filter(Boolean).join(" ");
  state.log = { project: project.path, install: install.path, args, failed: state.scenario === "launch-error" };
  render();
  state.launchTimer = setTimeout(() => {
    state.launching = false;
    if (state.log.failed) state.status = { key: "launchFailed", hint: "launchFailedHint", tone: "error" };
    else {
      project.lastVersion = install.version;
      project.opened = "openedToday";
      state.projects = [project, ...state.projects.filter(candidate => candidate.id !== project.id)].slice(0, 20);
      state.status = { key: "launched", hint: "launchedHint", values: { version: install.version } };
    }
    render();
  }, 1100);
}

function scan() {
  if (state.scanning || state.launching) return;
  state.scanning = true;
  messageAt("manage-status", "scanManaged");
  render();
  state.scanTimer = setTimeout(() => {
    const known = new Set(state.installs.map(install => pathKey(install.path)));
    baseInstalls.forEach(install => {
      if (!known.has(pathKey(install.path))) state.installs.push({ ...install });
      else if (element("removed-install-actions")) {
        const recovered = state.installs.find(candidate => pathKey(candidate.path) === pathKey(install.path));
        recovered.available = true;
        state.removedInstalls = state.removedInstalls.filter(removed => pathKey(removed.path) !== pathKey(install.path));
      }
    });
    state.installs.sort((first, second) => second.version.localeCompare(first.version, undefined, { numeric: true }));
    state.scanning = false;
    if (state.source !== "manualSource") pickInstall();
    state.status = { key: "scanDone", values: { count: availableInstalls().length } };
    messageAt("manage-status", "scanDone", { count: availableInstalls().length });
    render();
  }, 1500);
}

function renderLog() {
  if (!state.log) { text("log-content", "logEmpty"); return; }
  const log = state.log;
  element("log-content").textContent = `${translate("logSimulation")}\n\n${translate(log.failed ? "logFailure" : "logSuccess")}\n\n${translate("logProject")}:\n${log.project}\n\n${translate("logInstall")}:\n${log.install}\n\n${translate("logDirectory")}:\n${parentPath(log.project)}\n\n${translate("logArgs")}:\n${log.args}`;
}
function showLog() { renderLog(); element("log-dialog").showModal(); }

element("search").addEventListener("input", handleSearch);
element("version").addEventListener("change", () => {
  const install = availableInstalls().find(candidate => candidate.id === element("version").value);
  if (!install) { renderDetails(); return; }
  state.selectedInstall = install.id;
  state.source = "manualSource";
  state.status = null;
  renderDetails();
  renderStatus();
});
element("bind-button").addEventListener("click", () => {
  if (element("bind-button").disabled) return;
  const project = state.selectedProject;
  if (project.bound === state.selectedInstall) { project.bound = null; state.source = "manualSource"; state.status = { key: "bindRemoved" }; }
  else { project.bound = state.selectedInstall; state.source = "bindingSource"; state.status = { key: "bindSaved", values: { version: currentInstall().version } }; }
  render();
});
for (const [id, field] of [["no-log", "noLog"], ["no-journal", "noJournal"]]) {
  element(id).addEventListener("change", () => { state[field] = element(id).checked; renderDetails(); });
}
element("extra-args").addEventListener("input", () => { state.extraArgs = element("extra-args").value; renderDetails(); });
for (const id of ["language", "settings-language"]) element(id).addEventListener("change", () => { preferences.language = element(id).value; applyPreferences(); });
for (const id of ["theme", "settings-theme"]) element(id).addEventListener("change", () => { preferences.theme = element(id).value; applyPreferences(); });
mediaTheme.addEventListener("change", () => { if (preferences.theme === "system") applyPreferences(); });
element("manage-button").addEventListener("click", () => { renderInstalls(); element("manage-dialog").showModal(); });
element("settings-button").addEventListener("click", () => { element("clear-recent").disabled = state.launching; renderCleanEntry(); element("settings-dialog").showModal(); });
element("browse-button").addEventListener("click", showBrowse);
element("empty-browse").addEventListener("click", showBrowse);
element("launch-button").addEventListener("click", requestLaunch);
element("refresh-button").addEventListener("click", scan);
element("reset-button").addEventListener("click", () => resetScenario("normal"));
element("scenario").addEventListener("change", () => resetScenario(element("scenario").value));
element("add-path-button").addEventListener("click", () => { element("add-path-form").hidden = false; element("install-path").focus(); });
element("undo-remove")?.addEventListener("click", undoRemoveInstall);
element("clean-unavailable")?.addEventListener("click", requestCleanUnavailable);
element("clean-unavailable-settings")?.addEventListener("click", requestCleanUnavailable);
element("undo-clean")?.addEventListener("click", undoClean);
element("add-path-form").addEventListener("submit", event => {
  event.preventDefault();
  const path = normalizePath(element("install-path").value).replace(/\\+$/, "");
  if (!isAbsolutePath(path)) { messageAt("install-error", "invalidInstall"); return; }
  const existing = state.installs.find(install => pathKey(install.path) === pathKey(path));
  if (existing && (!element("removed-install-actions") || existing.available !== false)) { messageAt("install-error", "duplicateInstall"); return; }
  const version = path.match(/(?:^|\\)(20\d{2}\.\d+)(?:\\|$)/)?.[1] || "—";
  const removed = state.removedInstalls.find(install => pathKey(install.path) === pathKey(path));
  const install = existing || { id: removed?.id || `custom-${state.installs.length}`, path, version };
  install.available = true;
  if (!existing) state.installs.push(install);
  state.removedInstalls = state.removedInstalls.filter(record => pathKey(record.path) !== pathKey(path));
  state.installs.sort((first, second) => second.version.localeCompare(first.version, undefined, { numeric: true }));
  if (!state.selectedInstall && (!state.selectedProject?.bound || state.selectedProject.bound === install.id)) pickInstall();
  messageAt("manage-status", existing || removed ? "installRecovered" : "installAdded", { version });
  element("add-path-form").reset();
  element("add-path-form").hidden = true;
  clearMessage("install-error");
  render();
  element("add-path-button").focus();
});
element("browse-form").addEventListener("submit", event => {
  event.preventDefault();
  const path = normalizePath(element("browse-path").value);
  if (!isProjectPath(path)) { messageAt("browse-error", "invalidProject"); return; }
  element("browse-dialog").close();
  useProjectPath(path);
  element("search").focus();
});
element("sample-project").addEventListener("click", () => { element("browse-path").value = "D:\\FPGA\\pynq\\pynq_base_zu15.xpr"; clearMessage("browse-error"); });
element("confirm-dialog").addEventListener("close", () => {
  const confirmed = element("confirm-dialog").returnValue === "confirm";
  element("confirm-dialog").returnValue = "";
  const action = state.confirmAction;
  state.confirmAction = null;
  if (confirmed) action?.();
});
element("clear-recent").addEventListener("click", () => {
  confirmAction("confirmClear", "confirmClearBody", "clear", {}, () => {
    state.projects = [];
    state.selectedProject = null;
    state.selectedInstall = null;
    state.query = "";
    state.status = null;
    element("search").value = "";
    messageAt("settings-status", "cleared");
    render();
  });
});
element("associate-button").addEventListener("click", () => messageAt("settings-status", "associateDemo"));
element("settings-log").addEventListener("click", showLog);
element("view-error").addEventListener("click", showLog);
element("copy-path").addEventListener("click", async () => {
  if (!state.selectedProject) return;
  try { await navigator.clipboard.writeText(state.selectedProject.path); state.status = { key: "copied" }; }
  catch { state.status = { key: "copyFallback", tone: "warning" }; }
  renderStatus();
});

document.addEventListener("keydown", event => {
  if (event.isComposing || (event.key === "Enter" && event.keyCode === 229) || document.querySelector("dialog[open]")) return;
  if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "f") {
    event.preventDefault(); element("search").focus(); element("search").select(); return;
  }
  if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "o") {
    event.preventDefault(); showBrowse(); return;
  }
  const target = event.target;
  if (target !== element("search") && target !== element("project-list")) return;
  if (state.launching) return;
  if (event.key === "ArrowDown" || event.key === "ArrowUp") {
    event.preventDefault();
    const projects = filteredProjects();
    if (!projects.length) return;
    const current = projects.findIndex(project => project.id === state.selectedProject?.id);
    const offset = event.key === "ArrowDown" ? 1 : -1;
    const index = target === element("search") && current >= 0 ? current : Math.max(0, Math.min(projects.length - 1, current + offset));
    selectProject(projects[index]);
    element("project-list").focus({ preventScroll: true });
    document.querySelector('.project-row[aria-selected="true"]')?.scrollIntoView({ block: "nearest" });
  }
  if (event.key === "Enter" && !event.repeat) { event.preventDefault(); requestLaunch(); }
});

let dragDepth = 0;
element("app").addEventListener("dragenter", event => {
  if (!event.dataTransfer.types.includes("Files") || state.launching || document.querySelector("dialog[open]")) return;
  event.preventDefault(); dragDepth += 1; element("drop-overlay").hidden = false;
});
element("app").addEventListener("dragover", event => { if (event.dataTransfer.types.includes("Files")) event.preventDefault(); });
element("app").addEventListener("dragleave", () => { dragDepth = Math.max(0, dragDepth - 1); if (!dragDepth) element("drop-overlay").hidden = true; });
element("app").addEventListener("drop", event => {
  event.preventDefault(); dragDepth = 0; element("drop-overlay").hidden = true;
  if (state.launching || document.querySelector("dialog[open]")) return;
  const files = [...event.dataTransfer.files];
  if (files.length !== 1 || !/\.xpr$/i.test(files[0].name)) { state.status = { key: "invalidDrop", tone: "warning" }; renderStatus(); return; }
  showBrowse();
  element("browse-path").value = `D:\\PreviewDrops\\${files[0].name}`;
  messageAt("browse-error", "dropDemo");
});

resetScenario("normal");
applyPreferences();
document.querySelectorAll("svg.icon").forEach(svg => svg.setAttribute("aria-hidden", "true"));
element("search").focus({ preventScroll: true });
