using System;

namespace FineCodeCoverage.Readme
{
    interface IFCCMarkdownFlowDocumentProvider
    {
        Func<FlowDocumentElementMarkers> Provide(TemplatedReadmeInfo templatedReadMeInfo, string optionTableReplacementMarker);
    }
}
