using System;
using System.Windows.Input;

namespace FineCodeCoverage.Readme
{
    interface IFCCMarkdownFlowDocumentProvider
    {
        Func<FlowDocumentElementMarkers> Provide(
            TemplatedReadmeInfo templatedReadMeInfo,
            string optionTableReplacementMarker,
            ICommand navigateCommand
            );
    }
}
