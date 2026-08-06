using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    /// <summary>
    /// Writes the coverage command line options into the test application's [AssemblyName].testconfig.json
    /// so that a Microsoft.Testing.Platform test host launched by the Test Explorer collects coverage.
    /// </summary>
    /// <remarks>
    /// A Test Explorer run launches the built executable with its own arguments (--server --client-port ...);
    /// neither MSBuild properties nor TestingPlatformCommandLineArguments reach that process.  The
    /// configuration file, which the host loads on start, is the only injection point that works.
    /// Reading command line options from it requires Microsoft.Testing.Platform 2.3.0 or later.
    /// </remarks>
    internal interface ITestConfigInjector
    {
        Task<TestConfigInjectionResult> InjectAsync(
            ITUnitCoverageProject tUnitCoverageProject,
            TUnitSettings tUnitSettings,
            CancellationToken cancellationToken);
    }
}
