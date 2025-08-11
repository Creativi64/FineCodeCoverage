using System;
using System.Windows.Input;
using FineCodeCoverage.Readme.Template;
using MarkdigExtended.NotifyingWpfRenderers.Base;

namespace FineCodeCoverage.Readme
{
    public interface IFCCMarkdownFlowDocumentProvider
    {
        Func<FlowDocumentElementMarkers> Provide(
            TemplatedReadmeInfo templatedReadMeInfo,
            string optionTableReplacementMarker,
            string truncateMarker,
            ICommand navigateCommand);
    }
}
