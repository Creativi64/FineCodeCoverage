using Markdig.Parsers;
using Markdig.Syntax;

namespace MarkdigExtended.Extensions
{
    public class TruncateParser : BlockParser
    {
        private readonly string _matchText;

        public TruncateParser(string matchText)
        {
            _matchText = matchText;
            OpeningCharacters = new[] { matchText.TrimStart()[0] };
        }

        public override BlockState TryOpen(BlockProcessor processor)
        {
            string line = processor.Line.ToString().Trim();

            if (line == _matchText)
            {
                var block = new TruncateBlock(this)
                {
                    Line = processor.LineIndex,
                    Column = processor.Column,
                    Span = new SourceSpan(processor.Start, processor.Line.End),
                };
                processor.NewBlocks.Push(block);
                processor.NoFurtherProcessing();

                return BlockState.BreakDiscard;
            }

            return BlockState.None;
        }
    }
}
