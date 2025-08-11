using System;
using System.Windows.Input;
using MarkdigExtended.NotifyingWpfRenderers.Base;

namespace FineCodeCoverage.Readme
{
    internal interface IFCCMarkdownFlowDocumentProvider
    {
        Func<FlowDocumentElementMarkers> Provide(
            TemplatedReadmeInfo templatedReadMeInfo,
            string optionTableReplacementMarker,
            string truncateMarker,
            ICommand navigateCommand);
    }
}
