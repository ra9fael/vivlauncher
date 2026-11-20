param(
    [ValidateSet('Build','Test','Publish')][string]$Action = 'Build',
    [ValidateSet('Net10','Net48','All')][string]$Target = 'All',
    [string]$Configuration = 'Release'
)
$ErrorActionPreference = 'Stop'
$solution = Join-Path $PSScriptRoot 'VivLauncher.slnx'
$tests = Join-Path $PSScriptRoot 'VivLauncher.Tests/VivLauncher.Tests.csproj'
$app = Join-Path $PSScriptRoot 'VivLauncher.App/VivLauncher.App.csproj'
$artifacts = Join-Path $PSScriptRoot 'artifacts'
$targets = if ($Target -eq 'All') { 'Net10', 'Net48' } else { $Target }
$records = foreach ($edition in $targets) {
    $framework = if ($edition -eq 'Net10') { 'net10.0-windows' } else { 'net48' }
    if ($Action -eq 'Build') {
        dotnet build $solution -c $Configuration -p:TargetFrameworks=$framework | Out-Host
        if ($LASTEXITCODE) { throw "$edition build failed." }
        continue
    }
    if ($Action -eq 'Test') {
        dotnet test $tests -c $Configuration -f $framework | Out-Host
        if ($LASTEXITCODE) { throw "$edition tests failed." }
        continue
    }
    $suffix = $edition.ToLowerInvariant() + '-win-x64'
    $destination = Join-Path $artifacts $suffix
    $name = "vivlauncher-$suffix.exe"
    $stage = Join-Path $artifacts ('publish-stage/' + [Guid]::NewGuid().ToString('N'))
    if ($edition -eq 'Net10') {
        dotnet publish $app -c $Configuration -f $framework -r win-x64 --self-contained false -o $stage -p:PublishSingleFile=true -p:PublishTrimmed=false -p:DebugType=embedded | Out-Host
        if ($LASTEXITCODE) { throw "$edition publish failed." }
    } else {
        dotnet build $app -c $Configuration -f $framework | Out-Host
        if ($LASTEXITCODE) { throw "$edition build failed." }
        $frameworkOutput = Join-Path $PSScriptRoot "VivLauncher.App/bin/$Configuration/$framework"
        $unexpectedDlls = @(Get-ChildItem -LiteralPath $frameworkOutput -File -Filter *.dll)
        if ($unexpectedDlls.Count) { throw "Unexpected unbundled dependencies: $($unexpectedDlls.Name -join ', ')" }
    }
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
    $unexpected = @(Get-ChildItem -LiteralPath $destination -Force | Where-Object Name -ne $name)
    if ($unexpected.Count) { throw "Release directory contains unexpected entries: $($unexpected.Name -join ', ')" }
    $executable = Join-Path $destination $name
    $candidate = Join-Path $stage 'vivlauncher.exe'
    if ($edition -eq 'Net48') {
        $standalone = New-Item -ItemType Directory -Path $stage -Force
        $candidate = Join-Path $standalone.FullName 'vivlauncher.exe'
        Copy-Item -LiteralPath (Join-Path $frameworkOutput 'vivlauncher.exe') -Destination $candidate
        & (Join-Path $PSScriptRoot 'tools/smoke-cli.ps1') -AppPath $candidate -Runtime '.NET Framework 4.8' | Out-Host
    } elseif (@(Get-ChildItem -LiteralPath $stage -Force).Count -ne 1) {
        throw "$edition publish must produce exactly one EXE."
    }
    Copy-Item -LiteralPath $candidate -Destination $executable -Force
    [pscustomobject]@{ File = "$suffix/$name"; Bytes = (Get-Item -LiteralPath $executable).Length; SHA256 = (Get-FileHash -LiteralPath $executable -Algorithm SHA256).Hash }
}
if ($Action -eq 'Publish') {
    ConvertTo-Json -InputObject @($records) | Set-Content -LiteralPath (Join-Path $artifacts ('release-' + $Target.ToLowerInvariant() + '.json')) -Encoding UTF8
    $records | Format-Table -AutoSize
}
