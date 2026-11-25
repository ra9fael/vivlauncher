# Vivado Launcher — native WPF edition

The native C# / WPF launcher on `main` implements the approved revision 03 design in `prototypes/dual-runtime/`. Both editions share the same layout and functionality. The original C++ / Win32 implementation and build instructions are on [legacy-win32](https://github.com/ra9fael/vivlauncher/tree/legacy-win32).

## Screenshot

![Vivado Launcher screenshot](https://github.com/user-attachments/assets/2ff842c6-229b-407b-a3b1-2058e0eec1c3)

## Run and build

Both editions target Windows 10 22H2 and Windows 11 x64. Pick the executable for the runtime on your machine.

| Executable | Runtime | Theme |
| --- | --- | --- |
| `vivlauncher-net10-win-x64.exe` | .NET 10 Desktop Runtime x64 | Official WPF Fluent |
| `vivlauncher-net48-win-x64.exe` | .NET Framework 4.8 or 4.8.1 | Compatible WPF styles |

Build with the .NET 10 SDK and the .NET Framework 4.8 reference assemblies.

```powershell
./build.ps1 -Action Build
./build.ps1 -Action Test
./build.ps1 -Action Publish
./build.ps1 -Action Publish -Target Net48
```

`-Target` accepts `Net10`, `Net48`, and `All` (default). Publish writes one independent EXE to each edition's directory under `artifacts/`, plus a JSON manifest with file sizes and SHA-256 hashes. Users provide the runtime. The modern edition uses the standard apphost install prompt when its [Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) is missing. Windows 10 22H2 and Windows 11 ship with a compatible .NET Framework by default.

Keep the EXE in a stable location before registering the `.xpr` file handler. Registration points the command and file icon at the edition you select in Settings.

```text
vivlauncher.exe [project.xpr]
vivlauncher.exe --list
vivlauncher.exe --add "D:\AMD tools\2025.2\Vivado"
vivlauncher.exe --help
vivlauncher.exe --licenses
```

The short forms `-l`, `-a`, `-h` and `list`, `add`, `help` are also supported. CLI output stays in the caller's redirected stream; errors use stderr and a nonzero exit code.

## Releases

Push a new Git tag on the release commit to trigger the workflow. For version `v2.2.0`, run these commands from that commit.

```powershell
git tag -a v2.2.0 -m "Vivado Launcher v2.2.0"
git push origin refs/tags/v2.2.0
```

GitHub Actions tests and publishes both targets on Windows. Once the tests and CLI smoke checks pass, both EXEs and their matching `.exe.sha256` files are available on the [GitHub Releases page](https://github.com/ra9fael/vivlauncher/releases).

## Workflow

- `Ctrl+F` searches recent projects, `Ctrl+O` browses, arrow keys select, Enter or double-click opens. IME composition does not trigger launch. Drop one `.xpr` or paste its complete path.
- Search uses the fzf v2 algorithm, smart case, exact terms (`'text`), exclusions (`!text`), OR (`term | term`), prefix (`^text`), suffix (`text$`) and escaped spaces. Scores rank results; ties retain recent order. Matched characters are highlighted in names and directories.
- Project bindings use installation **paths**, so two installations with the same version remain distinct. Missing bindings block launch until a replacement is chosen. A changed version requires confirmation and does not overwrite the binding automatically.
- Unavailable paths, bindings and last-used versions share the same warning treatment. Simultaneous warnings stack in the same column and details panel.
- In **Installations**, **Remove record** hides an unavailable installation and preserves the associated projects, bindings and history. Use **Undo last removal** in the current session, or reconnect the original path and scan again to recover it.
- Missing recent projects are marked **Path unavailable**. **Clean unavailable paths (N)** beside the Recent projects heading removes every missing record after a confirmation, and each unavailable row repeats the same action as **Remove record** for one project at a time. **Undo cleanup** in the status bar restores the last cleanup or single removal. Project files, bindings and installation records are never touched.
- Use the language dropdown at the top right or open Settings to change the display language. New profiles follow the Windows display language. The app remembers a manual choice and applies it immediately. Settings also controls light, dark, and system appearance. File associations change only after pressing **Associate .xpr** and confirming.

## Compatibility and files

The app reads and writes `%APPDATA%\vivlauncher\settings.ini`, keeping unrelated sections and legacy project hash keys. On first write, an ANSI INI is backed up as `settings.ini.pre-wpf.bak` and converted to UTF-16 so Unicode paths round-trip through both launchers. `paths.txt` and the old line-based `paths.json` are supported, including spaces and Unicode. Version-only bindings/defaults remain readable. New preferences use `[wpf]`; discovered installation paths use `[known_installs]` so unavailable installations can still be shown. The latest 20 successful process creations populate recent history. Clearing history never deletes projects or bindings.

Logs are stored at `%LOCALAPPDATA%\vivlauncher\vivlauncher.log`. The preferred Vivado process is `bin\unwrapped\win64.o\vvgl.exe`, with the batch entry point and arguments passed separately. Both editions build the same command line: an argument is quoted only when it is empty or contains a space, tab or quote, so the `vivado.bat` path reaches `vvgl.exe` without quotes and is located correctly. If the wrapper is missing, the app invokes the batch entry point through Windows `cmd.exe`, with delayed expansion disabled and values passed through environment variables to preserve spaces, Unicode and shell metacharacters. Embedded literal quotes within individual arguments require the wrapper; the fallback reports that limitation instead of reinterpreting them as commands. A successful launch means Windows created the process, not that Vivado finished loading or accepted the project.

Both editions read and write the same profile. The `[removed_installs]` section keeps removed installation records hidden on subsequent scans until the same path becomes available or the user undoes the removal. The saved default is cleared when its installation is removed.

## Localization

The launcher UI supports Simplified Chinese and English. Edit translations in `VivLauncher.App/Localization/`, using `en.xml` as the reference for message keys and format placeholders such as `{0}`. To add a language, add an XML catalog named for its culture code and register the code and native display name in `Localizer.Languages`. Both editions embed the catalogs in their EXEs. Unsupported system languages and missing translations fall back to English.

## Verification and isolation

`VivLauncher.Tests` covers the matcher, legacy configuration, path identity, unavailable installations, binding selection, launch arguments and the real batch fallback with a harmless capture script. Search parity fixtures are generated from the prototype's pinned `fzf@0.5.2` bundle:

```powershell
node tools/generate-fzf-fixtures.cjs
./build.ps1 -Action Test
powershell -NoProfile -Sta -ExecutionPolicy Bypass -File tools/generate-icon.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/smoke-ui.ps1 -AppPath ./artifacts/net10-win-x64/vivlauncher-net10-win-x64.exe
powershell -NoProfile -ExecutionPolicy Bypass -File tools/smoke-ui.ps1 -AppPath ./artifacts/net48-win-x64/vivlauncher-net48-win-x64.exe
powershell -NoProfile -ExecutionPolicy Bypass -File tools/smoke-runtime.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/smoke-runtime.ps1 -CoreOnly
```

The generator resets the JavaScript library per case because its shared scratch slab can retain stale backtracking data. The C# implementation allocates clean matrices for stable highlights. Node is needed only to regenerate the checked-in normalization table and fixtures. The icon tool derives the WPF vector and nine ICO sizes from the approved SVG.

The native UI smoke script stores screenshots and fixture profiles under `artifacts/qa/`. `VIVLAUNCHER_PROFILE` selects an isolated configuration directory. `VIVLAUNCHER_ISOLATED=1` limits discovery to paths recorded in that profile.

Before replacing the C++ launcher permanently, verify launching your real Vivado project and monitor transitions at 100%, 150% and 200% DPI. Process-capture fixtures do not validate Vivado licensing, project upgrades or a particular vendor installation.
