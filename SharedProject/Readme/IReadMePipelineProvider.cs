using Markdig;
using System;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    internal interface IReadMePipelineProvider
    {
        MarkdownPipeline Provide(string marker, Func<Table> tableCreator);
    }
}
