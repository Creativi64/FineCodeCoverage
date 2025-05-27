using System;
using System.Windows.Documents;
using Markdig;

namespace FineCodeCoverage.Readme
{
    internal interface IReadMePipelineProvider
    {
        MarkdownPipeline Provide(string marker, Func<Table> tableCreator);
    }
}
