using FineCodeCoverage.Readme;
using Markdig;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
    internal class Readme_Tests
    {
        [Test]
        public void Pipeline_Should_Parse_Markers_To_Markers_Blocks()
        {
            var markdownPipeline = new ReadMePipeLineProvider().Provide("Marker", () => null);
            var markdownDocument = Markdown.Parse(@"
Heading 1
{{Marker}}
# Heading 2
", markdownPipeline);
            var heading1 = markdownDocument[0];
            var marker = markdownDocument[1];
            var heading2 = markdownDocument[2];
        }
    }
}
