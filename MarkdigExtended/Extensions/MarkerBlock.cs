using Markdig.Syntax;

namespace MarkdigExtended.Extensions
{
    public class MarkerBlock : LeafBlock
    {
        public MarkerBlock(string marker, MarkerBlockParser parser)
            : base(parser) => Marker = marker;

        public string Marker { get; }
    }
}
