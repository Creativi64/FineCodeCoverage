using System;
using System.Windows.Documents;
using Markdig;
using Markdig.Renderers;

namespace FineCodeCoverage.Readme
{
    internal class TableReplacerMarkdownExtension : IMarkdownExtension
    {
        private readonly string _marker;
        private readonly Func<Table> _tableCreator;

        public TableReplacerMarkdownExtension(string marker, Func<Table> tableCreator)
        {
            this._marker = marker;
            this._tableCreator = tableCreator;
        }

        public void Setup(MarkdownPipelineBuilder pipeline) => pipeline.BlockParsers.AddIfNotAlready<MarkerBlockParser>();

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
            => renderer.ObjectRenderers.AddIfNotAlready(
                () => new MarkerBlockRenderer(this._marker, () => new Block[] { this._tableCreator() })
            );
    }
}
