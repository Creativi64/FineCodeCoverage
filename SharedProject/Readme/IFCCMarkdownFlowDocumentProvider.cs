using System;

namespace FineCodeCoverage.Readme
{
    interface IFCCMarkdownFlowDocumentProvider
    {
        Func<FlowDocumentElementMarkers> Provide(string readmeTemplate, string optionTableReplacementMarker);
    }
}
