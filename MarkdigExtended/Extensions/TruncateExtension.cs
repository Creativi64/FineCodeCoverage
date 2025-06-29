using Markdig;
using Markdig.Renderers;

namespace MarkdigExtended.Extensions
{
    public class TruncateExtension : IMarkdownExtension
    {
        private readonly string _matchText;

        public TruncateExtension(string matchText) => _matchText = matchText;

        public void Setup(MarkdownPipelineBuilder pipeline)
            => pipeline.BlockParsers.Insert(0, new TruncateParser(_matchText));

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
        }
    }
}
