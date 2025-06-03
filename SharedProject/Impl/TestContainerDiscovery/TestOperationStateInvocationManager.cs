using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Initialization;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using ILogger = FineCodeCoverage.Output.ILogger;

namespace FineCodeCoverage.Impl
{
    [Export(typeof(ITestOperationStateInvocationManager))]
    internal class TestOperationStateInvocationManager : ITestOperationStateInvocationManager
    {
        private readonly IInitializeStatusProvider initializeStatusProvider;
        private readonly ILogger logger;
        private bool initializedWhenTestExecutionStarting;

        [ImportingConstructor]
        public TestOperationStateInvocationManager(
            IInitializeStatusProvider initializeStatusProvider,
            ILogger logger
        )
        {
            this.initializeStatusProvider = initializeStatusProvider;
            this.logger = logger;
        }
        public async Task<bool> CanInvokeAsync(TestOperationStates testOperationState)
        {
            if (testOperationState == TestOperationStates.TestExecutionStarting)
            {
                this.initializedWhenTestExecutionStarting = this.initializeStatusProvider.InitializeStatus == InitializeStatus.Initialized;
            }

            if (!this.initializedWhenTestExecutionStarting)
            {
                await this.logger.LogAsync($"Skipping {testOperationState} as FCC not initialized");
            }

            return this.initializedWhenTestExecutionStarting;
        }
    }
}