using System.Collections.Generic;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.Engine;
using FineCodeCoverage.Collection.TestExplorer;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Collection.Ms
{
    internal interface IMsCodeCoverageRunSettingsService
    {
        /// <param name="excludedProjectFilePaths">
        /// Projects another collector owns.  Microsoft.Testing.Platform projects do not run under VSTest,
        /// so the data collector can never produce coverage for them and they must not be prepared here -
        /// preparing empties the coverage output folder the other collector is already using.
        /// </param>
        Task<MsCodeCoverageCollectionStatus> IsCollectingAsync(
            ITestOperation testOperation,
            IReadOnlyCollection<string> excludedProjectFilePaths = null);

        /// <summary>
        /// The coverage this collector produced.  The caller combines it with the other collectors'
        /// results so a mixed solution still produces a single report.
        /// </summary>
        Task<CoverageCollectionResult> CollectAsync(IOperation operation, ITestOperation testOperation);

        void StopCoverage();

        Task TestExecutionNotFinishedAsync(ITestOperation testOperation);
    }
}
