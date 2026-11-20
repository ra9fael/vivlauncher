# Project instructions

## GUI workflow

- Before refactoring or changing GUI layout, styling, or interactions, deliver an interactive HTML prototype or an SVG visual draft first. Prefer interactive HTML for behavior changes.
- Present the draft for review and incorporate the user's feedback before implementing the production GUI. An instruction to implement the overall plan does not skip this prototype review stage.
- Use representative project names, long paths, duplicate filenames, and meaningful loading, empty, and error states in prototypes.
- Clearly identify simulated actions. A prototype must not launch Vivado, scan real installations, modify application settings, or register file associations.
- The current migration targets C# / .NET 10 / WPF with the official Fluent theme and MVVM, on the `wpf` branch. Keep the original C++ application available until migration and validation are complete.
- Support Simplified Chinese and English in the redesigned interface.
- Publish a framework-dependent Windows x64 single EXE by default. Users install the required .NET Desktop Runtime themselves; rely on the standard .NET apphost missing-runtime prompt. Do not bundle the runtime unless the user explicitly requests it.

## Working conventions

- Preserve unrelated local files and changes.
- Keep prototype assets self-contained and runnable without a frontend build step.
- Verify GUI changes in a browser or the actual application, including keyboard operation, both languages, long paths, and light/dark appearance.
- Existing C++ code uses Unicode Win32 APIs, four-space indentation, and Allman braces. Keep existing source files ASCII unless their encoding is explicitly migrated.
