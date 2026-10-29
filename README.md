# Fine Code Coverage

## About this fork

A Visual Studio 2026 extension that shows test coverage in the editor and a native WPF report window.
This fork is based on [tonyhallett/FineCodeCoverage](https://github.com/tonyhallett/FineCodeCoverage).

- Microsoft Code Coverage is the only coverage engine. OpenCover, Coverlet and the `RunMsCodeCoverage` option have been removed.
- Classic VSTest and Microsoft.Testing.Platform (MTP) projects can contribute to the same report.
- VSTest runsettings are injected in memory. FCC does not add or remove `RunSettingsFilePath` in your project files.
- Report generation runs in a separate process to isolate its dependencies from Visual Studio.
- Dependencies use central package management. The legacy feedback and embedded README UI have been removed.

The upstream Marketplace packages are different builds; build this fork to use these changes.

## Requirements and quick start

1. Use Visual Studio 2026 (version 18). This extension does not target Visual Studio 2019 or 2022.
2. Install the VSIX built from this fork and restart Visual Studio.
3. Open a solution with test projects. Classic VSTest projects need their framework's NuGet test adapter.
4. Run tests from Test Explorer and open the Fine Code Coverage tool window.

FCC uses Microsoft Code Coverage whenever coverage is enabled. Use `Enabled` in the FCC options or project configuration to control collection.
Progress and diagnostic messages appear in the Fine Code Coverage Output pane.

## Test execution and coverage

### Classic VSTest projects

FCC supplies the Microsoft Code Coverage collector and injects the necessary runsettings through Visual Studio's runsettings service.
Coverage comes from the tests selected in Test Explorer; FCC does not rerun those tests.
You can supply your own runsettings or customize FCC's generated settings.

### Microsoft.Testing.Platform projects

MTP detection supports projects using MSTest, NUnit, xUnit and TUnit.

Automatic collection is controlled by `CollectTestingPlatformCoverageAfterTestRun` (enabled by default).
To collect coverage during the original Test Explorer run, the test project needs:

- Microsoft.Testing.Platform 2.3.0 or later.
- The `Microsoft.Testing.Extensions.CodeCoverage` package.

Before execution, FCC adds coverage options to the test host's `.testconfig.json` in the build output directory.
It preserves unrelated configuration and existing coverage output/settings choices, while enabling coverage.
FCC then reads the coverage files produced by the original run. A project that produces no coverage file is skipped.
After collection, cancellation, preparation failure, or normal Visual Studio shutdown, FCC restores its injected options.
If the file was otherwise unchanged, its original contents are restored exactly (or FCC's newly created file is removed).
If a user or build edited it during the run, FCC removes only its own unchanged injected values and preserves those later edits.
An abrupt process termination cannot perform this cleanup; rebuilding the test project refreshes its output configuration.

**Automatic collection never reruns MTP test hosts.** If injection is unavailable, such as with an older platform,
a missing coverage extension or unreadable configuration, FCC skips automatic coverage for that project and explains why in the Output pane.
This preserves the Test Explorer selection: FCC cannot reconstruct its selected-test filter for a separate host invocation.

The manual MTP coverage command on the FCC tool window builds and runs **all enabled MTP projects**.
It uses the project's coverage extension when available, or the bundled `dotnet-coverage` tool otherwise.
This command is independent of the Test Explorer selection; it can also be used for projects that cannot collect coverage automatically.

`FCCTestingPlatformCommandLineArguments`, or `TestingPlatformCommandLineArguments` when the FCC-specific property is absent,
can supply additional host arguments and coverage output/settings choices.
Manual runs use these additional arguments. Automatic collection uses the coverage choices during configuration injection;
Test Explorer remains responsible for selecting and executing tests.

`DisableTestingPlatformServer` is an optional compatibility setting for projects that can run through VSTest instead.
Its default is false.

### Mixed solutions and cancellation

FCC combines available VSTest and MTP coverage into one report. A skipped MTP project does not prevent reporting coverage from other projects.
The report has an expandable collection status showing how many test projects produced coverage and why others were skipped.
MTP counts include enabled projects considered for collection; a project outside the Test Explorer selection can appear as skipped.
VSTest attachments do not identify each contributing test project, so those projects are explicitly shown as unverified rather than assumed covered.
When no coverage is produced, the previous report tree is cleared and the status explains what was missing.

Cancelling report generation terminates its external generator process and removes its temporary response file.
Cancellation is also checked before and after the in-process parsing and report collation steps.
Each collection/report run owns its cancellation source. A superseded run cannot dispose the next run's source,
and report generation is serialized to prevent simultaneous writes to the report directory.

## Report and editor

The WPF report supports line and branch coverage, assembly and source views, and navigation to source files.
Summary and risk hotspot views help identify code that needs more testing.

Editor margins and optional line highlighting distinguish covered, uncovered and partially covered code.
For C# and Visual Basic, FCC also tracks edits and newly added lines so old coverage marks do not imply that changed code has been tested.

Useful editor options include:

- `EditorCoverageColouringMode`: disable coloring, or avoid Roslyn analysis on text changes.
- `ShowLineCoverageHighlighting`: enable line highlighting.
- `UseEnterpriseFontsAndColors`: control whether FCC uses the host's built-in coverage colors when available.
- `BlazorCoverageLinesFromGeneratedSource`: limit Razor coverage lines using generated-source information.

Configure colors under **Tools > Options > Environment > Fonts and Colors > Text Editor**.
FCC supplies Covered/Touched, Partially Touched, Not Touched, Dirty, New Lines and Not Included display items.
Search for `Coverage` or `FCC` in the Text Editor display items (including the redesigned Visual Studio settings).
If Visual Studio cannot read a coverage item from Fonts and Colors, FCC uses its registered editor format and logs the failed lookup in the FCC output pane.
If that format also lacks colors, FCC uses built-in defaults: covered green, uncovered red, and partially covered yellow.
Dirty lines remain brown, new lines yellow, and not-included code light pink. Available custom colors are preserved.
Dirty and new-line coloring must be enabled in the options.

Use **Tools > Toggle Indicators** to toggle editor coloring, or **FCC Clear UI** to clear displayed coverage.
Coverage describes the last collected run; rerun tests to measure changed code.
FCC suppresses marks when it detects that a file changed outside the editor or during collection.

## Configuration

For settings that support project scope, values are applied in this order, with later values taking precedence:

1. Visual Studio FCC options.
2. `finecodecoverage-settings.xml` files, from ancestor directories down to the test project's directory.
3. MSBuild project settings.

Set `topLevel="true"` on an XML settings root to stop searching higher directories.
Collections replace inherited values by default. Use `defaultMerge="true"` on the root or `merge="true"` on an individual setting to merge them.

For example, a solution-level `finecodecoverage-settings.xml` can contain:

```xml
<FineCodeCoverage topLevel="true">
  <Enabled>True</Enabled>
  <IncludeReferencedProjects>True</IncludeReferencedProjects>
  <ModulePathsExclude>.*ThirdParty.*</ModulePathsExclude>
  <SourcesExclude>.*[\\/]Migrations[\\/].*</SourcesExclude>
</FineCodeCoverage>
```

Settings can also be written directly in a project:

```xml
<PropertyGroup Label="FineCodeCoverage">
  <Enabled>True</Enabled>
  <IncludeTestAssembly>False</IncludeTestAssembly>
  <ModulePathsExclude>.*ThirdParty.*</ModulePathsExclude>
</PropertyGroup>
```

For imported MSBuild settings or merge attributes, put the settings inside a `FineCodeCoverage` element within a `PropertyGroup`.

The current option definitions and descriptions are in [FineCodeCoverage.Options](FineCodeCoverage.Options).
Run behavior is documented in [RunOptions.cs](FineCodeCoverage.Options/Run/RunOptions.cs).

### Exclusions and inclusions

Microsoft Code Coverage filters use regular expressions. The old OpenCover/Coverlet wildcard filter syntax is not supported.

Use `ModulePathsInclude` / `ModulePathsExclude`, `SourcesInclude` / `SourcesExclude`,
`AttributesInclude` / `AttributesExclude`, `FunctionsInclude` / `FunctionsExclude`,
`CompanyNamesInclude` / `CompanyNamesExclude`, and `PublicKeyTokensInclude` / `PublicKeyTokensExclude`.

Exclusions take precedence. Once you specify includes, ensure they cover every assembly you want to measure.
`IncludeReferencedProjects` helps restrict collection to referenced application projects.
`IncludeTestAssembly` controls whether tests themselves are included.

Use `System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute` to exclude code,
or `AttributesExclude` to match a custom attribute.
A referenced project can opt out using `<FCCExcludeFromCodeCoverage/>` in a `PropertyGroup`; this is not transitive.

### Runsettings

FCC can create VSTest runsettings from a template or augment user-supplied runsettings.
For a custom template, place `fcc-ms-runsettings-template.xml` in the project or solution directory.
It must be valid XML. FCC fills in missing collector, adapter, results-directory and coverage configuration elements.

Template placeholders include `%fcc_modulepaths_exclude%` and `%fcc_testadapter%`.
For user runsettings, FCC ensures the adapter path and collector exist and applies replacement to the supplied collector configuration.

These settings are injected in memory; inspecting the project file for a generated `RunSettingsFilePath` will not show them.
For MTP, FCC prepares coverage configuration and injects coverage options into the output `.testconfig.json`;
it does not use the VSTest collector.

### Output and tools

FCC normally writes coverage artifacts below the test project's build output directory.
A solution-level `fcc-output` directory or the `FCCSolutionOutputDirectoryName` option can redirect output when FCC locates the solution directory.

Bundled coverage tools are extracted below the local application data FineCodeCoverage directory.
`ToolsDirectory` can override the tool location; create the directory and restart Visual Studio after changing it.
See [coverage tool maintenance](dev-readmes/coverage-tool-zips.md) for the bundled tools.

## Building and testing

Use Visual Studio 2026 with the Visual Studio extension development tools and .NET Framework targeting packs.
Open `FineCodeCoverage.slnx`, restore packages and build.
The extension and report tool target .NET Framework 4.7.2; the test project targets .NET Framework 4.8.
The build references `Microsoft.VisualStudio.TestWindow.Interfaces.dll` from the installed Visual Studio using `VsInstallRoot`.
Runtime Roslyn packages are aligned at 4.14.0 in `Directory.Packages.props` to satisfy the exact dependencies of
`Microsoft.CodeAnalysis.EditorFeatures.Text`. Upgrade these packages together; analyzer versions are independent.

Run `FineCodeCoverageTests` through Test Explorer.
The VSIX is produced under `FineCodeCoverage2022/bin/<Configuration>/`; the project directory retains its historical name.
Debugging the extension starts the experimental Visual Studio instance.

See [bundled dependency notes](Shared%20Files/BUNDLED-DLLS.md) for packaging details.

## License and credits

[Apache 2.0](LICENSE).

Based on Fine Code Coverage by its upstream contributors.
Coverage collection uses Microsoft Code Coverage; reports use [ReportGenerator](https://github.com/danielpalme/ReportGenerator).
