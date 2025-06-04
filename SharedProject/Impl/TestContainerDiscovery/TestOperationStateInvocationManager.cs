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
        private readonly IInitializeStatusProvider _initializeStatusProvider;
        private readonly ILogger _logger;
        private bool _initializedWhenTestExecutionStarting;

        [ImportingConstructor]
        public TestOperationStateInvocationManager(
            IInitializeStatusProvider initializeStatusProvider,
            ILogger logger
        )
        {
            this._initializeStatusProvider = initializeStatusProvider;
            this._logger = logger;
        }

        public async Task<bool> CanInvokeAsync(TestOperationStates testOperationState)
        {
            if (testOperationState == TestOperationStates.TestExecutionStarting)
            {
                this._initializedWhenTestExecutionStarting = this._initializeStatusProvider.InitializeStatus == InitializeStatus.Initialized;
            }

            if (!this._initializedWhenTestExecutionStarting)
            {
                await this._logger.LogAsync($"Skipping {testOperationState} as FCC not initialized");
            }

            return this._initializedWhenTestExecutionStarting;
        }
    }
}