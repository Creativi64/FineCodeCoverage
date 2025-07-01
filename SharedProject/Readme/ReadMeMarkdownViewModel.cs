using System.ComponentModel.Composition;
using System.Windows.Documents;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Options.Misc;
using FineCodeCoverage.Utilities.Wpf.Commands;
using FineCodeCoverage.Utilities.Wrappers;
using MarkdigExtended.NotifyingWpfRenderers.Base;
using WpfHelpers;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(ReadMeMarkdownViewModel))]
    internal sealed class ReadMeMarkdownViewModel : ObservableBase
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
            IOptionsProvider<MiscOptions> miscOptionsProvider)
        {
            _readmeProvider = readmeProvider;
            _fccMarkdownFlowDocumentProvider = fccMarkdownFlowDocumentProvider;
            _readMeFlowDocumentStyleSetter = readMeFlowDocumentStyleSetter;
            _processStartCommand = new ProcessStartCommand(process);
            ShowHyperlinkUrlHover = miscOptionsProvider.Get().ShowHyperlinkUrlHover;
            miscOptionsProvider.OptionsChanged += (newOptions) => ShowHyperlinkUrlHover = newOptions.ShowHyperlinkUrlHover;
        }

        public bool ShowHyperlinkUrlHover
        {
            get => _showHyperlinkUrlHover;
            set => Set(ref _showHyperlinkUrlHover, value);
        }

        private FlowDocument GetFlowDocument()
        {
            TemplatedReadmeInfo templatedReadmeInfo = _readmeProvider.GetTemplatedReadme();
            FlowDocumentElementMarkers flowDocumentElementMarkers = _fccMarkdownFlowDocumentProvider.Provide(
                templatedReadmeInfo,
                ReadMeMarkdownViewModel.OptionsTableReplacementMarker,
                ReadMeMarkdownViewModel.TruncateMarker,
                _processStartCommand)();
            _readMeFlowDocumentStyleSetter.SetStyles(flowDocumentElementMarkers.ElementAndMarkers);
            return flowDocumentElementMarkers.FlowDocument;
        }

        public FlowDocument FlowDocument => _flowDocument ?? (_flowDocument = GetFlowDocument());
    }
}
