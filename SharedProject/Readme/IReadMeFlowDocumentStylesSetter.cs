using System.Collections.Generic;

namespace FineCodeCoverage.Readme
{
    internal interface IReadMeFlowDocumentStylesSetter
    {
        void SetStyles(IReadOnlyList<ElementAndMarker> elementAndMarkers);
    }
}
