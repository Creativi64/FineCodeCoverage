namespace FineCodeCoverage.Core.Utilities
{
    internal interface IDotNetToolListExecutor
    {
        DotNetToolListExecutionResult Global();

        DotNetToolListExecutionResult GlobalToolsPath(string directory);

        DotNetToolListExecutionResult Local(string directory);
    }
}
