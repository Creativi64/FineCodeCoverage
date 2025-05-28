namespace FineCodeCoverage.Readme
{
    public class ElementAndMarker
    {
        public ElementAndMarker(object element, MarkdownTypeMarker marker)
        {
            this.Element = element;
            this.Marker = marker;
        }

        public object Element { get; }
        public MarkdownTypeMarker Marker { get; }
    }
}
