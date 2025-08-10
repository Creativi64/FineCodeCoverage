using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.Initialization;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using ILogger = FineCodeCoverage.VSAbstractions.OutputWindow.ILogger;

namespace FineCodeCoverage.Collection.TestExplorer
{
    [Export(typeof(ITestOperationStateInvocationManager))]
    internal sealed class TestOperationStateInvocationManager : ITestOperationStateInvocationManager
    {
        private readonly IInitializeStatusProvider _initializeStatusProvider;
        private readonly ILogger _logger;
        private bool _initializedWhenTestExecutionStarting;

        [ImportingConstructor]
        public TestOperationStateInvocationManager(
            IInitializeStatusProvider initializeStatusProvider,
            ILogger logger)
        {
            _initializeStatusProvider = initializeStatusProvider;
            _logger = logger;
        }

        public async Task<bool> CanInvokeAsync(TestOperationStates testOperationState)
        {
            if (testOperationState == TestOperationStates.TestExecutionStarting)
            {
                _initializedWhenTestExecutionStarting = _initializeStatusProvider.InitializeStatus == InitializeStatus.Initialized;
            }

            if (!_initializedWhenTestExecutionStarting)
            {
                await _logger.LogAsync($"Skipping {testOperationState} as FCC not initialized");
            }

            return _initializedWhenTestExecutionStarting;
        }
    }
}
