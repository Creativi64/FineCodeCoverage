using System;
using Newtonsoft.Json.Linq;

namespace FineCodeCoverage.Collection.TestingPlatform
{
    internal static class TestingPlatformCompatibility
    {
        internal static readonly Version MinimumNativeCoverageVersion = new(2, 3, 0);

        internal static bool RequiresVsTest(string assetsContents)
        {
            if (string.IsNullOrWhiteSpace(assetsContents))
            {
                return false;
            }

            if (JObject.Parse(assetsContents)["libraries"] is not JObject libraries)
            {
                return false;
            }

            Version platformVersion = null;
            bool hasTestSdk = false;
            bool hasAdapter = false;
            foreach (JProperty library in libraries.Properties())
            {
                int separator = library.Name.LastIndexOf('/');
                if (separator < 0)
                {
                    continue;
                }

                string packageId = library.Name.Substring(0, separator);
                if (packageId.Equals("TUnit", StringComparison.OrdinalIgnoreCase) ||
                    packageId.StartsWith("TUnit.", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (packageId.Equals("Microsoft.Testing.Platform", StringComparison.OrdinalIgnoreCase))
                {
                    string version = library.Name.Substring(separator + 1).Split('-', '+')[0];
                    if (!Version.TryParse(version, out platformVersion) || platformVersion >= MinimumNativeCoverageVersion)
                    {
                        return false;
                    }
                }

                hasTestSdk |= packageId.Equals("Microsoft.NET.Test.Sdk", StringComparison.OrdinalIgnoreCase);
                hasAdapter |= packageId.Equals("MSTest.TestAdapter", StringComparison.OrdinalIgnoreCase) ||
                    packageId.Equals("NUnit3TestAdapter", StringComparison.OrdinalIgnoreCase) ||
                    packageId.Equals("xunit.runner.visualstudio", StringComparison.OrdinalIgnoreCase);
            }

            return platformVersion != null && platformVersion < MinimumNativeCoverageVersion && hasTestSdk && hasAdapter;
        }
    }
}
