namespace FineCodeCoverage.Utilities.DotNetToolList
{
    public interface IDotNetToolListExecutor
    {
        DotNetToolListExecutionResult Global();

        DotNetToolListExecutionResult GlobalToolsPath(string directory);

        DotNetToolListExecutionResult Local(string directory);
    }
}
