using FineCodeCoverage.Output;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IProcessResponseProcessor))]
    internal class ProcessResponseProcessor : IProcessResponseProcessor
    {
        private readonly ILogger logger;

        [ImportingConstructor]
        public ProcessResponseProcessor(ILogger logger)
        {
            this.logger = logger;
        }
        public async Task<bool> ProcessAsync(ExecuteResponse result, Func<int, bool> exitCodeSuccessPredicate, bool throwError, string title,Action successCallback = null)
        {
            if (!exitCodeSuccessPredicate(result.ExitCode))
            {
                if (throwError)
                {
                    throw new Exception(result.Output);
                }

                await logger.LogAsync($"{title} Error", result.Output);
                return false;
            }

            await logger.LogAsync(title, result.Output);
            successCallback?.Invoke();
            return true;
        }
    }
}
