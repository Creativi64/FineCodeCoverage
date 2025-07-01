namespace FineCodeCoverage.Core.Utilities
{
    public interface IDotNetToolListExecutor
    {
        DotNetToolListExecutionResult Global();

        DotNetToolListExecutionResult GlobalToolsPath(string directory);

        DotNetToolListExecutionResult Local(string directory);
    }
}
