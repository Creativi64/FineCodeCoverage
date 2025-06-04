using System;
using System.ComponentModel.Composition;
using System.Windows.Documents;
using Markdig;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IReadMePipelineProvider))]
    internal class ReadMePipeLineProvider : IReadMePipelineProvider
    {
        public MarkdownPipeline Provide(string marker, string truncateMatch, Func<Table> tableCreator)
            => new MarkdownPipelineBuilder()
                .Use(new TableReplacerMarkdownExtension(marker, tableCreator))
                .Use(new TruncateExtension(truncateMatch))
                .Build();
    }
}
