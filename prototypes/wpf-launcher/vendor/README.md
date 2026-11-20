# FZF for JavaScript

- Package: `fzf@0.5.2`
- Upstream: https://github.com/ajitid/fzf-for-js
- Registry package: https://www.npmjs.com/package/fzf/v/0.5.2
- Local file: `fzf-0.5.2.umd.js`, the unmodified published `dist/fzf.umd.js` browser bundle.
- License: BSD-3-Clause, reproduced in `fzf-LICENSE.txt` and retained in the upstream bundle header.
- Published archive integrity: `sha512-Tt4kuxLXFKHy8KT40zwsUPUkg1CrsgY25FxA2U/j/0WgEDCk3ddc/zLTCCcbSHX9FcKtLuVaDGtGE/STWC+j3Q==`.

This package is a JavaScript port of fzf's matching algorithm, not an invocation of the fzf executable. The prototype explicitly selects the v2 algorithm, extended matching, smart case, score ordering, and stable original-order ties. The bundle is stored locally so the draft remains usable directly from `index.html`, offline and without a build step. No executable package scripts were run when obtaining it.
