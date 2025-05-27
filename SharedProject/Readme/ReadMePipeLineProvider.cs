using System;
using System.ComponentModel.Composition;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IReadMePipelineProvider))]
    internal class ReadMePipeLineProvider : IReadMePipelineProvider
    {
        // still need the fixup links
        public MarkdownPipeline Provide(string marker, Func<Table> tableCreator)
        {
            // do I need advanced now that replacing with a Table ?
            var readmeMarkerTableReplacerMarkdownExtension =
                new ReadmeMarkerTableReplacerMarkdownExtension(marker, tableCreator);
            return new MarkdownPipelineBuilder().UseAdvancedExtensions().Use(readmeMarkerTableReplacerMarkdownExtension)
                .Build();
        }
    }
}
