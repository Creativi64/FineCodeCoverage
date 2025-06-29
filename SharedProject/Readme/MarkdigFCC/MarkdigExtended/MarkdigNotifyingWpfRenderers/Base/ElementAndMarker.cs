namespace FineCodeCoverage.Readme
{
    public class ElementAndMarker
    {
        public ElementAndMarker(object element, MarkdownTypeMarker marker)
        {
            Element = element;
            Marker = marker;
        }

        public object Element { get; }

        public MarkdownTypeMarker Marker { get; }
    }
}
