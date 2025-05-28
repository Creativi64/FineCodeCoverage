using System;
using System.Windows.Documents;
using Markdig;
using Markdig.Helpers;
using Markdig.Renderers;

namespace FineCodeCoverage.Readme
{
    internal class ReadmeMarkerTableReplacerMarkdownExtension : IMarkdownExtension
    {
        private readonly string marker;
        private readonly Func<Table> tableCreator;

        public ReadmeMarkerTableReplacerMarkdownExtension(string marker, Func<Table> tableCreator)
        {
            this.marker = marker;
            this.tableCreator = tableCreator;
        }

        public void Setup(MarkdownPipelineBuilder pipeline) => pipeline.BlockParsers.AddIfNotAlready<MarkerBlockParser>();

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) =>
            renderer.ObjectRenderers.AddIfNotAlready(
                () => new MarkerBlockRenderer(marker, () => new Block[] { this.tableCreator() })
            );
    }
}
