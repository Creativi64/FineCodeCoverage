using System;
using System.Windows.Documents;
using System.ComponentModel.Composition;
using Markdig;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IFCCMarkdownFlowDocumentProvider))]
    internal class FCCMarkdownFlowDocumentProvider : IFCCMarkdownFlowDocumentProvider
    {
        private readonly IOptionPageTableCreator optionPageTableCreator;
        private readonly IReadMePipelineProvider readMePipeLineProvider;
        [ImportingConstructor]
        public FCCMarkdownFlowDocumentProvider(
            IOptionPageTableCreator optionPageTableCreator,
            IReadMePipelineProvider readMePipeLineProvider)
        {
            this.optionPageTableCreator = optionPageTableCreator;
            this.readMePipeLineProvider = readMePipeLineProvider;
        }
        public Func<FlowDocumentElementMarkers> Provide(string readmeTemplate, string optionTableReplacementMarker)
        {
            var pipeline = readMePipeLineProvider.Provide(optionTableReplacementMarker, optionPageTableCreator.Create);
            var markdownDocument = Markdown.Parse(readmeTemplate, pipeline);
            return () =>
            {
                var fccWpfRenderer = new FCCMarkdigWpfRenderer();
                var flowDocument = new FlowDocument();
                fccWpfRenderer.LoadDocument(flowDocument);
                pipeline.Setup(fccWpfRenderer);
                fccWpfRenderer.Render(markdownDocument);
                return new FlowDocumentElementMarkers(flowDocument, fccWpfRenderer.ElementAndMarkers);
            };
        }
    }
}
