using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Coverlet
{
    [Export(typeof(ICoverletConsoleExecuteRequestProvider))]
    internal class CoverletConsoleExecuteRequestProvider : ICoverletConsoleExecuteRequestProvider
    {
        private readonly List<ICoverletConsoleExecutor> executors;

        [ImportingConstructor]
        public CoverletConsoleExecuteRequestProvider(
            [Import(typeof(ICoverletConsoleDotnetToolsGlobalExecutor))]
            ICoverletConsoleExecutor globalExecutor,
            [Import(typeof(ICoverletConsoleCustomPathExecutor))]
            ICoverletConsoleExecutor customPathExecutor,
            [Import(typeof(ICoverletConsoleDotnetToolsLocalExecutor))]
            ICoverletConsoleExecutor localExecutor,
            IFCCCoverletConsoleExecutor fccExecutor
        ) => this.executors = new List<ICoverletConsoleExecutor>
            {
                localExecutor,
                customPathExecutor,
                globalExecutor,
                fccExecutor
            };

        // for now FCCCoverletConsoleExeProvider can return null for exe path
        public async Task<ExecuteRequest> GetExecuteRequestAsync(ICoverageProject project, string coverletSettings)
        {
            foreach (ICoverletConsoleExecutor exeProvider in this.executors)
            {
                ExecuteRequest executeRequest = await exeProvider.GetRequestAsync(project, coverletSettings);
                if (executeRequest != null)
                {
                    return executeRequest;
                }
            }

            return null;//todo change to throw when using zip file
        }
    }
}