using Markdig.Parsers;
using Markdig.Syntax;

namespace MarkdigExtended.Extensions
{
    public class TruncateBlock : LeafBlock
    {
        public TruncateBlock(BlockParser parser)
            : base(parser)
        {
        }
    }
}
