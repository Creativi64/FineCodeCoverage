using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Coverlet
{
    internal interface ICoverletConsoleCustomPathExecutor : ICoverletConsoleExecutor { }

    [Export(typeof(ICoverletConsoleCustomPathExecutor))]
    internal class CoverletConsoleCustomPathExecutor : ICoverletConsoleCustomPathExecutor
    {
        public Task<ExecuteRequest> GetRequestAsync(ICoverageProject coverageProject,string coverletSettings)
        {
            var coverletConsoleCustomPath = coverageProject.Settings.CoverletConsoleCustomPath;
            if (string.IsNullOrWhiteSpace(coverletConsoleCustomPath))
            {
				return Task.FromResult<ExecuteRequest>(null);
            }
            if (File.Exists(coverletConsoleCustomPath) && Path.GetExtension(coverletConsoleCustomPath) == ".exe")
            {
                return Task.FromResult(
                    new ExecuteRequest
                    {
                        FilePath = coverletConsoleCustomPath,
                        Arguments = coverletSettings,
                        WorkingDirectory = coverageProject.ProjectOutputFolder
                    }
                );
            }
			return Task.FromResult<ExecuteRequest>(null);
        }
    }
}
