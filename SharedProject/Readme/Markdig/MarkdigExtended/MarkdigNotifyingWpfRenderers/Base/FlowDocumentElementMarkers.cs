using System.Collections.Generic;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
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
