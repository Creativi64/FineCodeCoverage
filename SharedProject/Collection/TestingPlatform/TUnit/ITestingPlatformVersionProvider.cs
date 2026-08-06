using System;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    internal interface ITestingPlatformVersionProvider
    {
        /// <summary>
        /// The version of Microsoft.Testing.Platform the test application will run on,
        /// null when it cannot be determined.
        /// </summary>
        Version GetVersion(string projectOutputFolder);
    }
}
