param([string]$AppPath = "$PSScriptRoot/../VivLauncher.App/bin/Release/net10.0-windows/vivlauncher.exe")
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Drawing, System.Windows.Forms
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class WindowCapture {
    [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr window, out Rect rect);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr window, IntPtr dc, uint flags);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr window, int command);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr window);
    [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr window, IntPtr after, int x, int y, int width, int height, uint flags);
    [DllImport("kernel32.dll", CharSet=CharSet.Unicode)] public static extern bool WritePrivateProfileString(string section, string key, string value, string path);
}
'@
$output = [IO.Path]::GetFullPath("$PSScriptRoot/../artifacts/qa/" + [IO.Path]::GetFileNameWithoutExtension($AppPath))
$fixture = Join-Path $output ('profile-' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($fixture) | Out-Null
$settings = Join-Path $fixture 'settings.ini'
[IO.File]::WriteAllText($settings, '', [Text.Encoding]::Unicode)
function Set-Setting($section, $key, $value) {
    if (-not [WindowCapture]::WritePrivateProfileString($section, $key, $value, $settings)) { throw 'Failed to seed settings' }
}
function Get-Key($path) {
    $hash = [Numerics.BigInteger]::Parse('14695981039346656037')
    $modulus = [Numerics.BigInteger]::Pow(2,64)
    foreach ($character in $path.ToLowerInvariant().ToCharArray()) {
        $hash = (($hash -bxor [int]$character) * [Numerics.BigInteger]::Parse('1099511628211')) % $modulus
    }
    'project_' + ([UInt64]$hash).ToString('X16')
}
$available = Join-Path $fixture 'AMD tools 设计/2025.2/Vivado'
$missing = Join-Path $fixture 'Xilinx/Vivado/2023.2'
[IO.Directory]::CreateDirectory((Join-Path $available 'bin')) | Out-Null
[IO.File]::WriteAllText((Join-Path $available 'bin/vivado.bat'), '@exit /b 0')
[IO.File]::WriteAllLines((Join-Path $fixture 'paths.txt'), @($available, $missing), [Text.UTF8Encoding]::new($false))
$projectPaths = @()
$projectNames = @('hifi/project_1.xpr','zu15/project_1.xpr','信号处理/工程一.xpr','pynq/pynq_base_zu15.xpr','long directory with spaces FPGA system implementation/archive/project.xpr')
for ($index = 0; $index -lt $projectNames.Count; $index++) {
    $projectPath = Join-Path $fixture ('projects/' + $projectNames[$index])
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($projectPath)) | Out-Null
    if ($index -ne 4) { [IO.File]::WriteAllText($projectPath, '<Project />') }
    $projectPaths += $projectPath
    $version = if ($index -in 3,4) { '2022.2' } else { '2025.2' }
    Set-Setting 'recent_projects' "recent_$index" "$version|$projectPath"
}
Set-Setting 'project_installs' (Get-Key $projectPaths[0]) $missing
Set-Setting 'vivlauncher' 'default_install' $available
Set-Setting 'wpf' 'theme' 'light'
$env:VIVLAUNCHER_PROFILE = $fixture
$env:VIVLAUNCHER_ISOLATED = '1'
$process = Start-Process -FilePath ([IO.Path]::GetFullPath($AppPath)) -WindowStyle Hidden -PassThru
$script:checks = 0
function Assert-True($condition, $message) { if (-not $condition) { throw $message }; $script:checks++; Write-Output "PASS $message" }
function Find-Id($root, $id) {
    $root.FindFirst([Windows.Automation.TreeScope]::Descendants, [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::AutomationIdProperty, $id))
}
function Find-Window($title) {
    $desktop = [Windows.Automation.AutomationElement]::RootElement
    $processCondition = [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::ProcessIdProperty, $process.Id)
    $nameCondition = [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::NameProperty, $title)
    $condition = [Windows.Automation.AndCondition]::new($processCondition, $nameCondition)
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        $window = $desktop.FindFirst([Windows.Automation.TreeScope]::Children, $condition)
        if (-not $window) { $window = $root.FindFirst([Windows.Automation.TreeScope]::Descendants, $condition) }
        if ($window) { return $window }
        Start-Sleep -Milliseconds 150
    }
}
function Invoke-Button($element) { $element.GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke(); Start-Sleep -Milliseconds 180 }
function Set-Text($element, $text) { $element.GetCurrentPattern([Windows.Automation.ValuePattern]::Pattern).SetValue($text); Start-Sleep -Milliseconds 200 }
function Capture($window, $name) {
    $handle = [IntPtr]$window.Current.NativeWindowHandle
    $rect = [WindowCapture+Rect]::new()
    [WindowCapture]::GetWindowRect($handle, [ref]$rect) | Out-Null
    $bitmap = [Drawing.Bitmap]::new($rect.Right-$rect.Left, $rect.Bottom-$rect.Top)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    $dc = $graphics.GetHdc()
    try { [WindowCapture]::PrintWindow($handle, $dc, 2) | Out-Null }
    finally { $graphics.ReleaseHdc($dc); $graphics.Dispose() }
    $bitmap.Save((Join-Path $output $name), [Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
}
try {
    $root = $null
    for ($attempt=0; $attempt -lt 80; $attempt++) {
        if ($process.HasExited) { throw "Application exited with $($process.ExitCode)" }
        $root = [Windows.Automation.AutomationElement]::RootElement.FindFirst([Windows.Automation.TreeScope]::Children, [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::ProcessIdProperty, $process.Id))
        if ($root -and (Find-Id $root 'Search')) { break }
        Start-Sleep -Milliseconds 100
    }
    Assert-True ($null -ne $root) 'Native WPF window opens'
    [WindowCapture]::ShowWindow([IntPtr]$root.Current.NativeWindowHandle, 5) | Out-Null
    Start-Sleep -Milliseconds 1000
    $search = Find-Id $root 'Search'
    $launch = Find-Id $root 'Launch'
    $initialList = Find-Id $root 'Projects'
    $initialItem = $initialList.FindFirst([Windows.Automation.TreeScope]::Children, [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::ControlTypeProperty, [Windows.Automation.ControlType]::ListItem))
    $initialItem.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
    Start-Sleep -Milliseconds 200
    Capture $root '01-native-light-zh.png'
    Assert-True (-not $launch.Current.IsEnabled) 'Missing binding blocks launch'
    Assert-True ((Find-Id $root 'ProjectIssues').Current.Name -eq '绑定失效') 'Binding warning uses the shared details area'
    Set-Text $search 'pjt1'
    $list = Find-Id $root 'Projects'
    $items = $list.FindAll([Windows.Automation.TreeScope]::Children, [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::ControlTypeProperty, [Windows.Automation.ControlType]::ListItem))
    Assert-True ($items.Count -ge 2) 'fzf non-consecutive search returns project matches'
    Capture $root '02-native-fzf.png'
    Set-Text $search "'project_1.xpr"
    $items = $list.FindAll([Windows.Automation.TreeScope]::Children, [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::ControlTypeProperty, [Windows.Automation.ControlType]::ListItem))
    Assert-True ($items.Count -eq 2) 'Exact extended query returns two duplicate filenames'
    Set-Text $search 'zzzzzzzz'
    Assert-True (-not $launch.Current.IsEnabled) 'Empty results disable launch'
    Set-Text $search $projectPaths[2]
    Assert-True ((Find-Id $root 'ProjectPath').GetCurrentPattern([Windows.Automation.ValuePattern]::Pattern).Current.Value -eq $projectPaths[2]) 'Pasted Unicode absolute project path bypasses fuzzy syntax'
    Assert-True ($launch.Current.IsEnabled) 'Available pasted project can launch'
    Invoke-Button (Find-Id $root 'Bind')
    Set-Text $search "'工程一.xpr"
    Assert-True ((Find-Id $root 'Bind').Current.Name.Contains('已绑定')) 'Binding a pasted project updates its history row'
    Invoke-Button (Find-Id $root 'Bind')
    Assert-True ((Find-Id $root 'Bind').Current.Name -eq '记住此工程的版本') 'Unbinding updates the selected history project'
    Set-Text $search (Join-Path $fixture 'missing.xpr')
    Assert-True (-not $launch.Current.IsEnabled) 'Unavailable pasted project cannot launch'
    Assert-True ((Find-Id $root 'ProjectIssues').Current.Name -eq '路径失效') 'Path warning uses the shared details area'
    Set-Text $search 'pynq_base_zu15'
    Assert-True ((Find-Id $root 'ProjectIssues').Current.Name -eq '版本失效') 'Version warning uses the shared details area'
    Set-Text $search 'long directory'
    Assert-True ((Find-Id $root 'ProjectIssues').Current.Name -eq '路径失效, 版本失效') 'Simultaneous warnings stack in the same details area'
    Set-Text $search ''
    $picker = Find-Id $root 'VersionPicker'
    $picker.GetCurrentPattern([Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
    Start-Sleep -Milliseconds 200
    $versions = $picker.FindAll([Windows.Automation.TreeScope]::Descendants, [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::ControlTypeProperty, [Windows.Automation.ControlType]::ListItem))
    $replacement = $null
    $unavailableCount = 0
    foreach ($version in $versions) {
        if ($version.Current.IsEnabled) { $replacement = $version } else { $unavailableCount++ }
    }
    Assert-True ($unavailableCount -ge 2) 'Missing bound and historical versions remain disabled in picker'
    $replacement.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
    Start-Sleep -Milliseconds 200
    Assert-True ($launch.Current.IsEnabled) 'Explicit replacement enables launch'
    Invoke-Button $launch
    $confirm = $root.FindFirst([Windows.Automation.TreeScope]::Descendants, [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::ControlTypeProperty, [Windows.Automation.ControlType]::Window))
    Assert-True ($confirm.Current.Name -eq '使用不同版本打开？') 'Changing a bound installation requires confirmation'
    $confirm.GetCurrentPattern([Windows.Automation.WindowPattern]::Pattern).Close()
    Start-Sleep -Milliseconds 200
    Invoke-Button (Find-Id $root 'Manage')
    $manage = $root.FindFirst([Windows.Automation.TreeScope]::Descendants, [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::ControlTypeProperty, [Windows.Automation.ControlType]::Window))
    if (-not $manage) { $manage = [Windows.Automation.AutomationElement]::RootElement.FindFirst([Windows.Automation.TreeScope]::Children, [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::NameProperty, '安装管理')) }
    Assert-True ($null -ne (Find-Id $manage 'InstallationList')) 'Installation manager opens'
    Capture $manage '03-native-installations.png'
    Invoke-Button (Find-Id $manage 'RemoveInstall')
    $removal = Find-Window '移除失效的安装记录？'
    Assert-True ($null -ne $removal) 'Removal asks for confirmation'
    Invoke-Button (Find-Id $removal 'DialogCancel')
    Assert-True ($null -ne (Find-Id $manage 'RemoveInstall')) 'Cancel keeps the missing installation'
    Invoke-Button (Find-Id $manage 'RemoveInstall')
    $removal = Find-Window '移除失效的安装记录？'
    Capture $removal '08-native-remove-confirmation.png'
    Invoke-Button (Find-Id $removal 'DialogConfirm')
    Start-Sleep -Milliseconds 400
    Assert-True ($null -eq (Find-Id $manage 'RemoveInstall')) 'Confirmed removal hides the installation'
    Assert-True ([IO.File]::ReadAllText($settings).Contains((Get-Key $projectPaths[0]) + '=' + $missing)) 'Removal keeps the project binding'
    Invoke-Button (Find-Id $manage 'Scan')
    Assert-True ($null -eq (Find-Id $manage 'RemoveInstall')) 'Rescan does not revive the unavailable record'
    Invoke-Button (Find-Id $manage 'UndoRemoval')
    Assert-True ($null -ne (Find-Id $manage 'RemoveInstall')) 'Undo restores the unavailable record'
    $manage.GetCurrentPattern([Windows.Automation.WindowPattern]::Pattern).Close()
    Invoke-Button (Find-Id $root 'Settings')
    $settingsWindow = $root.FindFirst([Windows.Automation.TreeScope]::Descendants, [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::ControlTypeProperty, [Windows.Automation.ControlType]::Window))
    if (-not $settingsWindow) { $settingsWindow = [Windows.Automation.AutomationElement]::RootElement.FindFirst([Windows.Automation.TreeScope]::Children, [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::NameProperty, '设置')) }
    $language = Find-Id $settingsWindow 'Language'
    $language.GetCurrentPattern([Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
    $english = $language.FindFirst([Windows.Automation.TreeScope]::Descendants, [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::NameProperty, 'English'))
    $english.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
    Start-Sleep -Milliseconds 300
    Assert-True ($settingsWindow.Current.Name -eq 'Settings') 'Language switches immediately'
    $theme = Find-Id $settingsWindow 'Theme'
    $theme.GetCurrentPattern([Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
    $dark = $theme.FindFirst([Windows.Automation.TreeScope]::Descendants, [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::NameProperty, 'Dark'))
    $dark.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
    Start-Sleep -Milliseconds 400
    Capture $settingsWindow '04-native-settings.png'
    $settingsWindow.GetCurrentPattern([Windows.Automation.WindowPattern]::Pattern).Close()
    Capture $root '05-native-dark-en.png'
    Assert-True ((Find-Id $root 'Launch').Current.Name -eq 'Open project') 'Main window labels update to English'
    [WindowCapture]::SetWindowPos([IntPtr]$root.Current.NativeWindowHandle, [IntPtr]::Zero, 0,0,720,480,6) | Out-Null
    Start-Sleep -Milliseconds 300
    Capture $root '06-native-minimum.png'
    [WindowCapture]::SetWindowPos([IntPtr]$root.Current.NativeWindowHandle, [IntPtr]::Zero, 0,0,980,700,6) | Out-Null
    [WindowCapture]::SetForegroundWindow([IntPtr]$root.Current.NativeWindowHandle) | Out-Null
    $search.SetFocus()
    [Windows.Forms.SendKeys]::SendWait('{DOWN}')
    Start-Sleep -Milliseconds 250
    $projectValue = (Find-Id $root 'ProjectPath').GetCurrentPattern([Windows.Automation.ValuePattern]::Pattern).Current.Value
    Assert-True ($projectValue -eq $projectPaths[1]) 'Down arrow selects the next project from search'
    $batchPath = Join-Path $available 'bin/vivado.bat'
    $resolvedBatch = [IO.Path]::GetFullPath($batchPath)
    if (-not $resolvedBatch.StartsWith($fixture + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Fixture boundary check failed' }
    Remove-Item -LiteralPath $resolvedBatch
    $search.SetFocus()
    [Windows.Forms.SendKeys]::SendWait('{ENTER}')
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        $status = (Find-Id $root 'Status').Current.Name
        if ($status.Contains('Launch failed')) { break }
        Start-Sleep -Milliseconds 100
    }
    Assert-True ($status.Contains('Launch failed')) "Enter reports launch failure without closing window ($status)"
    Capture $root '07-native-launch-error.png'
    [IO.File]::WriteAllText($batchPath, '@exit /b 0')
    Invoke-Button $launch
    Assert-True ($process.WaitForExit(5000)) 'Successful fixture launch closes the launcher'
    $saved = [IO.File]::ReadAllText($settings)
    Assert-True ($saved.Contains('recent_0=2025.2|' + $projectPaths[1])) 'Successful process creation updates recent history'
    Assert-True ($saved.Contains('language=en') -and $saved.Contains('theme=dark')) 'Language and appearance persist'
    Write-Output "Passed $script:checks native UI checks. Screenshots: $output"
} finally {
    if (-not $process.HasExited) { $process.CloseMainWindow() | Out-Null; if (-not $process.WaitForExit(3000)) { $process.Kill() } }
    Remove-Item Env:VIVLAUNCHER_PROFILE, Env:VIVLAUNCHER_ISOLATED -ErrorAction SilentlyContinue
}
