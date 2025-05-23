namespace FineCodeCoverage.Readme
{
    internal interface IReadmeToFlowDocumentService
    {
        FlowDocumentElementMarkers MarkdownToFlowDocument(string markdown);
    }
}
