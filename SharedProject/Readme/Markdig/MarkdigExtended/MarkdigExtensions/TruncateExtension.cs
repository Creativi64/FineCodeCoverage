using Markdig.Renderers;
using Markdig;

namespace FineCodeCoverage.Readme
{
    public class TruncateExtension : IMarkdownExtension
    {
        private readonly string matchText;

        public TruncateExtension(string matchText) => this.matchText = matchText;
        public void Setup(MarkdownPipelineBuilder pipeline)
            => pipeline.BlockParsers.Insert(0, new TruncateParser(this.matchText));

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) { }
    }
}
