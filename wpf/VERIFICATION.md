# WPF implementation verification

Verified on Windows x64, 2026-11-20, using .NET SDK 10.0.403. The default release is now a framework-dependent Windows x64 single EXE, with the required .NET 10 Desktop Runtime installed separately by the user. The earlier self-contained build is retained as a historical output.

## Single-EXE distribution

- `wpf/artifacts/win-x64-exe/` contains exactly one file: `vivlauncher.exe`, **888,573 bytes** (about 868 KiB).
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

Screenshots are generated in `wpf/artifacts/qa/`: Chinese/light, fzf highlights, installation management, settings, English/dark, 720 × 480 minimum window, and launch failure. The default and minimum-size layouts were visually reviewed; lists and details scroll at the minimum size.

## Remaining real-environment verification

- No real Vivado process or real project was opened by these tests. Validate the installed vendor wrapper, licensing and project loading before replacing the C++ launcher permanently.
- 150% and 200% monitor DPI transitions have not been exercised. The application declares PerMonitorV2, uses WPF layout in DIP, and supports long-path wrapping and scrolling.
- IME composition is guarded in the event handlers; a live Chinese IME conversion and real Explorer drag/drop have not been automated.
- File associations are implemented but were not applied to the user's registry during verification.

The original C++ source and unrelated `.claude` files are unchanged.
