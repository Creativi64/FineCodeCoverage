using AutoMoq;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using Moq;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Options;
using FineCodeCoverage.Engine.ReportGenerator;
using System.Collections.Generic;
using System;

namespace FineCodeCoverageTests.Editor.DynamicCoverage.BufferLineCoverageTests
{
    internal class TestCoberturaLIne : ICoberturaLine
    {
        public TestCoberturaLIne(int number, CoverageType coverageType = CoverageType.Covered)
        {
            Number = number;
            CoverageType = coverageType;
        }
        public int Number { get; }
        public CoverageType CoverageType { get; }
    }

    internal abstract class FilePath
    {
        public const string Value = "filepath";
    }

    internal static class TextContentChangedEventArgsCreator
    {
        class NormalizedTextChangeCollection : List<ITextChange>, INormalizedTextChangeCollection
        {
            public bool IncludesLineChanges => throw new NotImplementedException();
        }
        public static TextContentChangedEventArgs Create(ITextSnapshot afterSnapshot, params Span[] newSpans)
        {
            var normalizedTextChangeCollection = new NormalizedTextChangeCollection();
            foreach (var newSpan in newSpans)
            {
                var mockTextChange = new Mock<ITextChange>();
                mockTextChange.SetupGet(textChange => textChange.NewSpan).Returns(newSpan);
                normalizedTextChangeCollection.Add(mockTextChange.Object);
            }

            var mockBeforeSnapshot = new Mock<ITextSnapshot>();
            mockBeforeSnapshot.Setup(snapshot => snapshot.Version.Changes).Returns(normalizedTextChangeCollection);
            return new TextContentChangedEventArgs(mockBeforeSnapshot.Object, afterSnapshot, EditOptions.None, null);
        }
    }
    internal class TextInfoMocks
    {
        public TextInfoMocks(Mock<ITextView> textView, Mock<ITextBuffer2> textBuffer, Mock<ITextSnapshot> textSnapshot, Mock<ITextInfo> textInfo)
        {
            TextView = textView;
            TextBuffer = textBuffer;
            TextSnapshot = textSnapshot;
            TextInfo = textInfo;
        }
        public Mock<ITextView> TextView { get; }
        public Mock<ITextBuffer2> TextBuffer { get; }
        public Mock<ITextSnapshot> TextSnapshot { get; }
        public Mock<ITextInfo> TextInfo { get; }

    }
    internal class SimpleSetupInfo
    {
        public SimpleSetupInfo(AutoMoqer autoMoqer, BufferLineCoverage bufferLineCoverage, TextInfoMocks textInfoMocks) {
            AutoMoqer = autoMoqer;
            BufferLineCoverage = bufferLineCoverage;
            TextInfoMocks = textInfoMocks;
        }

        public AutoMoqer AutoMoqer { get; }
        public BufferLineCoverage BufferLineCoverage { get; }
        public TextInfoMocks TextInfoMocks { get; }
        
    }
    internal static class SimpleSetup
    {
        public static TextInfoMocks MockTextInfo(Mock<ITextInfo> mockTextInfo = null)
        {
            mockTextInfo = mockTextInfo ?? new Mock<ITextInfo>();
            var mockTextView = new Mock<ITextView>();
            var mockTextBuffer = new Mock<ITextBuffer2>();
            var mockTextSnapshot = new Mock<ITextSnapshot>();
            var textSnapshot = mockTextSnapshot.Object;
            mockTextBuffer.Setup(textBuffer => textBuffer.CurrentSnapshot).Returns(textSnapshot);
            mockTextInfo.SetupGet(textInfo => textInfo.TextBuffer).Returns(mockTextBuffer.Object);
            mockTextInfo.SetupGet(textInfo => textInfo.TextView).Returns(mockTextView.Object);
            mockTextInfo.SetupGet(textInfo => textInfo.FilePath).Returns(FilePath.Value);
            return new TextInfoMocks(mockTextView, mockTextBuffer, mockTextSnapshot, mockTextInfo);
        }

