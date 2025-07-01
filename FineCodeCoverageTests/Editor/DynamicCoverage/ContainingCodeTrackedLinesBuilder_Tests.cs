using AutoMoq;
using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Editor.DynamicCoverage.TrackedLinesImpl.Construction;
using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Text;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FineCodeCoverageTests.Editor.DynamicCoverage
{
    class Line : ICoberturaLine
    {
        public Line(int number):this(number, CoverageType.Covered)
        {
        }
        public Line(int number, CoverageType coverageType)
        {
            Number = number;
            CoverageType = coverageType;
        }
        public override bool Equals(object obj)
        {
            var other = obj as ICoberturaLine;
            return other.Number == Number && other.CoverageType == CoverageType;
        }

        [ExcludeFromCodeCoverage]
        public override int GetHashCode()
        {
            int hashCode = 1698846147;
            hashCode = hashCode * -1521134295 + Number.GetHashCode();
            hashCode = hashCode * -1521134295 + CoverageType.GetHashCode();
            return hashCode;
        }

        public int Number { get; }

        public CoverageType CoverageType { get; }
    }

    internal static class TestHelper
    {
        public static CodeSpanRange CodeSpanRangeFromLine(ICoberturaLine line)
        {
            return CodeSpanRange.SingleLine(line.Number - 1);
        }
    }
    
    internal class ContainingCodeTrackedLinesBuilder_Tests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void Should_Have_NewCodeTracker_When_CoverageContentType_Has_LineExcluder(bool hasLineExcluder)
        {
            var mockTextSnapshot = new Mock<ITextSnapshot>();
            mockTextSnapshot.SetupGet(textSnapshot => textSnapshot.ContentType.TypeName).Returns("contenttypename");
            
            var autoMoqer = new AutoMoqer();
            var lineExcluder = new Mock<ILineExcluder>().Object;
            var newCodeTracker = new Mock<INewCodeTracker>().Object;
            var mockNewCodeTrackerFactory = autoMoqer.GetMock<INewCodeTrackerFactory>();
            mockNewCodeTrackerFactory.Setup(newCodeTrackerFactory => newCodeTrackerFactory.Create(lineExcluder)).Returns(newCodeTracker);

            var mockContainingCodeTrackedLinesFactory = autoMoqer.GetMock<IContainingCodeTrackedLinesFactory>();
            var trackedLinesFromFactory = new Mock<IContainingCodeTrackerTrackedLines>().Object;

            mockContainingCodeTrackedLinesFactory.Setup(containingCodeTrackedLinesFactory => containingCodeTrackedLinesFactory.Create(
                It.IsAny<List<IContainingCodeTracker>>(),
                hasLineExcluder ? newCodeTracker : null,
                It.IsAny<IFileCodeSpanRangeService>()
                )).Returns(trackedLinesFromFactory);
            var mockCoverageContentType = new Mock<ICoverageContentType>();
            mockCoverageContentType.Setup(coverageContentType => coverageContentType.ContentTypeName).Returns("contenttypename");
            if(hasLineExcluder)
            {
                mockCoverageContentType.SetupGet(coverageContentType => coverageContentType.LineExcluder).Returns(lineExcluder);
            }
            autoMoqer.SetInstance(new ICoverageContentType[] { mockCoverageContentType.Object });

            var containingCodeTrackedLinesBuilder = autoMoqer.Create<ContainingCodeTrackedLinesBuilder>();

            containingCodeTrackedLinesBuilder.Create(new List<ICoberturaLine> {}, mockTextSnapshot.Object,"");

            mockContainingCodeTrackedLinesFactory.VerifyAll();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Use_CoverageContentType_FileCodeSpanRangeService_When_UseFileCodeSpanRangeServiceForChanges(bool useFileCodeSpanRangeServiceForChanges)
        {
            var mockTextSnapshot = new Mock<ITextSnapshot>();
            mockTextSnapshot.SetupGet(textSnapshot => textSnapshot.ContentType.TypeName).Returns("contenttypename");

            var autoMoqer = new AutoMoqer();
            var fileCodeSpanRangeService = new Mock<IFileCodeSpanRangeService>().Object;
            var mockContainingCodeTrackedLinesFactory = autoMoqer.GetMock<IContainingCodeTrackedLinesFactory>();
            var trackedLinesFromFactory = new Mock<IContainingCodeTrackerTrackedLines>().Object;
            mockContainingCodeTrackedLinesFactory.Setup(containingCodeTrackedLinesFactory => containingCodeTrackedLinesFactory.Create(
                It.IsAny<List<IContainingCodeTracker>>(),
                It.IsAny<INewCodeTracker>(),
                useFileCodeSpanRangeServiceForChanges ? fileCodeSpanRangeService : null
                )).Returns(trackedLinesFromFactory);

            var mockCoverageContentType = new Mock<ICoverageContentType>();
            mockCoverageContentType.SetupGet(coverageContentType => coverageContentType.UseFileCodeSpanRangeServiceForChanges).Returns(useFileCodeSpanRangeServiceForChanges);
            mockCoverageContentType.Setup(coverageContentType => coverageContentType.ContentTypeName).Returns("contenttypename");
            
            mockCoverageContentType.SetupGet(coverageContentType => coverageContentType.FileCodeSpanRangeService).Returns(fileCodeSpanRangeService);
            autoMoqer.SetInstance(new ICoverageContentType[] { mockCoverageContentType.Object });

            var containingCodeTrackedLinesBuilder = autoMoqer.Create<ContainingCodeTrackedLinesBuilder>();

            containingCodeTrackedLinesBuilder.Create(new List<ICoberturaLine> { }, mockTextSnapshot.Object, "");

            mockContainingCodeTrackedLinesFactory.VerifyAll();
        }

        private ITrackedLines CoverageLinesNotWithinSnapshot(bool inSnapshot,string filePath = "", Action<AutoMoqer> additionalSetup = null)
        {
            var line1 = new Line(1);
            var line2 = new Line(inSnapshot ? 2 : 100);

            var mockTextSnapshot = new Mock<ITextSnapshot>();
            mockTextSnapshot.SetupGet(textSnapshot => textSnapshot.ContentType.TypeName).Returns("contenttypename");
            mockTextSnapshot.SetupGet(textSnapshot => textSnapshot.LineCount).Returns(5);
            var autoMoqer = new AutoMoqer();
            autoMoqer.Setup<IContainingCodeTrackedLinesFactory, IContainingCodeTrackerTrackedLines>(
                cctlf => cctlf.Create(It.IsAny<List<IContainingCodeTracker>>(), It.IsAny<INewCodeTracker>(), It.IsAny<IFileCodeSpanRangeService>())).Returns(new Mock<IContainingCodeTrackerTrackedLines>().Object);
            additionalSetup?.Invoke(autoMoqer);
            var trackedLinesFromFactory = new Mock<IContainingCodeTrackerTrackedLines>().Object;

            var mockCoverageContentType = new Mock<ICoverageContentType>();
            mockCoverageContentType.SetupGet(coverageContentType => coverageContentType.ContentTypeName).Returns("contenttypename");
            autoMoqer.SetInstance(new ICoverageContentType[] { mockCoverageContentType.Object });

            var containingCodeTrackedLinesBuilder = autoMoqer.Create<ContainingCodeTrackedLinesBuilder>();

            return containingCodeTrackedLinesBuilder.Create(new List<ICoberturaLine> { line1, line2 }, mockTextSnapshot.Object, filePath);

        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Create_Null_TrackedLines__When_Coverage_Lines_Not_Within_TextSnapshot(bool inSnapshot)
        {
            var trackedLines = CoverageLinesNotWithinSnapshot(inSnapshot);
            if (inSnapshot)
            {
                Assert.IsNotNull(trackedLines);
            }
            else
            {
                Assert.IsNull(trackedLines);
            }
        }

        [Test]
        public void Should_Log_When_Coverage_Lines_Not_Within_TextSnapshot()
        {
            var mockLogger = new Mock<ILogger>();

            _ = CoverageLinesNotWithinSnapshot(false, "filepath", autoMoqer => autoMoqer.SetInstance(mockLogger.Object));

            mockLogger.Verify(logger => logger.LogFileAndForget("Not creating editor marks for filepath as some coverage lines are outside the text snapshot"));
        }


     
    }

    internal class ContainingCodeTrackedLinesBuilder_ContentType_FileLineCoverageService_Tests
    {
        [Test]
        public void Should_Create_CoverageLines_ContainingCodeTracker_For_Each_Line_When_Null_CodeSpanRanges()
        {
            var line1 = new Line(1);
            var line2 = new Line(2);

            var mockTextSnapshot = new Mock<ITextSnapshot>();
            mockTextSnapshot.SetupGet(textSnapshot => textSnapshot.ContentType.TypeName).Returns("contenttypename");
            mockTextSnapshot.SetupGet(textSnapshot => textSnapshot.LineCount).Returns(2);
            var autoMoqer = new AutoMoqer();
            var mockCodeSpanRangeContainingCodeTrackerFactory = autoMoqer.GetMock<ICodeSpanRangeContainingCodeTrackerFactory>();
            var containingCodeTracker1 = new Mock<IContainingCodeTracker>().Object;
            var containingCodeTracker2 = new Mock<IContainingCodeTracker>().Object;
            mockCodeSpanRangeContainingCodeTrackerFactory.Setup(
                codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateCoverageLines(
                    mockTextSnapshot.Object, new List<ICoberturaLine> { line1 }, TestHelper.CodeSpanRangeFromLine(line1), SpanTrackingMode.EdgeExclusive)
                ).Returns(containingCodeTracker1);
            mockCodeSpanRangeContainingCodeTrackerFactory.Setup(
               codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateCoverageLines(
                   mockTextSnapshot.Object, new List<ICoberturaLine> { line2 }, TestHelper.CodeSpanRangeFromLine(line2), SpanTrackingMode.EdgeExclusive)
               ).Returns(containingCodeTracker2);
            var mockContainingCodeTrackedLinesFactory = autoMoqer.GetMock<IContainingCodeTrackedLinesFactory>();
            var trackedLinesFromFactory = new Mock<IContainingCodeTrackerTrackedLines>().Object;

            mockContainingCodeTrackedLinesFactory.Setup(containingCodeTrackedLinesFactory => containingCodeTrackedLinesFactory.Create(
                new List<IContainingCodeTracker> { containingCodeTracker1, containingCodeTracker2 },
                It.IsAny<INewCodeTracker>(),
                It.IsAny<IFileCodeSpanRangeService>()
                )).Returns(trackedLinesFromFactory);
            var mockCoverageContentType = new Mock<ICoverageContentType>();
            mockCoverageContentType.SetupGet(coverageContentType => coverageContentType.ContentTypeName).Returns("contenttypename");
            mockCoverageContentType.SetupGet(coverageContentType => coverageContentType.FileCodeSpanRangeService).Returns(new Mock<IFileCodeSpanRangeService>().Object);
            autoMoqer.SetInstance(new ICoverageContentType[] { mockCoverageContentType.Object });

            var containingCodeTrackedLinesBuilder = autoMoqer.Create<ContainingCodeTrackedLinesBuilder>();

            var trackedLines = containingCodeTrackedLinesBuilder.Create(new List<ICoberturaLine> { line1, line2 }, mockTextSnapshot.Object, "");
            Assert.That(trackedLines, Is.SameAs(trackedLinesFromFactory));
        }
    
        private struct OtherLineText
        {
            public int LineNumber { get; set; }
            public string Text { get; set; }
        }

        private class DummyFileCodeSpanRangeService : IFileCodeSpanRangeService
        {
            private readonly ITextSnapshot expectedSnapshot;
            private readonly List<CodeSpanRange> codeSpanRanges;

            public DummyFileCodeSpanRangeService(ITextSnapshot expectedSnapshot, List<CodeSpanRange> codeSpanRanges)
            {
                this.expectedSnapshot = expectedSnapshot;
                this.codeSpanRanges = codeSpanRanges;
            }
            public List<CodeSpanRange> GetFileCodeSpanRanges(ITextSnapshot snapshot)
            {
                Assert.That(snapshot, Is.SameAs(expectedSnapshot));
                return codeSpanRanges;
            }
        }

        private void TestCreatesContainingCodeTrackers(
            List<ICoberturaLine> lines,
            bool coverageOnlyFromFileCodeSpanRangeService,
            List<CodeSpanRange> codeSpanRanges,
            int textSnapshotLineCount,
            Action<ITextSnapshot> textSnapshotCallback,
            Action<Mock<ICodeSpanRangeContainingCodeTrackerFactory>> setUpCodeSpanRangeContainingCodeTrackerFactory,
            List<OtherLineText> otherLineTexts,
            List<IContainingCodeTracker> expectedOrderedContainingCodeTrackers
            )
        {
            var mockTextSnapshot = new Mock<ITextSnapshot>();
            mockTextSnapshot.SetupGet(textSnapshot => textSnapshot.LineCount).Returns(textSnapshotLineCount);
            mockTextSnapshot.SetupGet(textSnapshot => textSnapshot.ContentType.TypeName).Returns("contenttypename");
            textSnapshotCallback(mockTextSnapshot.Object);

            var autoMoqer = new AutoMoqer();
            var mockTextSnapshotText = autoMoqer.GetMock<ITextSnapshotText>(MockBehavior.Strict);
            otherLineTexts.ForEach(
                otherLineText => mockTextSnapshotText.Setup(
                    textSnapshotText => textSnapshotText.GetLineText(mockTextSnapshot.Object, otherLineText.LineNumber)
                ).Returns(otherLineText.Text));
            var mockCodeSpanRangeContainingCodeTrackerFactory = autoMoqer.GetMock<ICodeSpanRangeContainingCodeTrackerFactory>();
            setUpCodeSpanRangeContainingCodeTrackerFactory(mockCodeSpanRangeContainingCodeTrackerFactory);

            var mockContainingCodeTrackedLinesFactory = autoMoqer.GetMock<IContainingCodeTrackedLinesFactory>();
            var trackedLinesFromFactory = new Mock<IContainingCodeTrackerTrackedLines>().Object;
            mockContainingCodeTrackedLinesFactory.Setup(
                containingCodeTrackedLinesFactory => containingCodeTrackedLinesFactory.Create(
                    expectedOrderedContainingCodeTrackers,
                    It.IsAny<INewCodeTracker>(),
                    It.IsAny<IFileCodeSpanRangeService>()
                )).Returns(trackedLinesFromFactory);

            var mockCoverageContentType = new Mock<ICoverageContentType>();
            mockCoverageContentType.SetupGet(coverageContentType => coverageContentType.ContentTypeName).Returns("contenttypename");
            mockCoverageContentType.SetupGet(coverageContentType => coverageContentType.CoverageOnlyFromFileCodeSpanRangeService).Returns(coverageOnlyFromFileCodeSpanRangeService);
            mockCoverageContentType.SetupGet(coverageContentType => coverageContentType.FileCodeSpanRangeService).Returns(
                new DummyFileCodeSpanRangeService(mockTextSnapshot.Object,codeSpanRanges));
            autoMoqer.SetInstance(new ICoverageContentType[] { mockCoverageContentType.Object });

            var containingCodeTrackedLinesBuilder = autoMoqer.Create<ContainingCodeTrackedLinesBuilder>();

            var trackedLines = containingCodeTrackedLinesBuilder.Create(lines, mockTextSnapshot.Object, "");

            Assert.That(trackedLines, Is.SameAs(trackedLinesFromFactory));
        }

        [Test]
        public void Should_Create_CoverageLinesTracker_For_Adjusted_Lines_In_CodeSpanRange()
        {
            var coverageLinesTracker = new Mock<IContainingCodeTracker>().Object;

            var line1 = new Line(1);
            var line2 = new Line(2);
            ITextSnapshot textSnapshotForSetup = null;
            TestCreatesContainingCodeTrackers(
                new List<ICoberturaLine> { line1, line2 },
                false,
                new List<CodeSpanRange> { new CodeSpanRange(0,1) },
                2,
                textSnapshot => textSnapshotForSetup = textSnapshot,
                mockCodeSpanRangeContainingCodeTrackerFactory =>
                {
                    mockCodeSpanRangeContainingCodeTrackerFactory.Setup(
                        
                        codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateCoverageLines(
                            textSnapshotForSetup,
                            new List<ICoberturaLine> { line1, line2},
                            new CodeSpanRange(0, 1),
                            SpanTrackingMode.EdgeExclusive)
                            ).Returns(coverageLinesTracker);
                },
                new List<OtherLineText> { },
                new List<IContainingCodeTracker> { coverageLinesTracker}
                );
        }

        [Test]
        public void Should_Create_NotIncludedTracker_For_CodeSpanRange_With_No_Coverage()
        {
            var notIncludedTracker = new Mock<IContainingCodeTracker>().Object;

            ITextSnapshot textSnapshotForSetup = null;
            TestCreatesContainingCodeTrackers(
                new List<ICoberturaLine> { },
                false,
                new List<CodeSpanRange> { new CodeSpanRange(0, 3) },
                4,
                textSnapshot => textSnapshotForSetup = textSnapshot,
                mockCodeSpanRangeContainingCodeTrackerFactory =>
                {
                    mockCodeSpanRangeContainingCodeTrackerFactory.Setup(

                        codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateNotIncluded(
                            textSnapshotForSetup,
                            new CodeSpanRange(0, 3),
                            SpanTrackingMode.EdgeExclusive)
                            ).Returns(notIncludedTracker);
                },
                new List<OtherLineText> { },
                new List<IContainingCodeTracker> { notIncludedTracker }
                );
        }

        [Test]
        public void Should_Create_OtherLinesTracker_For_Lines_Between_CodeSpanRanges_That_Are_Not_Whitespace()
        {
            var coverageLinesRange1 = new CodeSpanRange(0, 1);
            var coverageLinesTracker = new Mock<IContainingCodeTracker>().Object;
            var otherLineRange = new CodeSpanRange(2, 2);
            var otherLineTracker = new Mock<IContainingCodeTracker>().Object;
            var coverageLinesRange2 = new CodeSpanRange(3, 4);
            var coverageLinesTracker2 = new Mock<IContainingCodeTracker>().Object;

            var range1Line = new Line(1);
            var range2Line = new Line(4);
            ITextSnapshot textSnapshotForSetup = null;
            TestCreatesContainingCodeTrackers(
                new List<ICoberturaLine> { range1Line, range2Line },
                false,
                new List<CodeSpanRange> { coverageLinesRange1, coverageLinesRange2 },
                5,
                textSnapshot => textSnapshotForSetup = textSnapshot,
                mockCodeSpanRangeContainingCodeTrackerFactory =>
                {
                    mockCodeSpanRangeContainingCodeTrackerFactory.Setup(

                        codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateCoverageLines(
                            textSnapshotForSetup,
                            new List<ICoberturaLine> { range1Line },
                            coverageLinesRange1,
                            SpanTrackingMode.EdgeExclusive)
                            ).Returns(coverageLinesTracker);

                    mockCodeSpanRangeContainingCodeTrackerFactory.Setup(

                        codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateOtherLines(
                            textSnapshotForSetup,
                            otherLineRange,
                            SpanTrackingMode.EdgeNegative)
                            ).Returns(otherLineTracker);

                    mockCodeSpanRangeContainingCodeTrackerFactory.Setup(

                        codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateCoverageLines(
                            textSnapshotForSetup,
                            new List<ICoberturaLine> { range2Line},
                            coverageLinesRange2,
                            SpanTrackingMode.EdgeExclusive)
                            ).Returns(coverageLinesTracker2);
                },
                new List<OtherLineText> {
                    new OtherLineText { LineNumber = 2, Text = "text" },
                },
                new List<IContainingCodeTracker> { coverageLinesTracker, otherLineTracker, coverageLinesTracker2 }
                );
        }

        [Test]
        public void Should_Create_OtherLinesTracker_For_Each_Line_After_Last_CodeSpanRange_That_Is_Not_Whitespace()
        {
            var coverageLinesTracker = new Mock<IContainingCodeTracker>().Object;
            var otherLineTracker = new Mock<IContainingCodeTracker>().Object;

            var line1 = new Line(1);
            var line2 = new Line(2);
            ITextSnapshot textSnapshotForSetup = null;
            TestCreatesContainingCodeTrackers(
                new List<ICoberturaLine> { line1, line2 },
                false,
                new List<CodeSpanRange> { new CodeSpanRange(0, 1) },
                4,
                textSnapshot => textSnapshotForSetup = textSnapshot,
                mockCodeSpanRangeContainingCodeTrackerFactory =>
                {
                    mockCodeSpanRangeContainingCodeTrackerFactory.Setup(
                        codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateCoverageLines(
                            textSnapshotForSetup,
                            new List<ICoberturaLine> { line1, line2 },
                            new CodeSpanRange(0, 1),
                            SpanTrackingMode.EdgeExclusive)
                            ).Returns(coverageLinesTracker);

                    mockCodeSpanRangeContainingCodeTrackerFactory.Setup(

                        codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateOtherLines(
                            textSnapshotForSetup,
                            CodeSpanRange.SingleLine(2),
                            SpanTrackingMode.EdgeNegative)
                            ).Returns(otherLineTracker);

                },
                new List<OtherLineText> { 
                    new OtherLineText { LineNumber = 2, Text = "text" },
                    new OtherLineText { LineNumber = 3, Text = "    " }
                },
                new List<IContainingCodeTracker> { coverageLinesTracker, otherLineTracker }
                );
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Create_CoverageLinesTracker_For_Each_CoverageLine_Not_In_CodeSpanRange_If_CoverageOnlyFromFileCodeSpanRangeService_Is_False(
            bool coverageOnlyFromFileCodeSpanRangeService
        )
        {
            // before first CodeSpanRange
            var coverageLineNotInRangeRange1 = new CodeSpanRange(0, 0);
            var notInRangeCoverageLineTracker1 = new Mock<IContainingCodeTracker>().Object;
            var notInRangeOtherLineTracker1 = new Mock<IContainingCodeTracker>().Object;

            var coverageLinesRange1 = new CodeSpanRange(1, 1);
            var coverageLinesTracker = new Mock<IContainingCodeTracker>().Object;

            // in between CodeSpanRange2
            var coverageLineNotInRangeRange2 = new CodeSpanRange(2, 2);
            var notInRangeCoverageLineTracker2 = new Mock<IContainingCodeTracker>().Object;
            var notInRangeOtherLineTracker2 = new Mock<IContainingCodeTracker>().Object;

            var coverageLinesRange2 = new CodeSpanRange(3, 3);
            var coverageLinesTracker2 = new Mock<IContainingCodeTracker>().Object;

            // after last CodeSpanRange
            var coverageLineNotInRangeRange3 = new CodeSpanRange(4, 4);
            var notInRangeCoverageLineTracker3 = new Mock<IContainingCodeTracker>().Object;
            var notInRangeOtherLineTracker3 = new Mock<IContainingCodeTracker>().Object;

            var notInRangeLine1 = new Line(1);
            var range1Line = new Line(2);
            var notInRangeLine2 = new Line(3);
            var range2Line = new Line(4);
            var notInRangeLine3 = new Line(5);
            ITextSnapshot textSnapshotForSetup = null;
            TestCreatesContainingCodeTrackers(
                new List<ICoberturaLine> { notInRangeLine1, range1Line, notInRangeLine2, range2Line, notInRangeLine3 },
                coverageOnlyFromFileCodeSpanRangeService,
                new List<CodeSpanRange> { coverageLinesRange1, coverageLinesRange2 },
                5,
                textSnapshot => textSnapshotForSetup = textSnapshot,
                mockCodeSpanRangeContainingCodeTrackerFactory =>
                {
                    mockCodeSpanRangeContainingCodeTrackerFactory.Setup(

                        codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateCoverageLines(
                            textSnapshotForSetup,
                            new List<ICoberturaLine> { notInRangeLine1},
                            coverageLineNotInRangeRange1,
                            SpanTrackingMode.EdgeExclusive)
                            ).Returns(notInRangeCoverageLineTracker1);

                    mockCodeSpanRangeContainingCodeTrackerFactory.Setup(

                        codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateOtherLines(
                            textSnapshotForSetup,
                            coverageLineNotInRangeRange1,
                            SpanTrackingMode.EdgeNegative)
                            ).Returns(notInRangeOtherLineTracker1);

                    mockCodeSpanRangeContainingCodeTrackerFactory.Setup(

                        codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateCoverageLines(
                            textSnapshotForSetup,
                            new List<ICoberturaLine> { range1Line },
                            coverageLinesRange1,
                            SpanTrackingMode.EdgeExclusive)
                            ).Returns(coverageLinesTracker);

                    mockCodeSpanRangeContainingCodeTrackerFactory.Setup(

                        codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateCoverageLines(
                            textSnapshotForSetup,
                            new List<ICoberturaLine> { notInRangeLine2},
                            coverageLineNotInRangeRange2,
                            SpanTrackingMode.EdgeExclusive)
                            ).Returns(notInRangeCoverageLineTracker2);

                    mockCodeSpanRangeContainingCodeTrackerFactory.Setup(

                        codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateOtherLines(
                            textSnapshotForSetup,
                            coverageLineNotInRangeRange2,
                            SpanTrackingMode.EdgeNegative)
                            ).Returns(notInRangeOtherLineTracker2);

                    mockCodeSpanRangeContainingCodeTrackerFactory.Setup(

                        codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateCoverageLines(
                            textSnapshotForSetup,
                            new List<ICoberturaLine> { range2Line },
                            coverageLinesRange2,
                            SpanTrackingMode.EdgeExclusive)
                            ).Returns(coverageLinesTracker2);

                    mockCodeSpanRangeContainingCodeTrackerFactory.Setup(

                        codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateCoverageLines(
                            textSnapshotForSetup,
                            new List<ICoberturaLine> { notInRangeLine3 },
                            coverageLineNotInRangeRange3,
                            SpanTrackingMode.EdgeExclusive)
                            ).Returns(notInRangeCoverageLineTracker3);

                    mockCodeSpanRangeContainingCodeTrackerFactory.Setup(

                        codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateOtherLines(
                            textSnapshotForSetup,
                            coverageLineNotInRangeRange3,
                            SpanTrackingMode.EdgeNegative)
                            ).Returns(notInRangeOtherLineTracker3);
                },
                coverageOnlyFromFileCodeSpanRangeService ? new List<OtherLineText> {
                    new OtherLineText { LineNumber = 0, Text = "text" },
                    new OtherLineText { LineNumber = 2, Text = "text" },
                    new OtherLineText { LineNumber = 4, Text = "text" },
                } : new List<OtherLineText> { },
                coverageOnlyFromFileCodeSpanRangeService ? 
                    new List<IContainingCodeTracker> {
                        notInRangeOtherLineTracker1,
                        coverageLinesTracker,
                        notInRangeOtherLineTracker2,
                        coverageLinesTracker2,
                        notInRangeOtherLineTracker3
                    } : 
                    new List<IContainingCodeTracker> { 
                        notInRangeCoverageLineTracker1, 
                        coverageLinesTracker, 
                        notInRangeCoverageLineTracker2, 
                        coverageLinesTracker2,
                        notInRangeCoverageLineTracker3
                    }
                );
        }
    }

    internal class ContainingCodeTrackedLinesBuilder_ContentType_No_FileLineCoverageService_Tests
    {
        [Test]
        public void Should_Create_CoverageLines_ContainingCodeTracker_For_Each_Line()
        {
            var line1 = new Line(1);
            var line2 = new Line(2);

            var mockTextSnapshot = new Mock<ITextSnapshot>();
            mockTextSnapshot.SetupGet(textSnapshot => textSnapshot.ContentType.TypeName).Returns("contenttypename");
            mockTextSnapshot.SetupGet(textSnapshot => textSnapshot.LineCount).Returns(5);
            var autoMoqer = new AutoMoqer();
            var mockCodeSpanRangeContainingCodeTrackerFactory = autoMoqer.GetMock<ICodeSpanRangeContainingCodeTrackerFactory>();
            var containingCodeTracker1 = new Mock<IContainingCodeTracker>().Object;
            var containingCodeTracker2 = new Mock<IContainingCodeTracker>().Object;
            mockCodeSpanRangeContainingCodeTrackerFactory.Setup(
                codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateCoverageLines(
                    mockTextSnapshot.Object, new List<ICoberturaLine> { line1 }, TestHelper.CodeSpanRangeFromLine(line1), SpanTrackingMode.EdgeExclusive)
                ).Returns(containingCodeTracker1);
            mockCodeSpanRangeContainingCodeTrackerFactory.Setup(
               codeSpanRangeContainingCodeTrackerFactory => codeSpanRangeContainingCodeTrackerFactory.CreateCoverageLines(
                   mockTextSnapshot.Object, new List<ICoberturaLine> { line2 }, TestHelper.CodeSpanRangeFromLine(line2), SpanTrackingMode.EdgeExclusive)
               ).Returns(containingCodeTracker2);
            var mockContainingCodeTrackedLinesFactory = autoMoqer.GetMock<IContainingCodeTrackedLinesFactory>();
            var trackedLinesFromFactory = new Mock<IContainingCodeTrackerTrackedLines>().Object;

            mockContainingCodeTrackedLinesFactory.Setup(containingCodeTrackedLinesFactory => containingCodeTrackedLinesFactory.Create(
                new List<IContainingCodeTracker> { containingCodeTracker1, containingCodeTracker2 },
                It.IsAny<INewCodeTracker>(),
                It.IsAny<IFileCodeSpanRangeService>()
                )).Returns(trackedLinesFromFactory);
            var mockCoverageContentType = new Mock<ICoverageContentType>();
            mockCoverageContentType.SetupGet(coverageContentType => coverageContentType.ContentTypeName).Returns("contenttypename");
            autoMoqer.SetInstance(new ICoverageContentType[] { mockCoverageContentType .Object});

            var containingCodeTrackedLinesBuilder = autoMoqer.Create<ContainingCodeTrackedLinesBuilder>();

            var trackedLines = containingCodeTrackedLinesBuilder.Create(new List<ICoberturaLine> { line1, line2 }, mockTextSnapshot.Object, "");

            Assert.That(trackedLines, Is.SameAs(trackedLinesFromFactory));
        }
    }
}
