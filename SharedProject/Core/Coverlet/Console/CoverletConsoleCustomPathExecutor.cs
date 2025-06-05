using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Coverlet
{
    [Export(typeof(ICoverletConsoleCustomPathExecutor))]
    internal class CoverletConsoleCustomPathExecutor : ICoverletConsoleCustomPathExecutor
    {
        public Task<ExecuteRequest> GetRequestAsync(ICoverageProject coverageProject, string coverletSettings)
        {
            string coverletConsoleCustomPath = coverageProject.Settings.CoverletConsoleCustomPath;
            return string.IsNullOrWhiteSpace(coverletConsoleCustomPath)
                ? Task.FromResult<ExecuteRequest>(null)
                : File.Exists(coverletConsoleCustomPath) && Path.GetExtension(coverletConsoleCustomPath) == ".exe"
                    ? Task.FromResult(
                        new ExecuteRequest
                        {
                            FilePath = coverletConsoleCustomPath,
                            Arguments = coverletSettings,
                            WorkingDirectory = coverageProject.ProjectOutputFolder,
                        })
                    : Task.FromResult<ExecuteRequest>(null);
        }
    }
}
