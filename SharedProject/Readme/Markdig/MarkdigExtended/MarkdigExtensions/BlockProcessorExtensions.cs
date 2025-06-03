using System.Reflection;
using Markdig.Parsers;

namespace FineCodeCoverage.Readme
{
    public static class BlockProcessorExtensions
    {
        public static void NoFurtherProcessing(this BlockProcessor blockProcessor)
        {
            var emptyBlockParsers = new BlockParserList(new BlockParser[] { });
            typeof(BlockProcessor)
                .GetProperty("Parsers", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .SetValue(blockProcessor, emptyBlockParsers);
        }
    }
}