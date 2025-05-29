using System;
using System.Windows.Documents;
using Markdig;
using Markdig.Renderers;

namespace FineCodeCoverage.Readme
{
    internal class TableReplacerMarkdownExtension : IMarkdownExtension
    {
        private readonly string marker;
        private readonly Func<Table> tableCreator;

        public TableReplacerMarkdownExtension(string marker, Func<Table> tableCreator)
        {
            this.marker = marker;
            this.tableCreator = tableCreator;
        }

        public void Setup(MarkdownPipelineBuilder pipeline) => pipeline.BlockParsers.AddIfNotAlready<MarkerBlockParser>();

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
            => renderer.ObjectRenderers.AddIfNotAlready(
                () => new MarkerBlockRenderer(this.marker, () => new Block[] { this.tableCreator() })
            );
    }
}
