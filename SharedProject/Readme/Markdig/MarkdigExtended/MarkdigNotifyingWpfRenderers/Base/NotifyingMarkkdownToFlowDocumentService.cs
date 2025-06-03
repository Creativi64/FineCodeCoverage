using Markdig;

namespace FineCodeCoverage.Readme
{
    public static class NotifyingMarkkdownToFlowDocumentService
    {
        public static FlowDocumentElementMarkers MarkdownToFlowDocument(
            string markdown,
            NotifyingWpfRenderer renderer,
            MarkdownPipeline pipeline = null
        )
        {
            var flowDocument = Markdig.Wpf.Markdown.ToFlowDocument(markdown, pipeline, renderer);
            return new FlowDocumentElementMarkers(flowDocument, renderer.ElementAndMarkers);
        }
    }
}