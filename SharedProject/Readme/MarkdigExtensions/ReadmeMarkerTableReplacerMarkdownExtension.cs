using System;
using System.Windows.Documents;
using Markdig;
using Markdig.Renderers;

namespace FineCodeCoverage.Readme
{
    class ReadmeMarkerTableReplacerMarkdownExtension : IMarkdownExtension
    {
        private readonly string marker;
        private readonly Func<Table> tableCreator;

        public ReadmeMarkerTableReplacerMarkdownExtension(string marker, Func<Table> tableCreator)
        {
            this.marker = marker;
            this.tableCreator = tableCreator;
        }
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            pipeline.BlockParsers.Add(new MarkerBlockParser());
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            renderer.ObjectRenderers.Add(new MarkerBlockRenderer(marker, tableCreator));
        }
    }

}
