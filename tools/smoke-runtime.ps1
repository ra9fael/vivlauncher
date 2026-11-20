param(
    [string]$AppPath = "$PSScriptRoot/../artifacts/net10-win-x64/vivlauncher-net10-win-x64.exe",
    [switch]$CoreOnly
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes
$fixture = Join-Path ([IO.Path]::GetFullPath("$PSScriptRoot/../artifacts/runtime-check")) ([Guid]::NewGuid().ToString('N'))
$hostRoot = Join-Path $env:ProgramFiles 'dotnet/host/fxr'
$hostVersion = Get-ChildItem -LiteralPath $hostRoot -Directory | Where-Object Name -Match '^10\.0\.\d+$' | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
if (-not $hostVersion) { throw '.NET 10 hostfxr is required to create the isolated missing-framework fixture.' }
$fixtureHost = Join-Path $fixture ('host/fxr/' + $hostVersion.Name)
[IO.Directory]::CreateDirectory($fixtureHost) | Out-Null
Copy-Item -LiteralPath (Join-Path $hostVersion.FullName 'hostfxr.dll') -Destination $fixtureHost
if ($CoreOnly) {
    $coreRoot = Join-Path $env:ProgramFiles 'dotnet/shared/Microsoft.NETCore.App'
    $coreVersion = Get-ChildItem -LiteralPath $coreRoot -Directory | Where-Object Name -Match '^10\.0\.\d+$' | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
    if (-not $coreVersion) { throw '.NET 10 core runtime is required for the core-only fixture.' }
    $fixtureCore = Join-Path $fixture 'shared/Microsoft.NETCore.App'
    [IO.Directory]::CreateDirectory($fixtureCore) | Out-Null
    Copy-Item -LiteralPath $coreVersion.FullName -Destination $fixtureCore -Recurse
}
$standalone = Join-Path $fixture 'app'
[IO.Directory]::CreateDirectory($standalone) | Out-Null
$executable = Join-Path $standalone 'vivlauncher.exe'
Copy-Item -LiteralPath ([IO.Path]::GetFullPath($AppPath)) -Destination $executable
$start = [Diagnostics.ProcessStartInfo]::new($executable)
$start.UseShellExecute = $false
$start.WindowStyle = [Diagnostics.ProcessWindowStyle]::Hidden
$start.RedirectStandardError = $true
$start.EnvironmentVariables['DOTNET_ROOT_X64'] = $fixture
$start.EnvironmentVariables['DOTNET_ROOT'] = $fixture
$start.EnvironmentVariables.Remove('DOTNET_DISABLE_GUI_ERRORS')
$start.EnvironmentVariables['VIVLAUNCHER_PROFILE'] = Join-Path $fixture 'profile'
$start.EnvironmentVariables['VIVLAUNCHER_ISOLATED'] = '1'
$process = [Diagnostics.Process]::Start($start)
try {
    $window = $null
    for ($attempt = 0; $attempt -lt 80; $attempt++) {
        $window = [Windows.Automation.AutomationElement]::RootElement.FindFirst([Windows.Automation.TreeScope]::Children, [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::ProcessIdProperty, $process.Id))
        if ($window -or $process.HasExited) { break }
        Start-Sleep -Milliseconds 100
    }
    if (-not $window) { throw 'The native missing-runtime dialog did not appear.' }
    $elements = $window.FindAll([Windows.Automation.TreeScope]::Descendants, [Windows.Automation.Condition]::TrueCondition)
    $labels = @($elements | ForEach-Object { $_.Current.Name })
    $text = $labels -join "`n"
    if ($text -notmatch '\.NET' -or $text -notmatch 'Download it now') { throw "Unexpected native host dialog: $text" }
    $details = $window.FindFirst([Windows.Automation.TreeScope]::Descendants, [Windows.Automation.PropertyCondition]::new([Windows.Automation.AutomationElement]::NameProperty, 'See details'))
    if ($details) {
        $details.GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
        Start-Sleep -Milliseconds 200
        $elements = $window.FindAll([Windows.Automation.TreeScope]::Descendants, [Windows.Automation.Condition]::TrueCondition)
        $text = @($elements | ForEach-Object { $_.Current.Name }) -join "`n"
    }
    if ($CoreOnly -and $text -notmatch 'Microsoft\.WindowsDesktop\.App') { throw 'Host did not identify the missing Desktop Runtime.' }
    [IO.File]::WriteAllText((Join-Path $fixture 'native-dialog.txt'), $text)
    Write-Output 'PASS Native .NET apphost displays its missing-runtime dialog before application startup'
    Write-Output $text
    $window.GetCurrentPattern([Windows.Automation.WindowPattern]::Pattern).Close()
    if (-not $process.WaitForExit(5000)) { throw 'Missing-runtime process did not exit after closing its dialog.' }
    if ($process.ExitCode -eq 0) { throw 'A missing runtime must return a nonzero exit code.' }
    if (Test-Path -LiteralPath (Join-Path $fixture 'profile')) { throw 'Application profile was unexpectedly created before runtime resolution.' }
    Write-Output 'PASS Host exits with failure and does not execute application configuration code'
} finally {
    if (-not $process.HasExited) { $process.Kill() }
    $process.Dispose()
}
