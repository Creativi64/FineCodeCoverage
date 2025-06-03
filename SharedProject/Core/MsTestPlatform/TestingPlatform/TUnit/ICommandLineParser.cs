namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ICommandLineParser
    {
        CommandLineParseResult Parse(string argumentsString);
    }
}