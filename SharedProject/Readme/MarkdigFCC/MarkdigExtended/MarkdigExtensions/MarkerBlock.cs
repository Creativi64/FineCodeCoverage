using Markdig.Syntax;

namespace FineCodeCoverage.Readme
{
    public class MarkerBlock : LeafBlock
    {
        public MarkerBlock(string marker, MarkerBlockParser parser)
            : base(parser) => Marker = marker;

        public string Marker { get; }
    }
}
