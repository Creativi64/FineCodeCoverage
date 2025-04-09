using AutoMoq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Editor.DynamicCoverage.TrackedLinesImpl.Construction;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Impl;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FineCodeCoverageTests.Editor.DynamicCoverage
{
    internal class NormalizedTextChangeCollection : List<ITextChange>, INormalizedTextChangeCollection
    {
        public bool IncludesLineChanges => throw new NotImplementedException();
    }

    internal class BufferLineCoverage_Tests
    {
        private AutoMoqer autoMoqer;
        private Mock<ITextSnapshot> mockTextSnapshot;
        private Mock<ITextBuffer2> mockTextBuffer;
        private Mock<ITextView> mockTextView;
        private ITextSnapshot textSnapshot;
        private Mock<ITextInfo> mockTextInfo;
        private readonly string filePath = "filepath";

        private ICoberturaLine CreateLine()
        {
            var mockLine = new Mock<ICoberturaLine>();
            mockLine.SetupGet(line => line.Number).Returns(1);
            mockLine.SetupGet(line => line.CoverageType).Returns(CoverageType.Partial);
            return mockLine.Object;
        }

        private IAppOptions CreateAppOptions(bool editorCoverageColouringModeOff)
        {
            var mockAppOptions = new Mock<IAppOptions>();
            mockAppOptions.SetupGet(appOptions => appOptions.EditorCoverageColouringMode)
                .Returns(editorCoverageColouringModeOff ? EditorCoverageColouringMode.Off : EditorCoverageColouringMode.UseRoslynWhenTextChanges);
            return mockAppOptions.Object;
        }

        private void SetupEditorCoverageColouringMode(AutoMoqer autoMoqer,bool off = false)
        {
            autoMoqer.Setup<IAppOptionsProvider, IAppOptions>(appOptionsProvider => appOptionsProvider.Get())
                .Returns(CreateAppOptions(off));
        }

        private void MockTextInfo(Mock<ITextInfo> mockTextInfo)
        {
            this.mockTextInfo = mockTextInfo;
            mockTextView = new Mock<ITextView>();
            mockTextBuffer = new Mock<ITextBuffer2>();
            mockTextSnapshot = new Mock<ITextSnapshot>();
            textSnapshot = mockTextSnapshot.Object;
            mockTextBuffer.Setup(textBuffer => textBuffer.CurrentSnapshot).Returns(textSnapshot);
            mockTextInfo.SetupGet(textInfo => textInfo.TextBuffer).Returns(mockTextBuffer.Object);
            mockTextInfo.SetupGet(textInfo => textInfo.TextView).Returns(mockTextView.Object);
            mockTextInfo.SetupGet(textInfo => textInfo.FilePath).Returns(filePath);
        }

        private void SimpleSetUp(bool editorCoverageOff)
        {
            autoMoqer = new AutoMoqer();
            SetupEditorCoverageColouringMode(autoMoqer, editorCoverageOff);
            MockTextInfo(autoMoqer.GetMock<ITextInfo>());
        }

        private void AddLastFileLineCoverage(Mock<IFileLineCoverage> mockFileLineCoverage)
        {
            autoMoqer.GetMock<ILastCoverage>()
                .SetupGet(lastCoverage => lastCoverage.FileLineCoverage)
                .Returns(mockFileLineCoverage.Object);
        }

        private BufferLineCoverage SetupNoInitialTrackedLines()
        {
            SimpleSetUp(false);
            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();
            Assert.False(bufferLineCoverage.HasCoverage);
            return bufferLineCoverage;
        }

        [Test]
        public void Should_Add_Itself_As_A_Listener()
        {
            SimpleSetUp(true);

            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();

            autoMoqer.Verify<IEventAggregator>(eventAggregator => eventAggregator.AddListener(bufferLineCoverage, null));
        }

        [Test]
        public void Should_Stop_Listening_When_TextView_Closed()
        {
            SimpleSetUp(true);

            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();
            mockTextView.Setup(textView => textView.TextSnapshot.GetText()).Returns("");
            mockTextView.Raise(textView => textView.Closed += null, EventArgs.Empty);

            autoMoqer.Verify<IEventAggregator>(eventAggregator => eventAggregator.RemoveListener(bufferLineCoverage));
            mockTextView.VerifyRemove(textView => textView.Closed -= It.IsAny<EventHandler>(), Times.Once);
            mockTextBuffer.VerifyRemove(textBuffer => textBuffer.Changed -= It.IsAny<EventHandler<TextContentChangedEventArgs>>(), Times.Once);
            var mockAppOptionsProvider = autoMoqer.GetMock<IAppOptionsProvider>();
            mockAppOptionsProvider.VerifyRemove(appOptionsProvider => appOptionsProvider.OptionsChanged -= It.IsAny<Action<IAppOptions>>(), Times.Once);
        }

        [Test]
        public void Should_Delegate_GetLines_To_Tracked_Lines()
        {
            SimpleSetUp(false);

            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();
            var mockTrackedLines = RaiseNewCoverageLinesMessage(bufferLineCoverage);
            var dynamicLines = new List<IDynamicLine> { new Mock<IDynamicLine>().Object };
            mockTrackedLines.Setup(trackedLines => trackedLines.GetLines(0, 5)).Returns(dynamicLines);

            Assert.That(bufferLineCoverage.GetLines(0, 5), Is.SameAs(dynamicLines));
        }

        [Test]
        public void Should_Return_Empty_Enumerable_If_No_Tracked_Lines()
        {
            SimpleSetUp(false);

            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();

            Assert.IsEmpty(bufferLineCoverage.GetLines(0, 5).ToList());
        }

        #region creating from existing coverage when file opens

        [Test]
        public void Should_Create_TrackedLines_From_Serialized_Coverage_If_Present_And_Not_Out_Of_Date()
        {
            SimpleSetUp(false);

            DateTime lastWriteDate = new DateTime(2024, 5, 8);
            DateTime textExecutionStartingDate = new DateTime(2024, 5, 10);
            DateTime serializedDate = new DateTime(2024, 5, 8);

            mockTextInfo.Setup(textInfo => textInfo.GetLastWriteTime()).Returns(lastWriteDate);
            var mockLastCoverage = autoMoqer.GetMock<ILastCoverage>();
            mockLastCoverage.SetupGet(lastCoverage => lastCoverage.TestExecutionStartingDate).Returns(textExecutionStartingDate);
            mockLastCoverage.SetupGet(lastCoverage => lastCoverage.FileLineCoverage).Returns(new Mock<IFileLineCoverage>().Object);

            autoMoqer.Setup<IDynamicCoverageStore, SerializedCoverageWhen>(dynamicCoverageStore => dynamicCoverageStore.GetSerializedCoverage(filePath))
                .Returns(new SerializedCoverageWhen { Serialized = "serialized", When = serializedDate });

            var mockTrackedLinesFactory = autoMoqer.GetMock<ITrackedLinesFactory>();
            mockTrackedLinesFactory.Setup(
                trackedLinesFactory => trackedLinesFactory.Create("serialized", textSnapshot, filePath)
            ).Returns(new Mock<ITrackedLines>().Object);

            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();

            mockTrackedLinesFactory.VerifyAll();
            Assert.That(bufferLineCoverage.HasCoverage, Is.True);
        }

        [Test]
        public void Should_Not_Create_TrackedLines_When_Existing_Coverage_When_LastWriteTime_After_LastTestExecutionStarting_When_No_Serialized_Coverage()
        {
            SimpleSetUp(false);
            mockTextInfo.Setup(textInfo => textInfo.GetLastWriteTime()).Returns(new DateTime(2024, 5, 10));
            var mockLastCoverage = autoMoqer.GetMock<ILastCoverage>();
            mockLastCoverage.SetupGet(lastCoverage => lastCoverage.TestExecutionStartingDate).Returns(new DateTime(2024, 5, 9));
            mockLastCoverage.SetupGet(lastCoverage => lastCoverage.FileLineCoverage).Returns(new Mock<IFileLineCoverage>().Object);

            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();

            Assert.IsFalse(bufferLineCoverage.HasCoverage);
        }

        private BufferLineCoverage SerializedCoverageisOutOfDate()
        {
            SimpleSetUp(false);

            DateTime lastWriteDate = new DateTime(2024, 5, 9);
            DateTime textExecutionStartingDate = new DateTime(2024, 5, 10);
            DateTime serializedDate = new DateTime(2024, 5, 8);

            mockTextInfo.Setup(textInfo => textInfo.GetLastWriteTime()).Returns(lastWriteDate);
            var mockLastCoverage = autoMoqer.GetMock<ILastCoverage>();
            mockLastCoverage.SetupGet(lastCoverage => lastCoverage.TestExecutionStartingDate).Returns(textExecutionStartingDate);
            mockLastCoverage.SetupGet(lastCoverage => lastCoverage.FileLineCoverage).Returns(new Mock<IFileLineCoverage>().Object);

            autoMoqer.Setup<IDynamicCoverageStore, SerializedCoverageWhen>(dynamicCoverageStore => dynamicCoverageStore.GetSerializedCoverage(filePath))
                .Returns(new SerializedCoverageWhen { Serialized = "serialized", When = serializedDate });

            return autoMoqer.Create<BufferLineCoverage>();
        }

        [Test]
        public void Should_Not_Create_TrackedLines_When_Existing_Coverage_When_Serialized_Coverage_Is_Out_Of_Date()
        {
            var bufferLineCoverage = SerializedCoverageisOutOfDate();

            Assert.IsFalse(bufferLineCoverage.HasCoverage);
        }

        [Test]
        public void Should_Log_When_Not_Creating_TrackedLines_As_Out_Of_Date()
        {
            SerializedCoverageisOutOfDate();

            autoMoqer.Verify<ILogger>(logger => logger.Log($"Not creating editor marks for {filePath} as coverage is out of date"));
        }

        [Test]
        public void Should_Create_TrackedLines_When_No_Serialized_Coverage_And_Not_Out_Of_Date()
        {
            SimpleSetUp(false);

            DateTime lastWriteDate = new DateTime(2024, 5, 9);
            DateTime textExecutionStartingDate = new DateTime(2024, 5, 10);

            mockTextInfo.Setup(textInfo => textInfo.GetLastWriteTime()).Returns(lastWriteDate);
            var mockLastCoverage = autoMoqer.GetMock<ILastCoverage>();
            mockLastCoverage.SetupGet(lastCoverage => lastCoverage.TestExecutionStartingDate).Returns(textExecutionStartingDate);
            var mockFileLineCoverage = new Mock<IFileLineCoverage>();
            mockLastCoverage.SetupGet(lastCoverage => lastCoverage.FileLineCoverage).Returns(mockFileLineCoverage.Object);
            var lines = new List<ICoberturaLine> { new Mock<ICoberturaLine>().Object };
            mockFileLineCoverage.Setup(fileLineCoverage => fileLineCoverage.GetLines(filePath)).Returns(lines);
            var trackedLinesFromSerialized = new Mock<ITrackedLines>().Object;
            var mockTrackedLinesFactory = autoMoqer.GetMock<ITrackedLinesFactory>();
            mockTrackedLinesFactory.Setup(
                trackedLinesFactory => trackedLinesFactory.Create(lines, textSnapshot, filePath)
            ).Returns(trackedLinesFromSerialized);

            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();

            mockTrackedLinesFactory.VerifyAll();
            Assert.True(bufferLineCoverage.HasCoverage);
        }

        [Test]
        public void Should_Log_If_Exception_Creating_TrackedLines()
        {
            var exception = new Exception("exception");

            SimpleSetUp(false);

            var mockFileLineCoverage = new Mock<IFileLineCoverage>();
            mockFileLineCoverage.Setup(fileLineCoverage => fileLineCoverage.GetLines(filePath)).Throws(exception);
            AddLastFileLineCoverage(mockFileLineCoverage);

            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();

            autoMoqer.Verify<ILogger>(logger => logger.Log($"Error creating tracked lines for {filePath}", exception.ToString()));

        }

        [Test]
        public void Should_Not_Create_TrackedLines_If_EditorCoverageColouringMode_Is_Off()
        {
            SimpleSetUp(true); // off

            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();
            
            Assert.IsFalse(bufferLineCoverage.HasCoverage);
        }

        [Test]
        public void Should_Have_HasCoverage_False_If_No_Initial_Coverage()
        {

            MockTextInfo(new Mock<ITextInfo>());
            var bufferLineCoverage = new BufferLineCoverage(
                null,
                mockTextInfo.Object,
                new Mock<IEventAggregator>().Object,
                null,
                null,
                new Mock<IAppOptionsProvider>().Object,
                new CoverageContentTypes(new ICoverageContentType[] { }),
                null
                );

            Assert.False(bufferLineCoverage.HasCoverage);
        }
        
        
        
        #endregion

        #region NewCoverageLinesMessge 

        [Test]
        public void Should_HasCoverage_False_After_Sending_CoverageChangedMessage_When_Coverage_Cleared()
        {
            SimpleSetUp(false);
            autoMoqer.Setup<ITrackedLinesFactory, ITrackedLines>(
                tlf => tlf.Create(It.IsAny<List<ICoberturaLine>>(), It.IsAny<ITextSnapshot>(), It.IsAny<string>()))
                .Returns(new Mock<ITrackedLines>().Object);
            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();
            bufferLineCoverage.Handle(new NewCoverageLinesMessage(new Mock<IFileLineCoverage>().Object));
            Assert.That(bufferLineCoverage.HasCoverage, Is.True);
            
            var mockEventAggregator = autoMoqer.GetMock<IEventAggregator>();
            
            mockEventAggregator.Setup(eventAggregator => eventAggregator.SendMessage(
                new CoverageChangedMessage(filePath,null),
                null
                )).Callback(() => Assert.That(bufferLineCoverage.HasCoverage, Is.False));

            bufferLineCoverage.Handle(new ClearLinesMessage());

            mockEventAggregator.VerifyAll();
        }

        [Test]
        public void Should_Not_Send_CoverageChangedMessage_For_Coverage_Clearing_If_Not_Tracking()
        {
            SimpleSetUp(false);
            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();

            bufferLineCoverage.Handle(new ClearLinesMessage());

            autoMoqer.Verify<IEventAggregator>(eventAggregator => eventAggregator.SendMessage(
                It.IsAny<CoverageChangedMessage>(),
                null
                ),Times.Never());

        }

        private Mock<ITrackedLines> RaiseNewCoverageLinesMessage(BufferLineCoverage bufferLineCoverage)
        {
            var mockTrackedLines = new Mock<ITrackedLines>();
            var newCoverageLines = new List<ICoberturaLine> { CreateLine() };
            autoMoqer.Setup<ITrackedLinesFactory, ITrackedLines>(
                trackedLinesFactory => trackedLinesFactory.Create(newCoverageLines, textSnapshot, filePath)
                ).Returns(mockTrackedLines.Object);

            var mockFileLineCoverage = new Mock<IFileLineCoverage>();
            mockFileLineCoverage.Setup(fileLineCoverage => fileLineCoverage.GetLines(filePath)).Returns(newCoverageLines);
            bufferLineCoverage.Handle(
                new NewCoverageLinesMessage(mockFileLineCoverage.Object));

            return mockTrackedLines;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Create_New_TextLines_From_New_Coverage_And_Not_EditorCoverageColouringModeOff_In_AppOptions(bool editorCoverageColouringModeOff)
        {
            // no initial tracked lines
            SimpleSetUp(true);
            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();

           
            var appOptionsOn = CreateAppOptions(false);
            autoMoqer.GetMock<IAppOptionsProvider>()
                .Raise(appOptionsProvider => appOptionsProvider.OptionsChanged += null, CreateAppOptions(editorCoverageColouringModeOff));


            RaiseNewCoverageLinesMessage(bufferLineCoverage);

            Assert.That(bufferLineCoverage.HasCoverage, Is.EqualTo(!editorCoverageColouringModeOff));
            autoMoqer.Verify<IEventAggregator>(eventAggregator => eventAggregator.SendMessage(
                    new CoverageChangedMessage(filePath, null), null), Times.Exactly(editorCoverageColouringModeOff ? 0 : 1));

            
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Not_Create_TrackedLines_From_NewCoverageLinesMessage_If_Text_Changed_Since_TestExecutionStartingMessage(
            bool textChangedSinceTestExecutionStarting
        )
        {
            var bufferLineCoverage = SetupNoInitialTrackedLines();

            var mockTrackedLinesFactory = autoMoqer.GetMock<ITrackedLinesFactory>();
            mockTrackedLinesFactory.Setup(trackedLinesFactory => trackedLinesFactory.Create(It.IsAny<List<ICoberturaLine>>(), textSnapshot, filePath))
                .Returns(new Mock<ITrackedLines>().Object);


            void RaiseChangedOnBackground()
            {
                mockTextBuffer.Raise(textBuffer => textBuffer.ChangedOnBackground += null, new TextContentChangedEventArgs(new Mock<ITextSnapshot>().Object, new Mock<ITextSnapshot>().Object, new EditOptions(), null));
            }
            void RaiseTestExecutionStartingMessage()
            {
                (bufferLineCoverage as IListener<TestExecutionStartingMessage>).Handle(new TestExecutionStartingMessage());
            }
            

            if (textChangedSinceTestExecutionStarting)
            {
                RaiseTestExecutionStartingMessage();
                Thread.Sleep(1);
                RaiseChangedOnBackground();
            } else
            {
                RaiseChangedOnBackground();
                Thread.Sleep(1);
                RaiseTestExecutionStartingMessage();
            }

            bufferLineCoverage.Handle(new NewCoverageLinesMessage(new Mock<IFileLineCoverage>().Object));
           

            Assert.That(bufferLineCoverage.HasCoverage, Is.EqualTo(textChangedSinceTestExecutionStarting ? false : true));

            var expectCoverageChangedMessage = textChangedSinceTestExecutionStarting ? false : true;
            autoMoqer.Verify<IEventAggregator>(eventAggregator => eventAggregator.SendMessage(
                It.IsAny<CoverageChangedMessage>(), null), 
                Times.Exactly(expectCoverageChangedMessage ? 1 : 0));
        }

        [Test]
        public void Should_Log_When_Text_Changed_Since_TestExecutionStartingMessage()
        {
            var bufferLineCoverage = SetupNoInitialTrackedLines();

            (bufferLineCoverage as IListener<TestExecutionStartingMessage>).Handle(new TestExecutionStartingMessage());
            Thread.Sleep(1);
            mockTextBuffer.Raise(textBuffer => textBuffer.ChangedOnBackground += null, new TextContentChangedEventArgs(new Mock<ITextSnapshot>().Object, new Mock<ITextSnapshot>().Object, new EditOptions(), null));

            bufferLineCoverage.Handle(new NewCoverageLinesMessage (new Mock<IFileLineCoverage>().Object));

            autoMoqer.Verify<ILogger>(ILogger => ILogger.Log($"Not creating editor marks for {filePath} as it was changed after test execution started"));
        }
        #endregion

        
        #region updating when text changes on background
        [TestCase(10, new int[] { -1, 9, 10 }, new int[] { 9 })]
        [TestCase(10, new int[] { 9, 10 }, new int[] {})]
        public void Should_Update_TrackedLines_When_Text_Buffer_ChangedOnBackground_And_Send_CoverageChangedMessage_If_Any_Changed_Within_Snapshot(
            int afterLineCount,
            int[] changedLineNumbers,
            int[] expectedMessageChangedLineNumbers
            )
        {
            SimpleSetUp(false);

            var mockAfterSnapshot = new Mock<ITextSnapshot>();
            mockAfterSnapshot.SetupGet(afterSnapshot => afterSnapshot.LineCount).Returns(afterLineCount);

            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();

            var mockTrackedLines = RaiseNewCoverageLinesMessage(bufferLineCoverage);
            var newSpan = new Span(0, 1);
            mockTrackedLines.Setup(
                trackedLines => trackedLines.GetChangedLineNumbers(mockAfterSnapshot.Object, new List<Span> { newSpan })
            ).Returns(changedLineNumbers);

            mockTextBuffer.Raise(textBuffer => textBuffer.ChangedOnBackground += null, CreateTextContentChangedEventArgs(mockAfterSnapshot.Object, newSpan));
            
            autoMoqer.Verify<IEventAggregator>(
                eventAggregator => eventAggregator.SendMessage(
                    It.Is<CoverageChangedMessage>(message =>
                        message.FilePath == filePath &&
                        message.ChangedLineNumbers != null && message.ChangedLineNumbers.SequenceEqual(expectedMessageChangedLineNumbers))
                    , null
                ), expectedMessageChangedLineNumbers.Length > 0 ? Times.Once() : Times.Never());

        }

        [Test]
        public void Should_Log_When_Exception_Updating_TrackedLines_When_Text_Buffer_ChangedOnBackground()
        {
            SimpleSetUp(false);
            
            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();
            var mockTrackedLines = RaiseNewCoverageLinesMessage(bufferLineCoverage);

            var exception = new Exception("message");
            mockTrackedLines.Setup(
                trackedLines => trackedLines.GetChangedLineNumbers(It.IsAny<ITextSnapshot>(), It.IsAny<List<Span>>())
            ).Throws(exception);

            mockTextBuffer.Raise(textBuffer => textBuffer.ChangedOnBackground += null, CreateTextContentChangedEventArgs(new Mock<ITextSnapshot>().Object, new Span(0,1)));

            autoMoqer.Verify<ILogger>(logger => logger.Log($"Error updating tracked lines for {filePath}", exception.ToString()));
        }

        [Test]
        public void Should_Not_Throw_When_Text_Buffer_Changed_And_No_Coverage()
        {
            SimpleSetUp(true);
            
            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();

            mockTextBuffer.Raise(textBuffer => textBuffer.Changed += null, CreateTextContentChangedEventArgs(new Mock<ITextSnapshot>().Object, new Span(0, 0)));
        }
        #endregion
        private TextContentChangedEventArgs CreateTextContentChangedEventArgs(ITextSnapshot afterSnapshot, params Span[] newSpans)
        {
            var normalizedTextChangeCollection = new NormalizedTextChangeCollection();
            foreach (var newSpan in newSpans)
            {
                var mockTextChange = new Mock<ITextChange>();
                mockTextChange.SetupGet(textChange => textChange.NewSpan).Returns(newSpan);
                normalizedTextChangeCollection.Add(mockTextChange.Object);
            };

            var mockBeforeSnapshot = new Mock<ITextSnapshot>();
            mockBeforeSnapshot.Setup(snapshot => snapshot.Version.Changes).Returns(normalizedTextChangeCollection);
            return new TextContentChangedEventArgs(mockBeforeSnapshot.Object, afterSnapshot, EditOptions.None, null);
        }

        #region saving serialized coverage
        [TestCase(true)]
        [TestCase(false)]
        public void Should_SaveSerializedCoverage_When_TextView_Closed_And_Tracking_And_File_System_Reflecting_TrackedLines(
            bool fileSystemReflectsTrackedLines    
        )
        {
            SimpleSetUp(false);
            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();
            
            var snapshotText = "snapshot text";
            var trackedLines = new Mock<ITrackedLines>().Object;
            autoMoqer.Setup<ITrackedLinesFactory, string>(
                trackedLinesFactory => trackedLinesFactory.Serialize(trackedLines, snapshotText)
            ).Returns("serialized");
            autoMoqer.Setup<ITrackedLinesFactory, ITrackedLines>(
               trackedLinesFactory => trackedLinesFactory.Create(It.IsAny<List<ICoberturaLine>>(),It.IsAny<ITextSnapshot>(),It.IsAny<string>())
           ).Returns(trackedLines);
            bufferLineCoverage.Handle(new NewCoverageLinesMessage(new Mock<IFileLineCoverage>().Object));


            mockTextInfo.Setup(textInfo => textInfo.GetFileText())
                .Returns(fileSystemReflectsTrackedLines ? snapshotText : "changes not saved");

            var mockTextViewCurrentSnapshot = new Mock<ITextSnapshot>();
            mockTextViewCurrentSnapshot.Setup(snapshot => snapshot.GetText()).Returns(snapshotText);
            mockTextView.Setup(textView => textView.TextSnapshot).Returns(mockTextViewCurrentSnapshot.Object);

            mockTextView.Raise(textView => textView.Closed += null, EventArgs.Empty);

            autoMoqer.Verify<IDynamicCoverageStore>(
                dynamicCoverageStore => dynamicCoverageStore.SaveSerializedCoverage(filePath, "serialized"), 
                fileSystemReflectsTrackedLines ? Times.Once() : Times.Never());
            
        }

        [Test]
        public void Should_Remove_Serialized_Coverage_When_TextView_Closed_And_No_TrackedLines()
        {
            SimpleSetUp(false);
            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();

            mockTextView.Raise(textView => textView.Closed += null, EventArgs.Empty);

            autoMoqer.Verify<IDynamicCoverageStore>(dynamicCoverageStore => dynamicCoverageStore.RemoveSerializedCoverage(filePath));
        }
        #endregion

        #region app options turn off editor coverage

        [Test]
        public void Should_Remove_TrackedLines_When_AppOptions_Changed_And_EditorCoverageColouringMode_Is_Off()
        {
            SimpleSetUp(false);
            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();
            RaiseNewCoverageLinesMessage(bufferLineCoverage);
            Assert.That(bufferLineCoverage.HasCoverage, Is.True);

            var mockEventAggregator = autoMoqer.GetMock<IEventAggregator>();
            mockEventAggregator.Setup(eventAggregator => eventAggregator.SendMessage(new CoverageChangedMessage(filePath, null), null))
                .Callback(() => Assert.That(bufferLineCoverage.HasCoverage, Is.False));

            var mockAppOptions = new Mock<IAppOptions>();
            mockAppOptions.SetupGet(appOptions => appOptions.EditorCoverageColouringMode).Returns(EditorCoverageColouringMode.Off);
            autoMoqer.GetMock<IAppOptionsProvider>().Raise(appOptionsProvider => appOptionsProvider.OptionsChanged += null, mockAppOptions.Object);

            mockEventAggregator.VerifyAll();
        }

        [Test]
        public void Should_Not_Remove_TrackedLines_When_AppOptions_Changed_And_EditorCoverageColouringMode_Is_Not_Off()
        {
            SimpleSetUp(false);
            var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();
            RaiseNewCoverageLinesMessage(bufferLineCoverage);
            Assert.That(bufferLineCoverage.HasCoverage, Is.True);

            var mockAppOptions = new Mock<IAppOptions>();
            mockAppOptions.SetupGet(appOptions => appOptions.EditorCoverageColouringMode).Returns(EditorCoverageColouringMode.DoNotUseRoslynWhenTextChanges);
            autoMoqer.GetMock<IAppOptionsProvider>().Raise(appOptionsProvider => appOptionsProvider.OptionsChanged += null, mockAppOptions.Object);

            autoMoqer.Verify<IEventAggregator>(eventAggregator => eventAggregator.SendMessage(It.IsAny<CoverageChangedMessage>(), null), Times.Once());

            Assert.That(bufferLineCoverage.HasCoverage, Is.True);
        }

        // todo - should also test that uses the changed value

        #endregion

    }
}
