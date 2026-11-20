param(
    [ValidateSet('Build','Test','Publish')][string]$Action = 'Build',
    [string]$Configuration = 'Release'
)
$ErrorActionPreference = 'Stop'
$solution = Join-Path $PSScriptRoot 'VivLauncher.slnx'
if ($Action -eq 'Test') {
    dotnet test $solution -c $Configuration
    if ($LASTEXITCODE) { throw 'Tests failed.' }
} elseif ($Action -eq 'Publish') {
    $destination = Join-Path $PSScriptRoot 'artifacts/win-x64-exe'
    dotnet publish (Join-Path $PSScriptRoot 'VivLauncher.App/VivLauncher.App.csproj') -c $Configuration -r win-x64 --self-contained false -o $destination -p:PublishSingleFile=true -p:PublishTrimmed=false -p:DebugType=embedded
    if ($LASTEXITCODE) { throw 'Publish failed.' }
    $executable = Join-Path $destination 'vivlauncher.exe'
    $publishedFiles = @(Get-ChildItem -LiteralPath $destination -File -Recurse)
    if ($publishedFiles.Count -ne 1 -or $publishedFiles[0].FullName -ne $executable) {
        throw 'The release directory must contain only vivlauncher.exe.'
    }
    Get-Item -LiteralPath $executable | Select-Object FullName, Length
    Get-FileHash -LiteralPath $executable -Algorithm SHA256 | Format-List
} else {
    dotnet build $solution -c $Configuration
    if ($LASTEXITCODE) { throw 'Build failed.' }
}
