using FineCodeCoverage.Readme;
using Markdig;
using Markdig.Renderers;
using NUnit.Framework;
using System.Linq;
using System.Threading;

namespace FineCodeCoverageTests
{
    internal class Readme_Tests
    {
        private const string md =
@"# Heading 1
{{Marker}}
# Heading 2";

        [Test]
        public void Pipeline_Should_Parse_Markers_To_Markers_Blocks()
        {
            var markdownPipeline = new ReadMePipeLineProvider().Provide("Marker", () => null);
            var markdownDocument = Markdown.Parse(md, markdownPipeline);
            var blocks = markdownDocument.ToList();
            var heading1 = markdownDocument[0];
            Assert.That(heading1, Is.TypeOf<Markdig.Syntax.HeadingBlock>());
            var marker = markdownDocument[1];
            Assert.That((marker as MarkerBlock).Marker, Is.EqualTo("Marker"));
            var heading2 = markdownDocument[2];
            Assert.That(heading2, Is.TypeOf<Markdig.Syntax.HeadingBlock>());
        }

        [Apartment(ApartmentState.STA)]
        [Test]
        public void Pipeline_Should_Create_Table_From_Marker_Block()
        {
            var table = new System.Windows.Documents.Table();
            var markdownPipeline = new ReadMePipeLineProvider().Provide("Marker", () => table);
            var flowDocument = Markdig.Wpf.Markdown.ToFlowDocument(md, markdownPipeline);
            Assert.That(flowDocument.Blocks.ToList()[1], Is.SameAs(table));
        }
    }
}
