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
        private FlowDocument flowDocument;
        public const string OptionsTableReplacementMarker = "FCCOptionsTable";
        public const string TruncateMarker = "## Please support the project";

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
            this.ShowHyperlinkUrlHover = miscOptionsProvider.Get().ShowHyperlinkUrlHover;
            miscOptionsProvider.OptionsChanged += (newOptions) => this.ShowHyperlinkUrlHover = newOptions.ShowHyperlinkUrlHover;
        }

        public bool ShowHyperlinkUrlHover
        {
            get => this.showHyperlinkUrlHover;
            set => this.Set(ref this.showHyperlinkUrlHover, value);
        }

        private FlowDocument GetFlowDocument()
        {
            TemplatedReadmeInfo templatedReadmeInfo = this.readmeProvider.GetTemplatedReadme();
            FlowDocumentElementMarkers flowDocumentElementMarkers = this.fccMarkdownFlowDocumentProvider.Provide(
                templatedReadmeInfo,
                ReadMeMarkdownViewModel.OptionsTableReplacementMarker,
                ReadMeMarkdownViewModel.TruncateMarker,
                this.processStartCommand
                )();
            this.readMeFlowDocumentStyleSetter.SetStyles(flowDocumentElementMarkers.ElementAndMarkers);
            return flowDocumentElementMarkers.FlowDocument;
        }

        public FlowDocument FlowDocument => this.flowDocument ?? (this.flowDocument = this.GetFlowDocument());
    }
}
