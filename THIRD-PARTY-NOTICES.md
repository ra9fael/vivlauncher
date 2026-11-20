# Third-party components

`VivLauncher.Core/FzfMatcher.cs` is a C# adaptation of the forward fzf v2 matcher and extended query parser in **fzf-for-js 0.5.2**, by Ajit. The original source is https://github.com/ajitid/fzf-for-js. Its BSD-3-Clause license is embedded in the EXE (source copy: `prototypes/wpf-launcher/vendor/fzf-LICENSE.txt`). Scoring, consecutive/boundary bonuses, gap penalties and backtracking follow that implementation. Large matrices use the corresponding greedy fallback.

**CommunityToolkit.Mvvm 8.4.0** is distributed under the MIT license: https://github.com/CommunityToolkit/dotnet/blob/main/License.md.

The .NET Framework edition embeds application dependencies using **Costura.Fody 6.0.0** and **Fody 6.8.2**, MIT-licensed tools from the Fody project. Costura's assembly loader is included in the Framework executable. https://github.com/Fody/Costura

**.NET compatibility libraries** used by the Framework edition include Microsoft.Bcl.AsyncInterfaces 8.0.0, System.ComponentModel.Annotations 5.0.0, System.Memory 4.6.0, System.Buffers 4.6.0, System.Numerics.Vectors 4.6.0, System.Runtime.CompilerServices.Unsafe 6.1.0, and System.Threading.Tasks.Extensions 4.5.4. They are distributed under the .NET MIT license. The corresponding license and third-party notices are embedded in that edition. https://github.com/dotnet/runtime

The blue icon uses arrow geometry from the AMD header SVG. The AMD mark belongs to its owner. https://www.amd.com/content/dam/code/images/header/amd-header-logo.svg

Run the executable with `--licenses` to display the bundled notices and license texts.
