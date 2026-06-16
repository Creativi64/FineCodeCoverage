# Markdig.Wpf (vendored)

Vendored copy of **Markdig.Wpf** — renders [Markdig](https://github.com/xoofx/markdig) Markdown to a
WPF `FlowDocument`. FCC's `MarkdigExtended` subclasses these renderers (and calls
`Markdig.Wpf.Markdown.ToFlowDocument`) to render the read-me / options pages.

- Upstream: https://github.com/Kryptos-FR/markdig.wpf
- Author: Nicolas Musset — License: MIT
- Vendored from package version 0.5.0.1 (the last release — the package is abandoned). Only the
  `src/Markdig.Wpf` library project was copied (not the console-app sample).

## Local changes
- Retargeted from `net452;netcoreapp3.1;net5.0-windows` to `net462;netcoreapp3.1;net5.0-windows` to
  match the only consumer, `MarkdigExtended`.
- References the same `Markdig` version the rest of the solution uses (via central package management),
  rather than its original `Markdig 0.22.0` pin. It compiles and the existing tests pass against it.
- Dropped packaging metadata, SourceLink and the build-time analyzer package.

## License (MIT)

Copyright © Nicolas Musset 2016-2021

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
associated documentation files (the "Software"), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN
NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
