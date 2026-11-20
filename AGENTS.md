# Project instructions

## GUI workflow

- Before refactoring or changing GUI layout, styling, or interactions, deliver an interactive HTML prototype or an SVG visual draft first. Prefer interactive HTML for behavior changes.
- Present the draft for review and incorporate the user's feedback before implementing the production GUI. An instruction to implement the overall plan does not skip this prototype review stage.
- Use representative project names, long paths, duplicate filenames, and meaningful loading, empty, and error states in prototypes.
- Clearly identify simulated actions. A prototype must not launch Vivado, scan real installations, modify application settings, or register file associations.
- The `main` branch contains the C# / .NET 10 / WPF application with the official Fluent theme and MVVM. The original C++ application is preserved on `legacy-win32`; do not merge WPF changes into that branch.
- Planned .NET 10 / .NET Framework 4.8 support shares source and multi-target builds on `main`, not separate runtime branches. Framework 4.8 and the revision 03 GUI remain planned until explicitly implemented and validated.
- Support Simplified Chinese and English in the redesigned interface.
- Publish a framework-dependent Windows x64 single EXE by default. Users install the required .NET Desktop Runtime themselves; rely on the standard .NET apphost missing-runtime prompt. Do not bundle the runtime unless the user explicitly requests it.

## Working conventions

- Preserve unrelated local files and changes.
- Keep prototype assets self-contained and runnable without a frontend build step.
- Verify GUI changes in a browser or the actual application, including keyboard operation, both languages, long paths, and light/dark appearance.
- App, Core and Tests projects, build.ps1 and tools are rooted in this repository. Keep all production resources independent of the old vivtool2 checkout.
