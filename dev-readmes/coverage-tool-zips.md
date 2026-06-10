# Updating coverage tool zips procedure

FCC uses 2 coverage tool zips included in the VSIX.
These get extracted to AppData\Local\FineCodeCoverage

- **microsoft.codecoverage** - the MS Code Coverage data collector used for classic VSTest test projects.
- **dotnet-coverage** - used to collect coverage for Microsoft.Testing.Platform (MTP) test projects that do not reference the `Microsoft.Testing.Extensions.CodeCoverage` package (when the package is present, FCC runs the test host's own `--coverage` extension instead).

You need to debug FCC to ensure updates to these zips work. This will override the existing.

## All updated zips will need to follow this procedure.

1. Add to \FineCodeCoverage\Shared Files\ZippedTools directory
2. Add as a linked file to the FineCodeCoverage2022 project

Expand the project in solution explorer  
Right click on the ZippedTools directory  
Add -> Existing Item  
Navigate to \FineCodeCoverage\Shared Files\ZippedTools  
In the browser dialog select All files to see the zips  
Select the zip you want to add  
Click the drop down arrow on the Add button and select Add as Link

3. Set the Build properties in the properties window

Select the file  
Set Build Action to Content  
Set Include in VSIX to True

4. Remove the previous zip in solution explorer

5. For all updated zips debug an appropriate project ( details follow ) and if coverage is provided the old zip can be deleted from \FineCodeCoverage\Shared Files\ZippedTools

## How to update the zips

### msCodeCoverage

1. [Download the nuget package](https://www.nuget.org/packages/Microsoft.CodeCoverage/).
2. Change the file extension to zip
3. Check to see if the zip is compatible

The name should be of the form microsoft.codecoverage.VERSION.zip
It needs to be compatible with MsCodeCoverageRunSettingsService

```csharp
public void Initialize(string appDataFolder, IFCCEngine fccEngine, CancellationToken cancellationToken)
{
    this.fccEngine = fccEngine;
    var zipDestination = toolUnzipper.EnsureUnzipped(appDataFolder, zipDirectoryName,zipPrefix, cancellationToken);
    fccMsTestAdapterPath = Path.Combine(zipDestination, "build", "netstandard2.0");
    shimPath = Path.Combine(zipDestination, "build", "netstandard2.0", "CodeCoverage", "coreclr", "Microsoft.VisualStudio.CodeCoverage.Shim.dll");
}
```

The fccMsTestAdapterPath needs to be a directory that contains a ...collector.dll which is currently called Microsoft.VisualStudio.TraceDataCollector.dll.  
The shimPath needs to exist.

4. Add the zip to the solution - see instruction at the top of this page.
5. Debug

Debug a classic (VSTest) test project.

### dotnet-coverage

1. dotnet tool install --global dotnet-coverage

This will create or update installation in username/.dotnet/tools directory

2. Create a directory - dotnet-coverage.VERSION
3. Create a sub directory - .store

Copy the following from within username/.dotnet/tools
dotnet-coverage.exe into dotnet-coverage.VERSION
dotnet-coverage directory into .store.

4. Add the zip to the solution - see instruction at the top of this page.
5. Debug

Debug a Microsoft.Testing.Platform test project that does not have Microsoft.Testing.Extensions.CodeCoverage as a package reference.
The TUnitRunner class will use dotnet-coverage when the button with id cmdidCollectTUnitCommand is pressed on the tool window.
