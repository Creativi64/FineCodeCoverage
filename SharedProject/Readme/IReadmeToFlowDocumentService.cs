namespace FineCodeCoverage.Readme
{
    public interface IReadmeToFlowDocumentService
    {
        FlowDocumentElementMarkers MarkdownToFlowDocument(string markdown);
    }
}
