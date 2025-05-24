using Markdig;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IReadmeToFlowDocumentService))]
    internal class ReadmeToFlowDocumentService : IReadmeToFlowDocumentService
    {
        public FlowDocumentElementMarkers MarkdownToFlowDocument(string markdown)
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
            return NotifyingMarkkdownToFlowDocumentService.MarkdownToFlowDocument(
                markdown, new FCCMarkdigWpfRenderer(), pipeline);
        }
    }
}
