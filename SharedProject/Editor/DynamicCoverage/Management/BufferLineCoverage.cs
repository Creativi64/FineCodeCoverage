using System;
using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Impl;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class BufferLineCoverage :
        IBufferLineCoverage,
        IListener<NewCoverageLinesMessage>,
        IListener<TestExecutionStartingMessage>,
        IListener<ClearLinesMessage>
    {
        private readonly ITextInfo textInfo;
        private readonly IEventAggregator eventAggregator;
        private readonly ITrackedLinesFactory trackedLinesFactory;
        private readonly IEditorCoverageColouringOptionsProvider editorCoverageColouringOptionsProvider;
        private readonly ICoverageContentTypes coverageContentTypes;
        private readonly ILogger logger;
        private readonly ITextBuffer2 textBuffer;
        private bool? editorCoverageModeOff;
        private IFileLineCoverage fileLineCoverage;
        private DateTime? textBufferLastChanged;
        private DateTime lastTestExecutionStarting;
        private bool isApplicableContentType = true;
        private IFileLines fileLines;
        private ITrackedLines trackedLines;
        public bool HasCoverage => this.trackedLines != null;

        public BufferLineCoverage(
            ITextInfo textInfo,
            IEventAggregator eventAggregator,
            ITrackedLinesFactory trackedLinesFactory,
            IEditorCoverageColouringOptionsProvider editorCoverageColouringOptionsProvider,
            ICoverageContentTypes coverageContentTypes,
            ILogger logger
        )
        {
            this.textBuffer = textInfo.TextBuffer;
            this.textBuffer.ContentTypeChanged += this.ContentTypeChanged;
            this.textInfo = textInfo;
            this.eventAggregator = eventAggregator;
            this.trackedLinesFactory = trackedLinesFactory;
            this.editorCoverageColouringOptionsProvider = editorCoverageColouringOptionsProvider;
            this.coverageContentTypes = coverageContentTypes;
            this.logger = logger;
            void EditorCoverageColouringOptionsChanged(EditorCoverageColouringOptions editorCoverageColouringOptions)
            {
                bool newEditorCoverageModeOff = editorCoverageColouringOptions.EditorCoverageColouringMode == EditorCoverageColouringMode.Off;
                this.editorCoverageModeOff = newEditorCoverageModeOff;
                if (this.trackedLines != null && newEditorCoverageModeOff)
                {
                    this.trackedLines = null;
                    this.SendCoverageChangedMessage();
                }
            }

            editorCoverageColouringOptionsProvider.OptionsChanged += EditorCoverageColouringOptionsChanged;
            _ = eventAggregator.AddListener(this);
            this.textBuffer.ChangedOnBackground += this.TextBuffer_ChangedOnBackground;
            void textViewClosedHandler(object s, EventArgs e)
            {
                if (s is ITextView textView)
                {
                    this.fileLines?.TextViewClosed();
                }

                this.textBuffer.ChangedOnBackground -= this.TextBuffer_ChangedOnBackground;
                this.textBuffer.ContentTypeChanged -= this.ContentTypeChanged;
                textInfo.TextView.Closed -= textViewClosedHandler;
                editorCoverageColouringOptionsProvider.OptionsChanged -= EditorCoverageColouringOptionsChanged;
                _ = eventAggregator.RemoveListener(this);
            }

            textInfo.TextView.Closed += textViewClosedHandler;
        }

        public void SetLastCoverage(ILastCoverage lastCoverage)
        {
            if (!this.EditorCoverageColouringModeOff())
            {
                this.UseLastCoverageIfHasFileLines(lastCoverage);
            }
        }

        private void UseLastCoverageIfHasFileLines(ILastCoverage lastCoverage)
        {
            this.fileLineCoverage = lastCoverage.FileLineCoverage;
            this.lastTestExecutionStarting = lastCoverage.TestExecutionStartingDate;
            this.fileLines = this.fileLineCoverage.GetLines(this.textInfo.FilePath);
            if(this.fileLines != null)
            {
                this.UseLastCoverage();
            }
        }

        private void UseLastCoverage()
        {
            bool isOutOfDate = this.FileLinesFromLastCoverageIfNotOutOfDate();

            if (isOutOfDate)
            {
                this.logger.LogFileAndForget($"Not creating editor marks for {this.textInfo.FilePath} as coverage is out of date");
                this.fileLineCoverage.OutOfDate(this.textInfo.FilePath);
                this.fileLines = null;
            }
        }

        private bool FileLinesFromLastCoverageIfNotOutOfDate()
        {
            bool isOutOfDate;
            DateTime lastWriteTime = this.textInfo.GetLastWriteTime();
            if (this.fileLines.HasTrackedLines)
            {
                this.trackedLines = this.fileLines.GetTrackedLinesIfNotOutOfDate(lastWriteTime);
                isOutOfDate = this.trackedLines == null;
            }
            else
            {
                isOutOfDate = lastWriteTime > this.lastTestExecutionStarting;
                if (!isOutOfDate)
                {
                    this.TryCreateTrackedLines(this.CreateTrackedLinesFromFileLines);
                }
            }

            return isOutOfDate;
        }

        private void ContentTypeChanged(object sender, ContentTypeChangedEventArgs args)
        {
            // purpose is so do not create tracked lines for a content type that is not applicable when new coverage
            this.isApplicableContentType = this.coverageContentTypes.IsApplicable(args.AfterContentType.TypeName);
            if (this.isApplicableContentType)
            {
                // this currently does not work as Roslyn is not ready.
                // could fallback to single lines but would have to look at other uses of IFileCodeSpanRangeService
                // this is low priority 
                this.CreateTrackedLinesIfRequired();
            }
            else
            {
                this.trackedLines = null;
            }

            this.SendCoverageChangedMessage();
        }

        private void CreateTrackedLinesIfRequired()
        {
            if (this.EditorCoverageColouringModeOff())
            {
                this.trackedLines = null;
            }
            else
            {
                this.TryCreateTrackedLines();
            }
        }

        private void TryCreateTrackedLines() => this.TryCreateTrackedLines(this.CreateTrackedLines);

        private void TryCreateTrackedLines(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                this.logger.LogFileAndForget($"Error creating tracked lines for {this.textInfo.FilePath}", e.ToString());
            }
        }

        private void CreateTrackedLinesIfRequiredWithMessage()
        {
            bool hadTrackedLines = this.trackedLines != null;
            if (!this.textBufferLastChanged.HasValue || this.textBufferLastChanged.Value < this.lastTestExecutionStarting)
            {
                this.CreateTrackedLinesIfRequired();
            }
            else
            {
                this.logger.LogFileAndForget($"Not creating editor marks for {this.textInfo.FilePath} as it was changed after test execution started");
                this.trackedLines = null;
            }

            bool hasTrackedLines = this.trackedLines != null;
            if (hadTrackedLines || hasTrackedLines)
            {
                this.SendCoverageChangedMessage();
            }
        }

        private void CreateTrackedLines()
        {
            this.fileLines = this.fileLineCoverage.GetLines(this.textInfo.FilePath);
            if(this.fileLines == null)
            {
                this.trackedLines = null;
            }
            else
            {
                this.CreateTrackedLinesFromFileLines();
            }
        }

        private void CreateTrackedLinesFromFileLines()
        {
            string filePath = this.textInfo.FilePath;
            ITextSnapshot currentSnapshot = this.textBuffer.CurrentSnapshot;
            this.trackedLines = this.trackedLinesFactory.Create(this.fileLines.Lines, currentSnapshot, filePath);
            this.fileLines.SetTrackedLines(this.trackedLines);
        }

        private bool EditorCoverageColouringModeOff()
        {
            // as handling the event do not need to check the value again
            if (this.editorCoverageModeOff.HasValue)
            {
                return this.editorCoverageModeOff.Value;
            }

            this.editorCoverageModeOff = this.editorCoverageColouringOptionsProvider.Get().EditorCoverageColouringMode == EditorCoverageColouringMode.Off;
            return this.editorCoverageModeOff.Value;
        }

        private void TextBuffer_ChangedOnBackground(object sender, TextContentChangedEventArgs textContentChangedEventArgs)
        {
            this.textBufferLastChanged = DateTime.Now;
            if (this.trackedLines != null)
            {
                this.TryUpdateTrackedLines(textContentChangedEventArgs);
            }
        }

        private void TryUpdateTrackedLines(TextContentChangedEventArgs textContentChangedEventArgs)
        {
            try
            {
                this.UpdateTrackedLines(textContentChangedEventArgs);
            }
            catch (Exception e)
            {
                this.logger.LogFileAndForget($"Error updating tracked lines for {this.textInfo.FilePath}", e.ToString());
            }
        }

        private void UpdateTrackedLines(TextContentChangedEventArgs textContentChangedEventArgs)
        {
            IEnumerable<int> changedLineNumbers = this.trackedLines.GetChangedLineNumbers(
                textContentChangedEventArgs.After,
                textContentChangedEventArgs.Changes.Select(change => change.NewSpan).ToList()
            ).Where(changedLine => changedLine >= 0 && changedLine < textContentChangedEventArgs.After.LineCount);
            this.SendCoverageChangedMessageIfChanged(changedLineNumbers);
        }

        private void SendCoverageChangedMessageIfChanged(IEnumerable<int> changedLineNumbers)
        {
            if (changedLineNumbers.Any())
            {
                this.SendCoverageChangedMessage(changedLineNumbers);
            }
        }

        private void SendCoverageChangedMessage(IEnumerable<int> changedLineNumbers = null)
            => this.eventAggregator.SendMessage(new CoverageChangedMessage(this.textInfo.FilePath, changedLineNumbers));

        public IEnumerable<IDynamicLine> GetLines(int startLineNumber, int endLineNumber)
            => this.trackedLines == null ? Enumerable.Empty<IDynamicLine>() : this.trackedLines.GetLines(startLineNumber, endLineNumber);

        public void Handle(NewCoverageLinesMessage message) => this.UpdateCoverageLines(message.CoverageLines);

        private void UpdateCoverageLines(IFileLineCoverage fileLineCoverage)
        {
            if (!this.isApplicableContentType) return;

            this.fileLineCoverage = fileLineCoverage;

            bool hadTrackedLines = this.trackedLines != null;
            if (this.fileLineCoverage == null)
            {
                this.trackedLines = null;
                if (hadTrackedLines)
                {
                    this.SendCoverageChangedMessage();
                }
            }
            else
            {
                this.CreateTrackedLinesIfRequiredWithMessage();
            }
        }

        public void Handle(TestExecutionStartingMessage message) => this.lastTestExecutionStarting = DateTime.Now;
        public void Handle(ClearLinesMessage message) => this.UpdateCoverageLines(null);
    }
}
