using System;
using System.Windows.Documents;
using System.ComponentModel.Composition;
using Markdig;
using System.Linq;
using System.Windows.Input;

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

        public Func<FlowDocumentElementMarkers> Provide(
            TemplatedReadmeInfo templatedReadMeInfo, 
            string optionTableReplacementMarker,
            ICommand navigateCommand
            )
        {
            var pipeline = readMePipeLineProvider.Provide(optionTableReplacementMarker, optionPageTableCreator.Create);
            var markdownDocument = Markdown.Parse(templatedReadMeInfo.Readme, pipeline);
            return () =>
            {
                var fccWpfRenderer = new FCCMarkdigWpfRenderer(templatedReadMeInfo.Directory, navigateCommand);
                var flowDocument = new FlowDocument();
                fccWpfRenderer.LoadDocument(flowDocument);
                pipeline.Setup(fccWpfRenderer);
                fccWpfRenderer.Render(markdownDocument);
                var elementAndMarkers = fccWpfRenderer.ElementAndMarkers.Concat(optionPageTableCreator.ElementAndMarkers).ToList();
                elementAndMarkers.Add(new ElementAndMarker(flowDocument, MarkdownTypeMarker.FlowDocument));
                return new FlowDocumentElementMarkers(flowDocument, elementAndMarkers);
            };
        }
    }
}
