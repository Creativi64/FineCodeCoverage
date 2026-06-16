# Bundled binary DLLs — provenance and resolution

This file is the running record of every loose, checked-in binary DLL that `FineCodeCoverage2022`
ever carried, and how each was resolved.  **The repo now ships zero checked-in binary DLLs.**
Two opaque `<Reference><HintPath>` binaries (`WpfExtended.dll`, `themes.dll`) were identified from
their PE metadata and removed; the four `markdig-redirects\` BCL assemblies were moved to NuGet; and
`Microsoft.VisualStudio.TestWindow.Interfaces.dll` is now referenced straight out of the installed VS
(which was unblocked by narrowing the VSIX to a single VS major).  Details below.

It also records the single-author / abandoned NuGet **packages** that were vendored in-tree (so they're
no longer hard NuGet dependencies) — see the "Vendored third-party packages" section at the bottom.

## WpfExtended.dll — VENDORED (then removed)

- Was: `WpfExtended, Version=0.7.7.0` — the **WPF Pixel Shader Effects Library** (the old CodePlex
  "WPFSLFx"), MS-PL, customised by FCC with `FilterColor` + `IsEnabled` on the `MonochromeAlphaSwap`
  effect. 34 `ShaderEffect` classes shipped for the single effect FCC uses.
- Used by: `SharedProject/CoverageUI/Report/ToolWindow/ReportToolWindowControl.xaml` —
  `<wpfExtended:MonochromeAlphaSwap>` on the report tree icons (monochrome recolouring).
- Resolution: vendored just that one effect into `WpfHelpers/ShaderEffects/MonochromeAlphaSwap.cs`
  with its compiled shader bytecode at `WpfHelpers/ShaderEffects/MonochromeAlphaSwap.ps` (embedded
  manifest resource `WpfHelpers.MonochromeAlphaSwap.ps`). The shader bytes are byte-identical to the
  original DLL's resource, so rendering behaviour is preserved. The XAML xmlns now points to
  `clr-namespace:WpfHelpers.ShaderEffects;assembly=WpfHelpers`. WpfExtended.dll deleted.

## themes.dll — REMOVED (dead reference)

- Was: `ThemeServiceLibrary, Version=0.0.0.0` — `Themes.ThemeService`/`ThemeResourceKey`/
  `ThemeResourceKeyType` (~1.9 MB, mostly an embedded VS theme colour table).
- Used by: nothing. No C#/XAML referenced the `Themes` namespace or these types; no
  `xmlns ... assembly=themes`, no `/themes;component/...` pack URI, and no compiled assembly carried a
  metadata reference to `ThemeServiceLibrary`. FCC has its own, separate `ThemeService`
  (`SharedProject/Wpf/DesignTimeResources/Colors/ThemeService.cs`).
- Resolution: `<Reference Include="themes">` removed and the 1.9 MB binary deleted. The solution and
  VSIX build clean and all tests pass without it. (If any theming regression ever surfaces in the
  report tool window / themed dialogs, this removal is the first thing to check.)

## markdig-redirects\*.dll — REMOVED (now restored from NuGet)

- Was: four BCL assemblies hand-copied into `FineCodeCoverage2022\markdig-redirects\` and shipped via
  `<Content IncludeInVSIX=true>` so Markdig (and friends) could resolve them at runtime inside the VS
  process — `System.Buffers`, `System.Memory`, `System.Numerics.Vectors`,
  `System.Runtime.CompilerServices.Unsafe`. The checked-in copies were the 4.6.0 / 6.1.0 wave, which
  was actually *older* than what the rest of the dependency graph already resolves.
- Resolution: declared as direct `PackageReference`s on the VSIX project (versions pinned in
  `Directory.Packages.props` to the same versions the graph already unifies on —
  Buffers 4.6.1, Memory 4.6.3, Numerics.Vectors 4.6.1, Unsafe 6.1.2). The VSSDK packs copy-local
  references into the `.vsix` (`CopyLocalLockFileAssemblies` + `IncludeCopyLocalReferencesInVSIXContainer`),
  so all four ship at the VSIX **root** — verified present in `bin\Release\FineCodeCoverage2022.vsix`.
  The loose folder is deleted. The four are now resolved at runtime by the existing
  `[ProvideBindingPath]` (extension root) on `FCCPackage`, so the now-redundant
  `[ProvideBindingPath(SubPath = "markdig-redirects")]` was removed. Build is clean and all 808 tests
  pass (807 + 1 skipped, 0 failed).

## Microsoft.VisualStudio.TestWindow.Interfaces.dll — REMOVED (now referenced from the installed VS)

- Was: `Microsoft.VisualStudio.TestWindow.Interfaces` (`PublicKeyToken=b03f5f7f11d50a3a`), a copy
  hand-pasted into `FineCodeCoverage2022\dlls\` and referenced via `<Reference><HintPath>` from both
  `FineCodeCoverage2022.csproj` and `FineCodeCoverageTests.csproj`. Provides the
  `Microsoft.VisualStudio.TestWindow.Extensibility` types.
- Used by: the test-explorer / MS Code Coverage integration — `TestContainerDiscoverer`
  (`[Export(typeof(ITestContainerDiscoverer))]`), `MsCodeCoverageRunSettingsService`
  (`[Export(typeof(IRunSettingsService))]`), and imports of `IOperation` / `IOperationState` etc.
- Two non-options (recorded so nobody re-tries them):
  - **NuGet** — the `Microsoft.VisualStudio.TestWindow.Interfaces` package is frozen at **v11.0.61030**;
    that's an API downgrade.
  - **Vendor a local copy of the interfaces** (what was done for WpfExtended) — these are **MEF
    contracts exchanged with the VS host**, so a local copy has a different assembly identity; MEF
    composition stops matching and the integration silently breaks. (WpfExtended was safe to vendor
    because a `ShaderEffect` has no cross-assembly type-identity contract.)
- Resolution: reference the real assembly from the installed VS via
  `$(VsInstallRoot)\Common7\IDE\CommonExtensions\Microsoft\TestWindow\Microsoft.VisualStudio.TestWindow.Interfaces.dll`
  (`SpecificVersion=False`, `Private=false` in the VSIX project so the host's copy is used at runtime).
  This was previously impossible because the build machine's VS (v18 / VS2026) is a *different major*
  than the host the VSIX targeted (v17 / VS2022), so a v18 compile-time ref wouldn't bind on a v17
  host. It became safe once the VSIX was **narrowed to a single VS major**: `source.extension.vsixmanifest`
  `InstallationTarget` is now `[18.0,19.0)`, so the build machine's VS and the runtime host are always
  the same major. The `dlls\` folder is deleted; the repo now carries no binary DLLs.
- Known wrinkle: the v18 TestWindow assembly drags in v18 `Microsoft.VisualStudio.GraphModel`, which
  RAR flags against the v17.14 `Microsoft.VisualStudio.SDK` packages (warning **MSB3277**, auto-resolved
  to v18). It can't be "fixed" by bumping the SDK — **there is no v18 VS SDK** (NuGet tops out at 17.14;
  Microsoft ships only a 17.x SDK and VS2026 runs 17-SDK extensions via back-compat). It's harmless (the
  v18 host provides and unifies a single GraphModel at runtime), so it's downgraded to a message via
  `<MSBuildWarningsAsMessages>MSB3277</MSBuildWarningsAsMessages>` in both csprojs (documented there).
  Build is green with no warnings and all 808 tests pass (807 + 1 skipped, 0 failed).

---

# Vendored third-party packages

Single-author / abandoned NuGet packages that were brought in-tree so they're no longer hard NuGet
dependencies (the user's call). Each is MIT (or the FCC author's own micro-lib) and its DLL already
shipped in the VSIX, so vendoring the source changes nothing about what is distributed.

## ReflectObject — vendored as source

- Was: `ReflectObject` 1.0.2 (Tony Hallett, https://github.com/tonyhallett/ReflectObject), a reflection
  wrapper base used by the TestExplorer `InternalTypes` wrappers.
- Resolution: source inlined at `SharedProject/Collection/TestExplorer/InternalTypes/ReflectObjectVendored.cs`
  (namespace kept as `ReflectObject` so the wrappers are unchanged; marked `// <auto-generated />` so
  FCC's analyzers leave it alone). `PackageReference` + the CPM entry removed.

