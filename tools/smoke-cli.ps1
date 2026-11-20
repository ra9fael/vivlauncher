param([string]$AppPath = "$PSScriptRoot/../VivLauncher.App/bin/Release/net10.0-windows/vivlauncher.exe", [string]$Runtime = '.NET 10 Desktop Runtime')
$ErrorActionPreference = 'Stop'
$fixture = Join-Path ([IO.Path]::GetFullPath("$PSScriptRoot/../artifacts/cli")) ([Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($fixture) | Out-Null
function Invoke-Cli($arguments, $executable = $AppPath) {
    $start = [Diagnostics.ProcessStartInfo]::new([IO.Path]::GetFullPath($executable), $arguments)
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.StandardOutputEncoding = [Text.Encoding]::UTF8
    $start.StandardErrorEncoding = [Text.Encoding]::UTF8
    $start.EnvironmentVariables['VIVLAUNCHER_PROFILE'] = $fixture
    $start.EnvironmentVariables['VIVLAUNCHER_ISOLATED'] = '1'
    $process = [Diagnostics.Process]::Start($start)
    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    if (-not $process.WaitForExit(10000)) { $process.Kill(); throw 'CLI timed out' }
    $result = @{ ExitCode = $process.ExitCode; Out = $stdout; Error = $stderr }
    $process.Dispose()
    $result
}
foreach ($verb in @('--help','-h','help')) {
    $result = Invoke-Cli $verb
    if ($result.ExitCode -ne 0 -or -not $result.Out.Contains('Usage:') -or -not $result.Out.Contains($Runtime)) { throw "Help redirection or runtime label failed: $verb $($result.Error)" }
}
$result = Invoke-Cli '--licenses'
if ($result.ExitCode -ne 0 -or -not $result.Out.Contains('BSD 3-Clause') -or -not $result.Out.Contains('MIT License')) { throw 'Embedded license notices failed' }
if ($Runtime -like '.NET Framework*' -and -not $result.Out.Contains('2012 Simon Cropp and contributors')) { throw 'Embedded Costura license is missing' }
$result = Invoke-Cli '--list'
if ($result.ExitCode -ne 0 -or -not $result.Out.Contains('No Vivado')) { throw 'Empty list failed' }
$result = Invoke-Cli '--add'
if ($result.ExitCode -eq 0 -or -not $result.Error.Contains('requires')) { throw 'CLI error status or stderr failed' }
$install = Join-Path $fixture 'AMD tools 2025.2/Vivado'
[IO.Directory]::CreateDirectory((Join-Path $install 'bin')) | Out-Null
[IO.File]::WriteAllText((Join-Path $install 'bin/vivado.bat'), '@exit /b 0')
$result = Invoke-Cli ('--add "' + $install + '"')
if ($result.ExitCode -ne 0 -or -not $result.Out.Contains('Added:')) { throw 'Add installation failed' }
$result = Invoke-Cli '-l'
if ($result.ExitCode -ne 0 -or -not $result.Out.Contains($install)) { throw 'List installation failed' }
Write-Output 'PASS CLI help aliases, embedded licenses, redirected stdout/stderr, empty discovery, invalid add, add path with spaces, list'
$other = if ($Runtime -eq '.NET 10 Desktop Runtime') { $env:VIVLAUNCHER_NET48_EXE } else { $env:VIVLAUNCHER_NET10_EXE }
if ($other) {
    $result = Invoke-Cli '--list' $other
    if ($result.ExitCode -ne 0 -or -not $result.Out.Contains($install)) { throw 'Other edition could not read the shared path.' }
    $alternate = Join-Path $fixture 'AMD tools 2024.2/Vivado'
    [IO.Directory]::CreateDirectory((Join-Path $alternate 'bin')) | Out-Null
    [IO.File]::WriteAllText((Join-Path $alternate 'bin/vivado.bat'), '@exit /b 0')
    $result = Invoke-Cli ('--add "' + $alternate + '"') $other
    if ($result.ExitCode -ne 0) { throw 'Other edition could not write the shared profile.' }
    $result = Invoke-Cli '--list'
    if ($result.ExitCode -ne 0 -or -not $result.Out.Contains($alternate) -or -not $result.Out.Contains($install)) { throw 'Shared profile round trip failed.' }
    Write-Output 'PASS both editions read and update the same installation profile'
}
