using System;
using System.Windows.Documents;
using Markdig;

namespace FineCodeCoverage.Readme
{
    internal interface IReadMePipelineProvider
    {
        MarkdownPipeline Provide(string marker, string truncateMatch, Func<Table> tableCreator);
    }
}