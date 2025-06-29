using System.Windows.Documents;

namespace MarkdigExtended.NotifyingWpfRenderers.Base
{
    public class FlowDocumentElementMarkers
    {
        public FlowDocumentElementMarkers(
            FlowDocument flowDocument,
            IReadOnlyList<ElementAndMarker> elementAndMarkers)
        {
            FlowDocument = flowDocument;
            ElementAndMarkers = elementAndMarkers;
        }

        public FlowDocument FlowDocument { get; }

        public IReadOnlyList<ElementAndMarker> ElementAndMarkers { get; }
    }
}
