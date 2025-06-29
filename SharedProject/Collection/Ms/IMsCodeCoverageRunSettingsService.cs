using System.Threading.Tasks;
using FineCodeCoverage.Impl;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal interface IMsCodeCoverageRunSettingsService
    {
        Task<MsCodeCoverageCollectionStatus> IsCollectingAsync(ITestOperation testOperation);

        Task CollectAsync(IOperation operation, ITestOperation testOperation);

        void StopCoverage();

        Task TestExecutionNotFinishedAsync(ITestOperation testOperation);
    }
}
