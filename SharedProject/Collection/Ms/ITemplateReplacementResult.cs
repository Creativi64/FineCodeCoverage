namespace FineCodeCoverage.Collection.Ms
{
    internal interface ITemplateReplacementResult
    {
        string Replaced { get; }

        bool ReplacedTestAdapter { get; }
    }
}
