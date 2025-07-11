using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.VSAbstractions.OutputWindow;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Process
{
    [Export(typeof(IProcessResponseProcessor))]
    internal sealed class ProcessResponseProcessor : IProcessResponseProcessor
    {
        private readonly ILogger _logger;

        [ImportingConstructor]
        public ProcessResponseProcessor(ILogger logger) => _logger = logger;

        public async Task<bool> ProcessAsync(ExecuteResponse result, Func<int, bool> exitCodeSuccessPredicate, bool throwError, string title, Action successCallback = null)
        {
            if (!exitCodeSuccessPredicate(result.ExitCode))
            {
                if (throwError)
                {
                    throw new ProcessResponseException(result.Output);
                }

                await _logger.LogAsync($"{title} Error", result.Output);
                return false;
            }

            await _logger.LogAsync(title, result.Output);
            successCallback?.Invoke();
            return true;
        }
    }
}
