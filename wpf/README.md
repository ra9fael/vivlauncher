# Vivado Launcher — native WPF edition

This implements the approved revision 02 prototype in `prototypes/wpf-launcher/`. The original C++ application remains in `vivlauncher/`. No WebView, Node.js or browser runtime is used by the WPF application.

## Run and build

Windows x64; users install **.NET 10 Desktop Runtime (x64)** themselves. Source builds require .NET 10 SDK with Windows Desktop support. The ordinary .NET Runtime and .NET Framework 4.8 do not satisfy the desktop runtime requirement.

```powershell
./wpf/build.ps1 -Action Build
./wpf/build.ps1 -Action Test
./wpf/build.ps1 -Action Publish
```

The release is the single file **`wpf/artifacts/win-x64-exe/vivlauncher.exe`**. Publish and distribute this EXE directly. It bundles the application, managed dependencies, configuration metadata, debug symbols and third-party notices; it does **not** bundle .NET. No companion DLL, JSON, PDB or ZIP is required. Keep the EXE in a stable location before registering the `.xpr` file handler in Settings.

If a compatible runtime is missing, the standard .NET apphost reports the missing dependency and offers a download link before application startup. Windows GUI launches normally display an installation prompt; the app does not install the runtime automatically. Install the **Windows x64 .NET 10 Desktop Runtime** from https://dotnet.microsoft.com/en-us/download/dotnet/10.0, then run the EXE again. An installed .NET 10 SDK also satisfies this requirement. A different major runtime version is not a substitute under the default runtime selection policy.

Earlier `artifacts/win-x64/` and `artifacts/vivlauncher-wpf-win-x64.zip` outputs are retained historical self-contained builds; they are no longer the default release artifacts.

```text
vivlauncher.exe [project.xpr]
vivlauncher.exe --list
vivlauncher.exe --add "D:\AMD tools\2025.2\Vivado"
vivlauncher.exe --help
vivlauncher.exe --licenses
```

The short forms `-l`, `-a`, `-h` and `list`, `add`, `help` are also supported. CLI output stays in the caller's redirected stream; errors use stderr and a nonzero exit code.

## Workflow

- `Ctrl+F` searches recent projects, `Ctrl+O` browses, arrow keys select, Enter or double-click opens. IME composition does not trigger launch. Drop one `.xpr` or paste its complete path.
- Search uses the fzf v2 algorithm, smart case, exact terms (`'text`), exclusions (`!text`), OR (`term | term`), prefix (`^text`), suffix (`text$`) and escaped spaces. Scores rank results; ties retain recent order. Matched characters are highlighted in names and directories.
- Project bindings use installation **paths**, so two installations with the same version remain distinct. Missing bindings block launch until a replacement is chosen. A changed version requires confirmation and does not overwrite the binding automatically.
- Unavailable projects, bound installations and last-used versions remain in lists with warning labels. Scans run in the background. Choose **Scan again** after reconnecting a drive.
- Settings changes language and light/dark/system appearance instantly. File associations change only after pressing **Associate .xpr** and confirming.

## Compatibility and files

The app reads and writes `%APPDATA%\vivlauncher\settings.ini`, keeping unrelated sections and legacy project hash keys. On first write, an ANSI INI is backed up as `settings.ini.pre-wpf.bak` and converted to UTF-16 so Unicode paths round-trip through both launchers. `paths.txt` and the old line-based `paths.json` are supported, including spaces and Unicode. Version-only bindings/defaults remain readable. New preferences use `[wpf]`; discovered installation paths use `[known_installs]` so unavailable installations can still be shown. The latest 20 successful process creations populate recent history. Clearing history never deletes projects or bindings.

Logs are stored at `%LOCALAPPDATA%\vivlauncher\vivlauncher.log`. The preferred Vivado process is `bin\unwrapped\win64.o\vvgl.exe`, with the batch entry point and arguments passed separately. If it is missing, the app invokes the batch entry point through Windows `cmd.exe`, with delayed expansion disabled and values passed through environment variables to preserve spaces, Unicode and shell metacharacters. Embedded literal quotes within individual arguments require the wrapper; the fallback reports that limitation instead of reinterpreting them as commands. A successful launch means Windows created the process, not that Vivado finished loading or accepted the project.

The native Fluent theme comes from WPF itself. .NET 10 still marks its runtime `ThemeMode` API as experimental (`WPF0001`); that diagnostic alone is suppressed in the application project. Theme handling is isolated in `ThemeService.cs`.

## Verification and isolation

`VivLauncher.Tests` covers the matcher, legacy configuration, path identity, unavailable installations, binding selection, launch arguments and the real batch fallback with a harmless capture script. Search parity fixtures are generated from the prototype's pinned `fzf@0.5.2` bundle:

```powershell
node wpf/tools/generate-fzf-fixtures.cjs
dotnet test wpf/VivLauncher.Tests -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File wpf/tools/smoke-ui.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File wpf/tools/smoke-cli.ps1 -AppPath ./wpf/artifacts/win-x64-exe/vivlauncher.exe
powershell -NoProfile -ExecutionPolicy Bypass -File wpf/tools/smoke-runtime.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File wpf/tools/smoke-runtime.ps1 -CoreOnly
```

The generator resets the JavaScript library per case: its shared scratch slab can retain stale backtracking data across searches. The C# implementation allocates clean matrices, so repeated searches give stable highlights. The generated normalization table and fixtures are committed source assets; Node is only needed when regenerating them.

The native UI smoke script uses Windows UI Automation and screenshots, with fixture files under `wpf/artifacts/qa/`. It never reads or writes the normal application profile. `VIVLAUNCHER_PROFILE` optionally selects an isolated configuration directory; `VIVLAUNCHER_ISOLATED=1` limits discovery to paths recorded in that profile. These variables are intended for diagnostics/testing, not required for normal use.

Before replacing the C++ launcher permanently, verify launching your real Vivado project and monitor transitions at 100%, 150% and 200% DPI. Process-capture fixtures do not validate Vivado licensing, project upgrades or a particular vendor installation.
