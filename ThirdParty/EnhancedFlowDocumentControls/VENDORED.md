# EnhancedFlowDocumentControls (vendored)

Vendored copy of the **EnhancedFlowDocumentControls** library — an enhanced `FlowDocumentReader` /
`FlowDocumentScrollViewer` / `FlowDocumentPageViewer` with a customisable find toolbar, used by FCC's
read-me tool window.

- Upstream: https://github.com/tonyhallett/EnhancedFlowDocumentControls
- Author: Tony Hallett — License: MIT
- Vendored from package version 1.0.0 (only the `EnhancedFlowDocumentControls` library project; the
  Demo/Tests/UITests/VideoRecorder projects were not copied).

## Local changes
- Retargeted from `net461;net5.0-windows` to `net472` to match the extension.
- Removed the `Fasterflect` NuGet dependency: `ViewModel/FindToolBarReflector.cs` was rewritten to use
  plain `System.Reflection` (functionally identical; these lookups run once per find-toolbar).
- Dropped packaging metadata, the `NugetRepoReadme` build dependency and the `InternalsVisibleTo`
  attributes (only its own test project needed them).

## License (MIT)

Copyright © 2025 Tony Hallett

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
