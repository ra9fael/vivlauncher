# Vivado Launcher interaction prototype

Open `index.html` directly in a current Edge or Chrome browser. No server, package installation, network connection, or build step is required. The HTML, CSS, JavaScript, and SVG icons are local files.

The native implementation of this approved prototype is documented in the [repository README](../../README.md). The original C++ application is maintained on `legacy-win32`. Future GUI changes require a reviewed HTML or SVG draft first.

## Try the workflow

1. Search using the locally bundled fzf v2 JavaScript port. Type `pjt1` for non-consecutive matches or `zu15 xpr` for multiple terms. Matches in project names and paths are highlighted and ranked by fzf score; ties preserve recent-project order. Empty search restores history order. Duplicate filenames have distinct parent directories and a full path in the details pane.
2. Select a project, choose a Vivado installation, and bind or unbind the version. Installation management lets you change the default and add a simulated installation directory.
3. Expand launch options, enter arguments, and select **Open project**. Changing away from a bound version requires confirmation. The simulated launch leaves the prototype open for continued review.
4. Switch between Simplified Chinese and English, and light, dark, or system appearance. These preferences persist in this browser; in-progress form input is preserved.
5. Use the scenario selector below the application to inspect first launch, no installations, missing bound versions, unavailable projects, background scanning, and launch failures. Select **Open project** in the launch-failure scenario to see the failure and log.
6. Select **Reset** to restore sample projects, installations, bindings, and launch options. Language and appearance preferences are retained.

Search uses fzf's smart-case rule (lowercase queries ignore case; uppercase queries are case-sensitive) and extended syntax, including `'text` for an exact term, `!text` for exclusion, and `term1 | term2` for alternatives. A complete Windows `.xpr` path still selects that path directly instead of treating it as a query. The first ranked match is selected when the query changes. Confirming an IME conversion does not launch a project.

In **Missing bound version**, Vivado 2023.2 remains visible as unavailable in installation management and as a disabled, warning-labelled version option. The recent-project list highlights both missing bindings and unavailable last-used versions with warning icons and explicit labels. A missing binding blocks launching until an available replacement is chosen; an unavailable last-used version can use an available default. Invalid installation records cannot be selected or set as default, and availability is checked by installation identity for bindings, even when another installation has the same version number.

Keyboard: `Ctrl+F` focuses search; `Ctrl+O` opens the demo file picker; arrow keys navigate the project list; Enter launches from search or the list; Escape dismisses a dialog. Inputs such as additional arguments retain their normal keyboard behavior.

## Simulation boundaries

- No Vivado process is created, no installation directories are scanned, no application configuration is written, and no Windows association is registered.
- Browse opens a demo picker. Enter a complete Windows `.xpr` path or use the sample. Pasted paths are treated as available demo paths; they are not checked on disk.
- Dropping a single `.xpr` reads only its filename, never its contents. Browsers do not expose its real full path, so a clearly identified `D:\PreviewDrops\...` placeholder is offered for confirmation.
- Copy path uses the browser clipboard only when explicitly clicked.
- A scan resolves after approximately 1.5 seconds. A launch resolves after approximately 1.1 seconds. Scenario changes cancel pending simulations.
- Project data and installation changes are kept in page memory. Only language and appearance preferences are saved under `vivlauncher-prototype-preferences` in browser local storage, where permitted.

## Production direction

The approved target is C# / .NET 10 / WPF with the official Fluent theme and CommunityToolkit.Mvvm. The HTML is a behavior and visual specification, not a WebView frontend for the production app. Desktop window resizing, Windows file pickers, live installation discovery, CLI compatibility, legacy configuration, and actual process creation belong to the subsequent WPF stage.

Default desktop layout is a search strip above a project list and project details, with status and the primary action below. At narrow browser widths the prototype stacks the panels to stay reviewable. The WPF version will target a minimum desktop size of approximately 720 × 480 DIP.

## Verification

`verify.cjs` exercises the prototype in headless Chromium using Playwright. It covers search and keyboard navigation, language switching without input loss, modal focus and dismissal, binding conflicts, defaults, paths with spaces and Chinese characters, missing installations, background scan cancellation, file-drop confirmation, launch failures, clearing history, double-click launching, narrow layouts, and preference persistence. It also fails on browser runtime errors.

Run `node verify.cjs` from this directory with Playwright available. If using an existing installation, set `PLAYWRIGHT_MODULE` to its absolute module path and optionally set `PLAYWRIGHT_CHROMIUM_EXECUTABLE` to an existing Chromium executable. No dependency is needed merely to view the prototype.

The run produces these review images in `previews/`:

- `01-light-zh.png`: Chinese, light appearance, main workflow.
- `02-dark-en.png`: English, dark appearance, expanded launch options.
- `03-installations.png`: installation management and default selection.
- `04-launch-error.png`: failed launch and recovery actions.
- `05-narrow.png`: narrow browser layout.
- `06-fzf-search.png`: non-consecutive fzf matching with highlighted characters.
- `07-unavailable-versions.png`: unavailable bound and last-used versions in the project list.
- `08-unavailable-install.png`: retained unavailable installation in management.
- `09-unavailable-dark-en.png`: unavailable versions in English and dark appearance.

The bundled matching library and its attribution are documented in `vendor/README.md`.

## Production scope represented by this prototype

- Translate the reviewed design into native WPF controls and resources; introduce Core, App, and Tests projects.
- Port installation discovery, legacy settings and project binding identity, CLI output redirection, process launch arguments and working directory, and Windows integration.
- Keep scanning and path checks off the UI thread. Preserve unavailable history entries and show the reason for the selected installation.
- Validate 100%, 150%, and 200% DPI, both languages, both themes, actual Vivado launching, and a self-contained Windows x64 release.
- Keep the original C++ project and unrelated `vivlauncher/.claude/` files unchanged during this review stage.
