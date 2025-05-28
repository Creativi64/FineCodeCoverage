using System.ComponentModel.Composition;
using System.Windows.Documents;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Options;
using FineCodeCoverage.Wpf;
using WpfHelpers;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(ReadMeMarkdownViewModel))]
    internal class ReadMeMarkdownViewModel : ObservableBase
    {
        private readonly ITemplatedReadmeProvider readmeProvider;
        private readonly IFCCMarkdownFlowDocumentProvider fccMarkdownFlowDocumentProvider;
        private readonly IReadMeFlowDocumentStylesSetter readMeFlowDocumentStyleSetter;
        private readonly ProcessStartCommand processStartCommand;
        private bool showHyperlinkUrlHover;

        [ImportingConstructor]
        public ReadMeMarkdownViewModel(
            IProcess process,
            ITemplatedReadmeProvider readmeProvider,
            IFCCMarkdownFlowDocumentProvider fccMarkdownFlowDocumentProvider,
            IReadMeFlowDocumentStylesSetter readMeFlowDocumentStyleSetter,
            IOptionsProvider<MiscOptions> miscOptionsProvider
            )
        {
            this.readmeProvider = readmeProvider;
            this.fccMarkdownFlowDocumentProvider = fccMarkdownFlowDocumentProvider;
            this.readMeFlowDocumentStyleSetter = readMeFlowDocumentStyleSetter;
            this.processStartCommand = new ProcessStartCommand(process);
            ShowHyperlinkUrlHover = miscOptionsProvider.Get().ShowHyperlinkUrlHover;
            miscOptionsProvider.OptionsChanged += (newOptions) => ShowHyperlinkUrlHover = newOptions.ShowHyperlinkUrlHover;
        }

        public bool ShowHyperlinkUrlHover
        {
            get => showHyperlinkUrlHover;
            set => this.Set(ref this.showHyperlinkUrlHover, value);
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
