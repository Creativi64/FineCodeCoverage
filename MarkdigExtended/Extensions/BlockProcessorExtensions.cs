using System.Reflection;
using Markdig.Parsers;

namespace MarkdigExtended.Extensions
{
    public static class BlockProcessorExtensions
    {
        public static void NoFurtherProcessing(this BlockProcessor blockProcessor)
        {
            var emptyBlockParsers = new BlockParserList(System.Array.Empty<BlockParser>());
            GetParsersPropertyInfo().SetValue(blockProcessor, emptyBlockParsers);
        }

        private static PropertyInfo GetParsersPropertyInfo()
        {
            return typeof(BlockProcessor).GetProperty("Parsers", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                   ?? throw new InvalidOperationException("Could not find 'Parsers' property on BlockProcessor.");
        }
    }
}
