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
        private readonly ITextInfo _textInfo;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITrackedLinesFactory _trackedLinesFactory;
        private readonly IOptionsProvider<EditorCoverageColouringOptions> _editorCoverageColouringOptionsProvider;
        private readonly ICoverageContentTypes _coverageContentTypes;
        private readonly ILogger _logger;
        private readonly ITextBuffer2 _textBuffer;
        private bool? _editorCoverageModeOff;
        private IFileLineCoverage _fileLineCoverage;
        private DateTime? _textBufferLastChanged;
        private DateTime _lastTestExecutionStarting;
        private bool _isApplicableContentType = true;
        private IFileLines _fileLines;
        private ITrackedLines _trackedLines;
        public bool HasCoverage => this._trackedLines != null;

        public BufferLineCoverage(
            ITextInfo textInfo,
            IEventAggregator eventAggregator,
            ITrackedLinesFactory trackedLinesFactory,
            IOptionsProvider<EditorCoverageColouringOptions> editorCoverageColouringOptionsProvider,
            ICoverageContentTypes coverageContentTypes,
            ILogger logger
        )
        {
            this._textBuffer = textInfo.TextBuffer;
            this._textBuffer.ContentTypeChanged += this.ContentTypeChanged;
            this._textInfo = textInfo;
            this._eventAggregator = eventAggregator;
            this._trackedLinesFactory = trackedLinesFactory;
            this._editorCoverageColouringOptionsProvider = editorCoverageColouringOptionsProvider;
            this._coverageContentTypes = coverageContentTypes;
            this._logger = logger;
            void EditorCoverageColouringOptionsChanged(EditorCoverageColouringOptions editorCoverageColouringOptions)
            {
                bool newEditorCoverageModeOff = editorCoverageColouringOptions.EditorCoverageColouringMode == EditorCoverageColouringMode.Off;
                this._editorCoverageModeOff = newEditorCoverageModeOff;
                if (this._trackedLines == null || !newEditorCoverageModeOff)
                {
                    return;
                }

                this._trackedLines = null;
                this.SendCoverageChangedMessage();
            }

            editorCoverageColouringOptionsProvider.OptionsChanged += EditorCoverageColouringOptionsChanged;
            _ = eventAggregator.AddListener(this);
            this._textBuffer.ChangedOnBackground += this.TextBuffer_ChangedOnBackground;
            void textViewClosedHandler(object s, EventArgs e)
            {
                if (s is ITextView textView)
                {
                    this._fileLines?.TextViewClosed();
                }

                this._textBuffer.ChangedOnBackground -= this.TextBuffer_ChangedOnBackground;
                this._textBuffer.ContentTypeChanged -= this.ContentTypeChanged;
                textInfo.TextView.Closed -= textViewClosedHandler;
                editorCoverageColouringOptionsProvider.OptionsChanged -= EditorCoverageColouringOptionsChanged;
                _ = eventAggregator.RemoveListener(this);
            }

            textInfo.TextView.Closed += textViewClosedHandler;
        }

        public void SetLastCoverage(ILastCoverage lastCoverage)
        {
            if (this.EditorCoverageColouringModeOff())
            {
                return;
            }

            this.UseLastCoverageIfHasFileLines(lastCoverage);
        }

        private void UseLastCoverageIfHasFileLines(ILastCoverage lastCoverage)
        {
            this._fileLineCoverage = lastCoverage.FileLineCoverage;
            this._lastTestExecutionStarting = lastCoverage.TestExecutionStartingDate;
            this._fileLines = this._fileLineCoverage.GetLines(this._textInfo.FilePath);
            if (this._fileLines == null)
            {
                return;
            }

            this.UseLastCoverage();
        }

        private void UseLastCoverage()
        {
            bool isOutOfDate = this.FileLinesFromLastCoverageIfNotOutOfDate();

            if (!isOutOfDate)
            {
                return;
            }

            this._logger.LogFileAndForget($"Not creating editor marks for {this._textInfo.FilePath} as coverage is out of date");
            this._fileLineCoverage.OutOfDate(this._textInfo.FilePath);
            this._fileLines = null;
        }

        private bool FileLinesFromLastCoverageIfNotOutOfDate()
        {
            bool isOutOfDate;
            DateTime lastWriteTime = this._textInfo.GetLastWriteTime();
            if (this._fileLines.HasTrackedLines)
            {
                this._trackedLines = this._fileLines.GetTrackedLinesIfNotOutOfDate(lastWriteTime);
                isOutOfDate = this._trackedLines == null;
            }
            else
            {
                isOutOfDate = lastWriteTime > this._lastTestExecutionStarting;
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
            this._isApplicableContentType = this._coverageContentTypes.IsApplicable(args.AfterContentType.TypeName);
            if (this._isApplicableContentType)
            {
                // this currently does not work as Roslyn is not ready.
                // could fallback to single lines but would have to look at other uses of IFileCodeSpanRangeService
                // this is low priority 
                this.CreateTrackedLinesIfRequired();
            }
            else
            {
                this._trackedLines = null;
            }

            this.SendCoverageChangedMessage();
        }

        private void CreateTrackedLinesIfRequired()
        {
            if (this.EditorCoverageColouringModeOff())
            {
                this._trackedLines = null;
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
                this._logger.LogFileAndForget($"Error creating tracked lines for {this._textInfo.FilePath}", e.ToString());
            }
        }

        private void CreateTrackedLinesIfRequiredWithMessage()
        {
            bool hadTrackedLines = this._trackedLines != null;
            if (!this._textBufferLastChanged.HasValue || this._textBufferLastChanged.Value < this._lastTestExecutionStarting)
            {
                this.CreateTrackedLinesIfRequired();
            }
            else
            {
                this._logger.LogFileAndForget($"Not creating editor marks for {this._textInfo.FilePath} as it was changed after test execution started");
                this._trackedLines = null;
            }

            bool hasTrackedLines = this._trackedLines != null;
            if (!hadTrackedLines && !hasTrackedLines)
            {
                return;
            }

            this.SendCoverageChangedMessage();
        }

        private void CreateTrackedLines()
        {
            this._fileLines = this._fileLineCoverage.GetLines(this._textInfo.FilePath);
            if (this._fileLines == null)
            {
                this._trackedLines = null;
            }
            else
            {
                this.CreateTrackedLinesFromFileLines();
            }
        }

        private void CreateTrackedLinesFromFileLines()
        {
            string filePath = this._textInfo.FilePath;
            ITextSnapshot currentSnapshot = this._textBuffer.CurrentSnapshot;
            this._trackedLines = this._trackedLinesFactory.Create(this._fileLines.Lines, currentSnapshot, filePath);
            this._fileLines.SetTrackedLines(this._trackedLines);
        }

        private bool EditorCoverageColouringModeOff()
        {
            // as handling the event do not need to check the value again
            if (this._editorCoverageModeOff.HasValue)
            {
                return this._editorCoverageModeOff.Value;
            }

            this._editorCoverageModeOff = this._editorCoverageColouringOptionsProvider.Get().EditorCoverageColouringMode == EditorCoverageColouringMode.Off;
            return this._editorCoverageModeOff.Value;
        }

        private void TextBuffer_ChangedOnBackground(object sender, TextContentChangedEventArgs textContentChangedEventArgs)
        {
            this._textBufferLastChanged = DateTime.Now;
            if (this._trackedLines == null)
            {
                return;
            }

            this.TryUpdateTrackedLines(textContentChangedEventArgs);
        }

        private void TryUpdateTrackedLines(TextContentChangedEventArgs textContentChangedEventArgs)
        {
            try
            {
                this.UpdateTrackedLines(textContentChangedEventArgs);
            }
            catch (Exception e)
            {
                this._logger.LogFileAndForget($"Error updating tracked lines for {this._textInfo.FilePath}", e.ToString());
            }
        }

        private void UpdateTrackedLines(TextContentChangedEventArgs textContentChangedEventArgs)
        {
            IEnumerable<int> changedLineNumbers = this._trackedLines.GetChangedLineNumbers(
                textContentChangedEventArgs.After,
                textContentChangedEventArgs.Changes.Select(change => change.NewSpan).ToList()
            ).Where(changedLine => changedLine >= 0 && changedLine < textContentChangedEventArgs.After.LineCount);
            this.SendCoverageChangedMessageIfChanged(changedLineNumbers);
        }

        private void SendCoverageChangedMessageIfChanged(IEnumerable<int> changedLineNumbers)
        {
            if (!changedLineNumbers.Any())
            {
                return;
            }

            this.SendCoverageChangedMessage(changedLineNumbers);
        }

        private void SendCoverageChangedMessage(IEnumerable<int> changedLineNumbers = null)
            => this._eventAggregator.SendMessage(new CoverageChangedMessage(this._textInfo.FilePath, changedLineNumbers));

        public IEnumerable<IDynamicLine> GetLines(int startLineNumber, int endLineNumber)
            => this._trackedLines == null ? Enumerable.Empty<IDynamicLine>() : this._trackedLines.GetLines(startLineNumber, endLineNumber);

        public void Handle(NewCoverageLinesMessage message) => this.UpdateCoverageLines(message.CoverageLines);

        private void UpdateCoverageLines(IFileLineCoverage fileLineCoverage)
        {
            if (!this._isApplicableContentType) return;

            this._fileLineCoverage = fileLineCoverage;

            bool hadTrackedLines = this._trackedLines != null;
            if (this._fileLineCoverage == null)
            {
                this._trackedLines = null;
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

        public void Handle(TestExecutionStartingMessage message) => this._lastTestExecutionStarting = DateTime.Now;
        public void Handle(ClearLinesMessage message) => this.UpdateCoverageLines(null);
    }
}
