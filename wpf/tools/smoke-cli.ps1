param([string]$AppPath = "$PSScriptRoot/../VivLauncher.App/bin/Release/net10.0-windows/vivlauncher.exe")
$ErrorActionPreference = 'Stop'
$fixture = Join-Path ([IO.Path]::GetFullPath("$PSScriptRoot/../artifacts/cli")) ([Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($fixture) | Out-Null
function Invoke-Cli($arguments) {
    $start = [Diagnostics.ProcessStartInfo]::new([IO.Path]::GetFullPath($AppPath), $arguments)
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
    if ($result.ExitCode -ne 0 -or -not $result.Out.Contains('Usage:')) { throw "Help redirection failed: $verb" }
}
$result = Invoke-Cli '--licenses'
if ($result.ExitCode -ne 0 -or -not $result.Out.Contains('BSD 3-Clause') -or -not $result.Out.Contains('MIT License')) { throw 'Embedded license notices failed' }
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
