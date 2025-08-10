using System.Threading.Tasks;
using FineCodeCoverage.Collection.TestExplorer;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Collection.Ms
{
    internal interface IMsCodeCoverageRunSettingsService
    {
        Task<MsCodeCoverageCollectionStatus> IsCollectingAsync(ITestOperation testOperation);

        Task CollectAsync(IOperation operation, ITestOperation testOperation);

        void StopCoverage();

        Task TestExecutionNotFinishedAsync(ITestOperation testOperation);
    }
}
