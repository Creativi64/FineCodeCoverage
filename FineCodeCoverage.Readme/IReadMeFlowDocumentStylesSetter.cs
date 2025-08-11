using System.Collections.Generic;
using MarkdigExtended.NotifyingWpfRenderers.Base;

namespace FineCodeCoverage.Readme
{
    internal interface IReadMeFlowDocumentStylesSetter
    {
        void SetStyles(IReadOnlyList<ElementAndMarker> elementAndMarkers);
    }
}
