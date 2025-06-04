using Markdig.Syntax;

namespace FineCodeCoverage.Readme
{
    public class MarkerBlock : LeafBlock
    {
        public MarkerBlock(string marker, MarkerBlockParser parser) : base(parser) => this.Marker = marker;

        public string Marker { get; }
    }
}
