# Revision 03 review verification

Verified on Windows on 2026-11-20 with Python Playwright 1.65.2 and headless Microsoft Edge. The test opened local files directly, without a server or network assets. The exact browser version and successful groups are recorded in `previews/verification.json`.

All 21 check groups passed with no JavaScript page errors:

- SVG source and standalone review board use identical polygon data.
- Local icons and the shared prototype load; HTML IDs are unique.
- fzf nonconsecutive matching works; edition switching preserves query, selection and additional arguments.
- Review text translates while application inputs remain intact.
- Each of the four edition/theme combinations preserves unavailable-version warnings, disabled selection, mismatch confirmation, Escape handling and the IME Enter guard.
- The four fixed light/dark samples preserve their own background/text colors and respond to buttons and disclosure controls.
- System-theme changes update the preview while the system option is selected.
- Empty history, missing installs/files, scanning and simulated launch failure remain usable.
- No-result search and keyboard browsing work.
- English layout has no document overflow at 720px and 390px widths.
- The standalone SVG review board renders.
- All four edition/theme combinations keep unavailable projects highlighted while selected, unselected and filtered with fzf; simultaneous path and version warnings remain distinct.
- Chinese modern/light and English compatible/dark removal flows cover cancel focus return, confirmation, hiding the record from both lists, rescanning without resurrection, undo, retained project warnings, and recovery only at the original installation path.
- Path, binding and last-used-version failure labels share measured right alignment, font size, line height, color and background across both editions, languages and themes. Their explanations appear in the same details area. Both narrow viewport sizes remain within the document width.

Visual inspection covered `01-review-light-zh.png`, `04-control-comparison.png` and `06-icon-review.png`. The main application, review toolbar and control comparison have distinct purposes; the runtime selector is outside the proposed application window. The new icon uses the official arrow geometry and no taskbar indicator.

Other screenshots show the modern light app, compatible dark app with unavailable versions, and the narrow English preview. These are HTML review screenshots, not screenshots of new native builds.

Additional review images: `07-missing-project-and-version.png`, `08-remove-install-entry.png`, and `09-remove-install-confirmation.png`.

`10-unified-warning-labels.png` shows the three failure labels in the same right-hand status column. This replaces the earlier path-specific inline badge and separate path alert.

Production WPF, Framework 4.8 compatibility code, ICO generation and dual-EXE publishing have not been modified in this stage. Native controls, runtime compatibility, real process launching, file associations and multi-monitor DPI need verification after the visual review and implementation.

To run from the repository root with Playwright installed in `artifacts/review-tools`:

```powershell
$env:PYTHONPATH = Join-Path (Get-Location) 'artifacts/review-tools'
python prototypes/dual-runtime/verify.py
```