## EnhancedFlowDocumentControls — vendored, then REMOVED (2026-06-16)

- Was: `EnhancedFlowDocumentControls` 1.0.0 (Tony Hallett, MIT) — the enhanced FlowDocument reader +
  find toolbar behind FCC's read-me tool window. Vendored at `ThirdParty/EnhancedFlowDocumentControls/`.
- Now: **deleted** along with the in-VS read-me viewer (its only consumer). Project, slnx entry, and the
  `FineCodeCoverage2022` `ProjectReference` are gone. The whole `ThirdParty/` folder is now empty/removed.

## Markdig.Wpf / MarkdigExtended / Markdig — vendored/used, then REMOVED (2026-06-16)

- Was: `Markdig.Wpf` 0.5.0.1 (Nicolas Musset, MIT, abandoned) rendered Markdig Markdown to a WPF
  FlowDocument; `MarkdigExtended` subclassed its renderers; both sat behind the in-VS read-me viewer.
- Now: **deleted** with the viewer. `ThirdParty/Markdig.Wpf/` + the `MarkdigExtended` project are gone
  (slnx + all `ProjectReference`s removed), and the `Markdig` NuGet `PackageVersion` was dropped from
  `Directory.Packages.props`. The GitHub-README generation (`GithubReadmeCreator` + the kept
  `FineCodeCoverage.Readme` options-table data classes) never used Markdig — it builds the table string
  with its own `PipeTable`. See [[removed-feedback-readme-ui]].
- The four BCL `PackageReference`s (System.Buffers/Memory/Numerics.Vectors/Runtime.CompilerServices.Unsafe)
  STAY — they ship for Roslyn / VS SDK (System.IO.Pipelines / System.Threading.Tasks.Extensions), not Markdig.

Current state: full solution builds with 0 errors and 805/806 tests pass (1 skipped, 0 failed).
