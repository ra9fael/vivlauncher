# Third-party components

`VivLauncher.Core/FzfMatcher.cs` is a C# adaptation of the forward fzf v2 matcher and extended query parser in **fzf-for-js 0.5.2**, by Ajit. The original source is https://github.com/ajitid/fzf-for-js. Its BSD-3-Clause license is embedded in the EXE (source copy: `prototypes/wpf-launcher/vendor/fzf-LICENSE.txt`). Scoring, consecutive/boundary bonuses, gap penalties and backtracking follow that implementation. Large matrices use the corresponding greedy fallback.

**CommunityToolkit.Mvvm 8.4.0** is distributed under the MIT license: https://github.com/CommunityToolkit/dotnet/blob/main/License.md.

Run `vivlauncher.exe --licenses` to display this notice and the full bundled license texts. The release is framework-dependent: users install the Microsoft .NET Desktop Runtime separately.
