using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    [Export(typeof(ITestingPlatformVersionProvider))]
    internal sealed class TestingPlatformVersionProvider : ITestingPlatformVersionProvider
    {
        private const string TestingPlatformAssemblyFileName = "Microsoft.Testing.Platform.dll";

        public Version GetVersion(string projectOutputFolder)
        {
            try
            {
                string assemblyPath = Path.Combine(projectOutputFolder, TestingPlatformAssemblyFileName);
                return File.Exists(assemblyPath)
                    ? ParseInformationalVersion(FileVersionInfo.GetVersionInfo(assemblyPath).ProductVersion)
                    : null;
            }
            catch
            {
                return null;
            }
        }

        /*
            FileVersion is not the package version - Microsoft.Testing.Platform 2.3.3 ships as
            FileVersion 2.300.326.37705.  ProductVersion is the informational version, e.g.
            "2.3.3+44aa76e6a61d4908f06dfd77a51d4b7e3e7ce40f".
        */
        private static Version ParseInformationalVersion(string informationalVersion)
        {
            if (string.IsNullOrWhiteSpace(informationalVersion))
            {
                return null;
            }

            string versionPart = informationalVersion.Split('+', '-')[0];
            return Version.TryParse(versionPart, out Version version) ? version : null;
        }
    }
}
