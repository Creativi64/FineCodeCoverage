using System;
using System.ComponentModel.Composition;
using System.Windows.Documents;
using Markdig;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IReadMePipelineProvider))]
    internal class ReadMePipeLineProvider : IReadMePipelineProvider
    {
        public MarkdownPipeline Provide(string marker, Func<Table> tableCreator)
        {
            var readmeMarkerTableReplacerMarkdownExtension =
                new ReadmeMarkerTableReplacerMarkdownExtension(marker, tableCreator);
            return new MarkdownPipelineBuilder().UseAdvancedExtensions().Use(readmeMarkerTableReplacerMarkdownExtension)
                .Build();
        }
    }
}
