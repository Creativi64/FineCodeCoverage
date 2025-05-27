using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Wpf;
using System.ComponentModel.Composition;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(ReadMeMarkdownViewModel))]
    internal class ReadMeMarkdownViewModel
    {
        private readonly ITemplatedReadmeProvider readmeProvider;
        private readonly IFCCMarkdownFlowDocumentProvider fccMarkdownFlowDocumentProvider;
        private readonly IReadMeFlowDocumentStylesSetter readMeFlowDocumentStyleSetter;
        private readonly ProcessStartCommand processStartCommand;

        [ImportingConstructor]
        public ReadMeMarkdownViewModel(
            IProcess process,
            ITemplatedReadmeProvider readmeProvider,
            IFCCMarkdownFlowDocumentProvider fccMarkdownFlowDocumentProvider,
            IReadMeFlowDocumentStylesSetter readMeFlowDocumentStyleSetter
            )
        {
            this.readmeProvider = readmeProvider;
            this.fccMarkdownFlowDocumentProvider = fccMarkdownFlowDocumentProvider;
            this.readMeFlowDocumentStyleSetter = readMeFlowDocumentStyleSetter;
            this.processStartCommand = new ProcessStartCommand(process);
        }

        public FlowDocument FlowDocument
        {
            get
            {
                var templatedReadmeInfo = this.readmeProvider.GetTemplatedReadme();
                var flowDocumentElementMarkers = fccMarkdownFlowDocumentProvider.Provide(
                    templatedReadmeInfo,
                    "FCCOptionsTable",
                    this.processStartCommand
                    )();
                this.readMeFlowDocumentStyleSetter.SetStyles(flowDocumentElementMarkers.ElementAndMarkers);
                return flowDocumentElementMarkers.FlowDocument;
            }
        }
    }
}
