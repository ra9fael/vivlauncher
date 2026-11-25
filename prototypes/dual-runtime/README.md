# Icon and dual-runtime design review — revision 03

Open `index.html` directly in current Edge or Chrome. No server, build, package installation or network connection is needed. Keep the sibling `../wpf-launcher/` directory: this review reuses its approved interaction logic, styles and pinned fzf 0.5.2 bundle.

This is the first review stage of the .NET 10 / .NET Framework 4.8 plan. It does not modify the production WPF app, the C++ launcher or release artifacts. Production GUI integration requires approval of this draft, as specified in the root `AGENTS.md` and the implementation plan.

## Review

- `icon-blue.svg`: editable 48-unit icon source. White AMD arrow, #0067C0 square, 8-unit inset, no rounding or effects.
- `icon-review.svg`: self-contained review board with light/dark backgrounds, header placement, taskbar illustration, and 16/24/32/48-pixel samples. View at 100% for actual-size assessment.
- `index.html`: working launcher prototype, edition selector, language/theme controls, scenarios and four fixed control samples (both editions, both color schemes).

The arrow's two polygon point lists are taken directly from AMD's official header SVG: https://www.amd.com/content/dam/code/images/header/amd-header-logo.svg. Only the arrow is used; the AMD letters are omitted. The lower taskbar indicator is not part of the icon. The modified blue icon is a Vivado Launcher review asset, not an unmodified vendor icon. No AI image generation or bitmap tracing is involved.

## What to try

1. Switch between .NET 10 and .NET Framework 4.8 above the app. Input, search results, selection and launch options remain intact; only review styles change.
2. Search `pjt1` or `zu15 xpr` using the same fzf v2 algorithm as revision 02. Try duplicate filenames and long paths.
3. Select **Missing bound version**. The invalid version stays visible, launching is blocked until an available replacement is chosen, and a version mismatch requires confirmation.
4. Switch Chinese/English and light/dark/system appearance. The four control samples below intentionally keep their own light or dark scheme.
5. Use Tab, Ctrl+F, Ctrl+O, arrow keys, Enter and Escape. Test focus, disabled buttons, expansion, installation management and launch-failure recovery.
6. Select **Compare unavailable states** to see path, binding and last-used-version failures together. All three use the same amber row treatment and right-aligned status block under the version number: `路径失效`, `绑定失效`, `版本失效` (English: Path / Binding / Version unavailable). Selected warning rows keep amber backgrounds with blue selection borders. Explanations share one area below the version picker. **Project and version unavailable** stacks both labels in that same status block. Path failures always disable launching; version-history failures may use another available installation.
7. Select **Missing bound version**, open **Installations**, and use **Remove record** on Vivado 2023.2. The confirmation names the version/path and counts bound and historical projects. Cancel is focused initially. Confirming removes the record from both installation management and the version picker; recent projects and their bindings stay intact, so missing-binding warnings remain. Use **Undo last removal** to restore the unavailable record.
8. Rescanning does not recreate a removed record merely because a project references it. Adding a different path with the same version does not revive an existing binding. In this simulation, adding the original path marks that installation available again and restores its original identity. No disk is inspected.
9. Select **Clean up stale paths** in the scenario list. The Recent projects heading exposes **Clean unavailable paths (3)** in both editions, and every unavailable row repeats the same amber **Path unavailable** label plus its own **Remove record** action. Confirming the bulk entry removes the three missing projects; the footer offers **Undo cleanup**, which restores the latest batch. Settings → Diagnostics repeats the bulk entry. Bindings and installation records stay unchanged, and no file is read or written.

## Removal behavior for subsequent native implementation

Removal acts on an installation path, not a version number, and does not uninstall Vivado or delete projects. Persist removal markers with the existing configuration and suppress stale installation records reconstructed from custom paths, discovery history or project bindings. Keep the underlying project binding and last-used version data. A removed default is cleared. Undo restores the record (as unavailable), without silently assigning a default. A live scan that actually finds the same path available, or an explicitly added verified path, clears its removal marker. A different installation of the same version never silently satisfies a path binding. The prototype keeps markers and undo history only in page memory; scenario reset restores the fixtures.

All scanning, launching, directory browsing and association actions are simulated. No Vivado process is started, no real installations are scanned, no application settings are written and no file association is changed. Project data stays in page memory; the existing prototype only persists language/appearance to its browser storage. Copying a path changes the clipboard only if explicitly clicked.

The edition selector belongs to the review toolbar, not the production app. HTML illustrates Fluent and the proposed compatible control styles; it cannot certify native WPF rendering, Windows folder pickers, real .NET Framework 4.8 execution or DPI behavior. Those checks belong to the implementation and release stages after approval.

## Visual direction

Keep the approved Windows utility layout and Segoe UI / Microsoft YaHei typography. Use #0067C0 for the brand and primary actions, #F7F7F7/#FFFFFF for light surfaces, #202020/#2B2B2B for dark surfaces, and amber for unavailable versions. The icon is the main visual change. Framework 4.8 uses simpler 3-pixel control corners, full-edge input focus and flat buttons while retaining the same hierarchy and spacing.

## Verification

`verify.py` uses Python Playwright to verify both edition styles, real fzf results, retained state, disabled versions, modal keyboard behavior, bilingual themes, system theme updates and narrow layouts. It also compares the SVG polygon data and saves browser screenshots under `previews/`.

Run with a Python installation containing Playwright:

```powershell
python prototypes/dual-runtime/verify.py
```

This stage deliberately does not create ICO files or replace WPF resources. Once reviewed, the production icon generator will derive ICO and WPF vector resources from `icon-blue.svg`, and compatibility implementation will follow the accepted dual-target plan.
