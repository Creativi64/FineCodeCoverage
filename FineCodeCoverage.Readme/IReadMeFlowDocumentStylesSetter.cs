using System.Collections.Generic;
using MarkdigExtended.NotifyingWpfRenderers.Base;

namespace FineCodeCoverage.Readme
{
    public interface IReadMeFlowDocumentStylesSetter
    {
        void SetStyles(IReadOnlyList<ElementAndMarker> elementAndMarkers);
    }
}
