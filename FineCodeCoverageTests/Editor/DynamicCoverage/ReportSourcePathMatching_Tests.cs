using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Editor.DynamicCoverage.Management;
using FineCodeCoverage.Editor.DynamicCoverage.Utilities;
using FineCodeCoverage.VSAbstractions.Files;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests.Editor.DynamicCoverage
{
    internal class ReportSourcePathMatching_Tests
    {
        private const string ReportPath = @"E:\Projects\Sample\Calculator.cs";
        private const string EditorPath = @"e:\projects\sample\calculator.cs";

        private static ReportFileLineCoverage CreateCoverage(params (string Path, int Line)[] files)
        {
            var classes = new List<IClass>();
            foreach (var file in files)
            {
                var element = new Mock<ICodeElement>();
                element.SetupGet(e => e.Lines).Returns(new List<ICoberturaLine> { new Line(file.Line) });
                var sourceClass = new Mock<IClass>();
                sourceClass.SetupGet(c => c.FileCodeElements).Returns(
                    new Dictionary<string, IReadOnlyList<ICodeElement>>
                    {
                        { file.Path, new List<ICodeElement> { element.Object } }
                    });
                classes.Add(sourceClass.Object);
            }

            var assembly = new Mock<IAssembly>();
            assembly.SetupGet(a => a.Classes).Returns(classes);
            return new ReportFileLineCoverage(new[] { assembly.Object }, Mock.Of<IDateTimeService>());
        }

        [Test]
        public void Should_Find_Coverage_When_Editor_Path_Casing_Differs_From_Report()
        {
            var coverage = CreateCoverage((ReportPath, 2));

            var lines = coverage.GetLines(EditorPath);

            Assert.That(lines, Is.Not.Null);
            Assert.That(lines.Lines.Select(line => line.Number), Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void Should_Share_Tracked_Line_Cache_Across_Path_Casing()
        {
            var coverage = CreateCoverage((ReportPath, 2));
            var lines = coverage.GetLines(ReportPath);

            Assert.That(coverage.GetLines(EditorPath), Is.SameAs(lines));
        }

        [Test]
        public void Should_Merge_Code_Elements_With_Different_Source_Path_Casing()
        {
            var coverage = CreateCoverage((ReportPath, 2), (EditorPath, 5));

            Assert.That(coverage.GetLines(ReportPath).Lines.Select(line => line.Number),
                Is.EqualTo(new[] { 2, 5 }));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Invalidate_All_Path_Casings_When_Coverage_Is_Out_Of_Date(bool cached)
        {
            var coverage = CreateCoverage((ReportPath, 2));
            if (cached)
            {
                Assert.That(coverage.GetLines(ReportPath), Is.Not.Null);
            }

            coverage.OutOfDate(EditorPath);

            Assert.That(coverage.GetLines(ReportPath), Is.Null);
            Assert.That(coverage.GetLines(EditorPath), Is.Null);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Rename_Coverage_When_Old_Path_Casing_Differs(bool cached)
        {
            var coverage = CreateCoverage((ReportPath, 2));
            var previousLines = cached ? coverage.GetLines(ReportPath) : null;
            const string renamedPath = @"E:\Projects\Sample\Renamed.cs";

            coverage.FilesRenamed(new[] { new FileRename(EditorPath, renamedPath) });

            Assert.That(coverage.GetLines(ReportPath), Is.Null);
            var renamedLines = coverage.GetLines(renamedPath.ToLowerInvariant());
            Assert.That(renamedLines, Is.Not.Null);
            Assert.That(renamedLines.Lines.Select(line => line.Number), Is.EqualTo(new[] { 2 }));
            if (cached)
            {
                Assert.That(renamedLines, Is.SameAs(previousLines));
            }
        }

        [Test]
        public void Should_Not_Return_Coverage_For_A_Different_File()
        {
            var coverage = CreateCoverage((ReportPath, 2));

            Assert.That(coverage.GetLines(@"E:\Projects\Other\Calculator.cs"), Is.Null);
        }
    }
}
