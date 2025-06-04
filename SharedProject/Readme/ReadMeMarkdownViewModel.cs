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
        private readonly ITemplatedReadmeProvider _readmeProvider;
        private readonly IFCCMarkdownFlowDocumentProvider _fccMarkdownFlowDocumentProvider;
        private readonly IReadMeFlowDocumentStylesSetter _readMeFlowDocumentStyleSetter;
        private readonly ProcessStartCommand _processStartCommand;
        private bool _showHyperlinkUrlHover;
        private FlowDocument _flowDocument;
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
            this._readmeProvider = readmeProvider;
            this._fccMarkdownFlowDocumentProvider = fccMarkdownFlowDocumentProvider;
            this._readMeFlowDocumentStyleSetter = readMeFlowDocumentStyleSetter;
            this._processStartCommand = new ProcessStartCommand(process);
            this.ShowHyperlinkUrlHover = miscOptionsProvider.Get().ShowHyperlinkUrlHover;
            miscOptionsProvider.OptionsChanged += (newOptions) => this.ShowHyperlinkUrlHover = newOptions.ShowHyperlinkUrlHover;
        }

        public bool ShowHyperlinkUrlHover
        {
            get => this._showHyperlinkUrlHover;
            set => this.Set(ref this._showHyperlinkUrlHover, value);
        }

        private FlowDocument GetFlowDocument()
        {
            TemplatedReadmeInfo templatedReadmeInfo = this._readmeProvider.GetTemplatedReadme();
            FlowDocumentElementMarkers flowDocumentElementMarkers = this._fccMarkdownFlowDocumentProvider.Provide(
                templatedReadmeInfo,
                ReadMeMarkdownViewModel.OptionsTableReplacementMarker,
                ReadMeMarkdownViewModel.TruncateMarker,
                this._processStartCommand
                )();
            this._readMeFlowDocumentStyleSetter.SetStyles(flowDocumentElementMarkers.ElementAndMarkers);
            return flowDocumentElementMarkers.FlowDocument;
        }

        public FlowDocument FlowDocument => this._flowDocument ?? (this._flowDocument = this.GetFlowDocument());
    }
}