        public static void MockEditorCoverageColouringMode(
            AutoMoqer autoMoqer,
            EditorCoverageColouringMode editorCoverageColouringMode = EditorCoverageColouringMode.UseRoslynWhenTextChanges
        )
        {
            autoMoqer.Setup<IOptionsProvider<EditorCoverageColouringOptions>, EditorCoverageColouringOptions>(p => p.Get())
                .Returns(new EditorCoverageColouringOptions { EditorCoverageColouringMode = editorCoverageColouringMode });
        }

        public static SimpleSetupInfo Setup(
            EditorCoverageColouringMode editorCoverageColouringMode = EditorCoverageColouringMode.UseRoslynWhenTextChanges
        )
        {
            var autoMoqer = new AutoMoqer();
            MockEditorCoverageColouringMode(autoMoqer, editorCoverageColouringMode);
            var textInfoMocks = MockTextInfo(autoMoqer.GetMock<ITextInfo>());

            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();
            return new SimpleSetupInfo(autoMoqer, bufferLineCoverage, textInfoMocks);
        }
    }

    internal static class SetupNewCoverageLinesMessage
    {
        public static NewCoverageLinesMessage Setup(IFileLines fileLines)
        {
            var mockFileLineCoverage = new Mock<IFileLineCoverage>();
            mockFileLineCoverage.Setup(fileLineCoverage => fileLineCoverage.GetLines(FilePath.Value)).Returns(fileLines);
            return new NewCoverageLinesMessage(mockFileLineCoverage.Object);
        }
    }

    internal class CoverageLinesSetupInfo : SimpleSetupInfo
    {
        public CoverageLinesSetupInfo(SimpleSetupInfo simpleSetupInfo, NewCoverageLinesMessage newCoverageLinesMessage, Mock<ITrackedLines> mockTrackedLines) : base(
            simpleSetupInfo.AutoMoqer, simpleSetupInfo.BufferLineCoverage,simpleSetupInfo.TextInfoMocks
            )
        {
            NewCoverageLinesMessage = newCoverageLinesMessage;
            MockTrackedLines = mockTrackedLines;
        }

        public NewCoverageLinesMessage NewCoverageLinesMessage { get; }
        public Mock<ITrackedLines> MockTrackedLines { get; }
    }

    
    internal static class SetupForCoverageLines
    {
        public static CoverageLinesSetupInfo Setup(EditorCoverageColouringMode editorCoverageColouringMode = EditorCoverageColouringMode.UseRoslynWhenTextChanges)
        {
            var simpleSetup = SimpleSetup.Setup(editorCoverageColouringMode);

            var coberturaLines = new List<ICoberturaLine> { new TestCoberturaLIne(1) };
            var mockFileLines = new Mock<IFileLines>();
            mockFileLines.SetupGet(fileLines => fileLines.Lines).Returns(coberturaLines);
            var newCoverageLinesMessage = SetupNewCoverageLinesMessage.Setup(mockFileLines.Object);

            var mockTrackedLines = new Mock<ITrackedLines>();
            
            var dynamicLines = new List<IDynamicLine> { new Mock<IDynamicLine>().Object };
            mockTrackedLines.Setup(trackedLines => trackedLines.GetLines(It.IsAny<int>(), It.IsAny<int>())).Returns(dynamicLines);

            simpleSetup.AutoMoqer.Setup<ITrackedLinesFactory, ITrackedLines>(trackedLinesFactory => trackedLinesFactory.Create(coberturaLines, simpleSetup.TextInfoMocks.TextSnapshot.Object, FilePath.Value))
                .Returns(mockTrackedLines.Object);

            return new CoverageLinesSetupInfo(simpleSetup, newCoverageLinesMessage, mockTrackedLines);
        }
    }
}
