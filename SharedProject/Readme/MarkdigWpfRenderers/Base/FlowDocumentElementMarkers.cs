using System.Collections.Generic;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    public class FlowDocumentElementMarkers
    {
        public FlowDocumentElementMarkers(FlowDocument flowDocument, List<ElementAndMarker> elementAndMarkers)
        {
            FlowDocument = flowDocument;
            ElementAndMarkers = elementAndMarkers;
        }

        public FlowDocument FlowDocument { get; }
        public List<ElementAndMarker> ElementAndMarkers { get; }
    }
}
