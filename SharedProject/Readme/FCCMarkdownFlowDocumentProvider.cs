using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using Markdig;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IFCCMarkdownFlowDocumentProvider))]
    internal class FCCMarkdownFlowDocumentProvider : IFCCMarkdownFlowDocumentProvider
    {
        private readonly IOptionPageTableCreator _optionPageTableCreator;
        private readonly IReadMePipelineProvider _readMePipeLineProvider;

        [ImportingConstructor]
        public FCCMarkdownFlowDocumentProvider(
            IOptionPageTableCreator optionPageTableCreator,
            IReadMePipelineProvider readMePipeLineProvider)
        {
            _optionPageTableCreator = optionPageTableCreator;
            _readMePipeLineProvider = readMePipeLineProvider;
        }

        public Func<FlowDocumentElementMarkers> Provide(
            TemplatedReadmeInfo templatedReadMeInfo,
            string optionTableReplacementMarker,
            string truncateMarker,
            ICommand navigateCommand)
        {
            MarkdownPipeline pipeline = _readMePipeLineProvider.Provide(optionTableReplacementMarker, truncateMarker, _optionPageTableCreator.Create);
            Markdig.Syntax.MarkdownDocument markdownDocument = Markdown.Parse(templatedReadMeInfo.Readme, pipeline);
            return () =>
            {
                var fccWpfRenderer = new FCCMarkdigWpfRenderer(templatedReadMeInfo.Directory, navigateCommand);
                var flowDocument = new FlowDocument();
                fccWpfRenderer.LoadDocument(flowDocument);
                pipeline.Setup(fccWpfRenderer);
                _ = fccWpfRenderer.Render(markdownDocument);
                var elementAndMarkers = fccWpfRenderer.ElementAndMarkers.Concat(_optionPageTableCreator.ElementAndMarkers).ToList();
                elementAndMarkers.Add(new ElementAndMarker(flowDocument, MarkdownTypeMarker.FlowDocument));
                return new FlowDocumentElementMarkers(flowDocument, elementAndMarkers);
            };
        }
    }
}
