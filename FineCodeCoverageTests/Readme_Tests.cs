using FineCodeCoverage.Readme;
using FineCodeCoverage.Readme.MarkdigFCC;
using Markdig;
using MarkdigExtended.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Windows.Documents;

namespace FineCodeCoverageTests
{
    [Apartment(ApartmentState.STA)]
    internal class Readme_Tests
    {
        private Table table = new Table();
        private MarkdownPipeline markdownPipeline;
        private const string marker = "Marker";
        private const string truncateMatch = "## remove this";
        private readonly string md =
$@"# Heading 1

{{{{{marker}}}}}

# Heading 2

{truncateMatch}
";
        [SetUp]
        public void Setup()
        {
            markdownPipeline = new ReadMePipeLineProvider().Provide(marker, truncateMatch, () => table);
        }

        [Test]
        public void Pipeline_Should_Parse_Markers_To_Markers_Blocks()
        {
            var markdownDocument = Markdown.Parse(md, markdownPipeline);

            var heading1 = markdownDocument[0];
            Assert.That(heading1, Is.TypeOf<Markdig.Syntax.HeadingBlock>());
            var markerBlock = markdownDocument[1] as MarkerBlock;
            Assert.That(markerBlock.Marker, Is.EqualTo(marker));
            var heading2 = markdownDocument[2];
            Assert.That(heading2, Is.TypeOf<Markdig.Syntax.HeadingBlock>());
            Assert.That(markdownDocument[3], Is.TypeOf<TruncateBlock>());
            Assert.That(markdownDocument.Count, Is.EqualTo(4));
        }

        [Test]
        public void Pipeline_Should_Create_Table_From_Marker_Block()
        {
            var flowDocument = Markdig.Wpf.Markdown.ToFlowDocument(md, markdownPipeline);
            
            var blocks = flowDocument.Blocks.ToList();
            Assert.That(blocks[0], Is.TypeOf<Paragraph>());
            Assert.That(blocks[1], Is.SameAs(table));
            Assert.That(blocks[2], Is.TypeOf<Paragraph>());
            Assert.That(blocks.Count, Is.EqualTo(3));
        }
    }
}
