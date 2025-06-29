using Markdig.Parsers;

namespace MarkdigExtended.Extensions
{
    public class MarkerBlockParser : BlockParser
    {
        public MarkerBlockParser() => OpeningCharacters = new[] { '{' }; // trigger on `{`

        private static string? GetMarker(string text)
            => text.StartsWith("{{") && text.EndsWith("}}") ? text.Substring(2, text.Length - 4).Trim() : null;

        public override BlockState TryOpen(BlockProcessor processor)
        {
            Markdig.Helpers.StringSlice line = processor.Line;
            string text = line.ToString();
            var marker = GetMarker(text);
            if (marker != null)
            {
                var block = new MarkerBlock(marker, this);
                processor.NewBlocks.Push(block);
                return BlockState.BreakDiscard;
            }

            return BlockState.None;
        }
    }
}
