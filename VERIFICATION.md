# WPF implementation verification

## 2.2.0 launch arguments and recent-project cleanup

On 2026-11-26, both Release targets built with zero warnings or errors and passed 1,380 tests each. The .NET Framework edition now builds the same `vvgl.exe` command line as the .NET 10 edition, quoting an argument only when it is empty or contains a space, tab or quote. New tests assert that a space-free batch path reaches the wrapper without quotes, and that every quoted form round-trips through `CommandLineToArgvW`.

Recent-project cleanup is covered at both layers. Core tests remove missing records through an atomic INI rewrite that keeps other sections, restores them as a snapshot, and rejects undo after an external change. Both published EXEs then passed 44 native UI checks, including the new flow:

- The **Clean unavailable paths (N)** entry appears only while a recent project is missing, confirms first, removes just the missing record, hides when nothing is left, and **Undo cleanup** restores it.
- Per-row **Remove record** opens its own confirmation and removes exactly one record.
- The renamed historical label reads **上次版本不可用** / **Last version unavailable** beside the path and binding warnings.

Screenshots are regenerated in `artifacts/qa/`, including `10-native-clean-confirmation.png`.

## Dual-edition build

On 2026-11-21, both Release targets built with zero warnings or errors. Each target passed 1,366 tests, including all 1,354 fzf parity cases and the installation removal, undo, and same-path recovery test.

| Edition | Standalone EXE size | Runtime verified |
| --- | ---: | --- |
| .NET 10 | 509,693 bytes | .NET 10 Desktop Runtime x64 |
| .NET Framework 4.8 | 487,936 bytes | .NET Framework 4.8.1, Release 534296 |

- Each EXE passed CLI and 31 native UI checks from an empty directory. The Framework EXE loaded with its bundled assemblies. The CLI checks also passed when both versions alternated reading and writing the same isolated installation profile.
- Native UI checks cover the three shared warning labels, removal and cancellation, keeping project bindings, scanning without reviving removed records, and undo. Screenshots under `artifacts/qa/` show Chinese/light and English/dark in both editions.
- Isolated apphost tests covered a missing .NET 10 runtime and a core-only installation. Both displayed the standard runtime download prompt before accessing the application profile.
- A Windows 10 environment with Framework 4.8, native folder-picker interaction, live IME and Explorer drag/drop, shell association rendering, real Vivado launch, and 150%/200% monitor transitions remain release checks.

## Original .NET 10 verification

Verified on Windows x64, 2026-11-20, using .NET SDK 10.0.403. The default release is now a framework-dependent Windows x64 single EXE, with the required .NET 10 Desktop Runtime installed separately by the user. The earlier self-contained build is retained as a historical output.

## Single-EXE distribution

- The original single-file release was **888,573 bytes** (about 868 KiB).
- The published EXE passes the CLI checks and all 21 native UI checks below, without companion DLL/configuration files. Embedded third-party notices are readable using `--licenses`.
- A copy of only the EXE was launched against an isolated .NET lookup root containing hostfxr but no shared frameworks. The standard native .NET dialog displayed **“You must install or update .NET to run this application.”**, **“Download it now”**, details identifying `Microsoft.NETCore.App` 10.0.0 x64, and a Microsoft download link.
- A second isolated lookup root supplied the core runtime but omitted the desktop runtime. The dialog correctly identified `Microsoft.WindowsDesktop.App` 10.0.0 x64 and its download link.
- Closing either dialog returned a nonzero exit code; application profile code did not run. The tests neither installed nor removed any system runtime and did not follow the download links. Reproduce using `tools/smoke-runtime.ps1` and `tools/smoke-runtime.ps1 -CoreOnly`.

## Automated checks

- Release build and publish succeed with no compiler warnings or errors. The application explicitly suppresses only the documented experimental WPF theme API diagnostic.
- **1,365 passing core tests**, including 1,354 fzf parity cases (pinned prototype library, scores and highlighted UTF-16 positions), randomized cases, normalization, extended query syntax, and large-input fallback.
- Compatibility tests cover legacy INI sections, FNV project keys, version-only defaults/bindings, unbinding, Unicode and spaces, ANSI-to-Unicode backup, unavailable records, recent-history ordering/capacity and surfaced write failures.
- Launch tests validate wrapper argument separation and run a harmless batch capture fixture with spaces, `&`, `!`, and literal `%PATH%` in paths/arguments. Missing projects produce a log and no process.
- CLI smoke checks pass for help aliases, empty installation discovery, adding a directory containing spaces, installation listing, redirected stdout/stderr, and nonzero errors.

## Native UI checks

The published framework-dependent single executable passes **21 checks** using Windows UI Automation and an isolated profile:

- Main window opens; a missing bound installation blocks launch.
- fzf nonconsecutive and extended exact searches work, including duplicate filenames; empty results disable launch.
- Pasted Unicode absolute `.xpr` paths bypass fuzzy syntax; missing paths remain blocked.
- Binding a pasted project updates its corresponding history row; unbinding immediately refreshes the selection.
- Missing bound and historical versions remain visible and disabled in the version picker.
- Explicit replacements enable launch, and differing bindings require confirmation.
- Installation management opens; switching to English updates both dialogs and the main window immediately.
- Down-arrow selection from search and Enter-to-launch work.
- A removed installation produces a visible launch failure without closing the launcher.
- Successful creation of the harmless fixture process closes the launcher and updates recent history.
- Language and dark-theme preferences persist.

Screenshots are generated in `artifacts/qa/`: Chinese/light, fzf highlights, installation management, settings, English/dark, 720 × 480 minimum window, and launch failure. The default and minimum-size layouts were visually reviewed; lists and details scroll at the minimum size.

## Remaining real-environment verification

- No real Vivado process or real project was opened by these tests. Validate the installed vendor wrapper, licensing and project loading before replacing the C++ launcher permanently.
- 150% and 200% monitor DPI transitions have not been exercised. The application declares PerMonitorV2, uses WPF layout in DIP, and supports long-path wrapping and scrolling.
- IME composition is guarded in the event handlers; a live Chinese IME conversion and real Explorer drag/drop have not been automated.
- File associations are implemented but were not applied to the user's registry during verification.

The original C++ source is preserved on `legacy-win32`. Personal `.claude` files are not part of this repository.

## Repository migration verification

- The WPF Release build, 1,365 tests, and single-file publish passed from the new repository root. The fzf generator reproduced all 1,354 cases and 622 normalization mappings byte for byte.
- A copy of the published EXE in an empty directory passed the CLI smoke checks, including embedded license output, and all 21 native UI checks.
- Both offline prototype suites passed from the migrated files, including all 21 revision 03 groups.
- The original Win32 Release x64 build and logic tests passed from a separate `legacy-win32` checkout. Git LFS verified the original documentation screenshot.
