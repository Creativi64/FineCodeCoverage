namespace FineCodeCoverage.Readme
{
    public class MarkerBlockParser : BlockParser
    {
        public MarkerBlockParser()
        {
            OpeningCharacters = new[] { '{' }; // trigger on `{`
        }

        private string GetMarker(string text)
        {
            // Extract the marker from the text, e.g., "{{marker}}" -> "marker"
            if (text.StartsWith("{{") && text.EndsWith("}}"))
            {
                return text.Substring(2, text.Length - 4).Trim();
            }
            return null;
        }

        public override BlockState TryOpen(BlockProcessor processor)
        {
            var line = processor.Line;
            var text = line.ToString();
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
