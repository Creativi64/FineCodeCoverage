namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal interface ITemplateReplacementResult
    {
        string Replaced { get; }

        bool ReplacedTestAdapter { get; }
    }
}
