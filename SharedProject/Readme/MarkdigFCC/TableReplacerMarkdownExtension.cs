using System;
using System.Windows.Documents;
using Markdig;
using Markdig.Renderers;
using MarkdigExtended.Extensions;

namespace FineCodeCoverage.Readme
{
    internal sealed class TableReplacerMarkdownExtension : IMarkdownExtension
    {
        private readonly string _marker;
        private readonly Func<Table> _tableCreator;

        public TableReplacerMarkdownExtension(string marker, Func<Table> tableCreator)
        {
            _marker = marker;
            _tableCreator = tableCreator;
        }

        public void Setup(MarkdownPipelineBuilder pipeline) => pipeline.BlockParsers.AddIfNotAlready<MarkerBlockParser>();

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
            => renderer.ObjectRenderers.AddIfNotAlready(
                () => new MarkerBlockRenderer(_marker, () => new Block[] { _tableCreator() }));
    }
}
