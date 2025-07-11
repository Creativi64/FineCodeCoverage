using System;
using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Collection.Messages;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Options.EditorCoverageColouring;
using FineCodeCoverage.Utilities.Events;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal sealed class BufferLineCoverage :
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

        public bool HasCoverage => _trackedLines != null;

        public BufferLineCoverage(
            ITextInfo textInfo,
            IEventAggregator eventAggregator,
            ITrackedLinesFactory trackedLinesFactory,
            IOptionsProvider<EditorCoverageColouringOptions> editorCoverageColouringOptionsProvider,
            ICoverageContentTypes coverageContentTypes,
            ILogger logger)
        {
            _textBuffer = textInfo.TextBuffer;
            _textBuffer.ContentTypeChanged += ContentTypeChanged;
            _textInfo = textInfo;
            _eventAggregator = eventAggregator;
            _trackedLinesFactory = trackedLinesFactory;
            _editorCoverageColouringOptionsProvider = editorCoverageColouringOptionsProvider;
            _coverageContentTypes = coverageContentTypes;
            _logger = logger;
            void EditorCoverageColouringOptionsChanged(EditorCoverageColouringOptions editorCoverageColouringOptions)
            {
                bool newEditorCoverageModeOff = editorCoverageColouringOptions.EditorCoverageColouringMode == EditorCoverageColouringMode.Off;
                _editorCoverageModeOff = newEditorCoverageModeOff;
                if (_trackedLines == null || !newEditorCoverageModeOff)
                {
                    return;
                }

                _trackedLines = null;
                SendCoverageChangedMessage();
            }

            editorCoverageColouringOptionsProvider.OptionsChanged += EditorCoverageColouringOptionsChanged;
            _ = eventAggregator.AddListener(this);
            _textBuffer.ChangedOnBackground += TextBuffer_ChangedOnBackground;
            void TextViewClosedHandler(object s, EventArgs e)
            {
                if (s is ITextView textView)
                {
                    _fileLines?.TextViewClosed();
                }

                _textBuffer.ChangedOnBackground -= TextBuffer_ChangedOnBackground;
                _textBuffer.ContentTypeChanged -= ContentTypeChanged;
                textInfo.TextView.Closed -= TextViewClosedHandler;
                editorCoverageColouringOptionsProvider.OptionsChanged -= EditorCoverageColouringOptionsChanged;
                _ = eventAggregator.RemoveListener(this);
            }

            textInfo.TextView.Closed += TextViewClosedHandler;
        }

        public void SetLastCoverage(ILastCoverage lastCoverage)
        {
            if (EditorCoverageColouringModeOff())
            {
                return;
            }

            UseLastCoverageIfHasFileLines(lastCoverage);
        }

        private void UseLastCoverageIfHasFileLines(ILastCoverage lastCoverage)
        {
            _fileLineCoverage = lastCoverage.FileLineCoverage;
            _lastTestExecutionStarting = lastCoverage.TestExecutionStartingDate;
            _fileLines = _fileLineCoverage.GetLines(_textInfo.FilePath);
            if (_fileLines == null)
            {
                return;
            }

            UseLastCoverage();
        }

        private void UseLastCoverage()
        {
            bool isOutOfDate = FileLinesFromLastCoverageIfNotOutOfDate();

            if (!isOutOfDate)
            {
                return;
            }

            _logger.LogFileAndForget($"Not creating editor marks for {_textInfo.FilePath} as coverage is out of date");
            _fileLineCoverage.OutOfDate(_textInfo.FilePath);
            _fileLines = null;
        }

        private bool FileLinesFromLastCoverageIfNotOutOfDate()
        {
            bool isOutOfDate;
            DateTime lastWriteTime = _textInfo.GetLastWriteTime();
            if (_fileLines.HasTrackedLines)
            {
                _trackedLines = _fileLines.GetTrackedLinesIfNotOutOfDate(lastWriteTime);
                isOutOfDate = _trackedLines == null;
            }
            else
            {
                isOutOfDate = lastWriteTime > _lastTestExecutionStarting;
                if (!isOutOfDate)
                {
                    TryCreateTrackedLines(CreateTrackedLinesFromFileLines);
                }
            }

            return isOutOfDate;
        }

        private void ContentTypeChanged(object sender, ContentTypeChangedEventArgs args)
        {
            // purpose is so do not create tracked lines for a content type that is not applicable when new coverage
            _isApplicableContentType = _coverageContentTypes.IsApplicable(args.AfterContentType.TypeName);
            if (_isApplicableContentType)
            {
                // this currently does not work as Roslyn is not ready.
                // could fallback to single lines but would have to look at other uses of IFileCodeSpanRangeService
                // this is low priority
                CreateTrackedLinesIfRequired();
            }
            else
            {
                _trackedLines = null;
            }

            SendCoverageChangedMessage();
        }

        private void CreateTrackedLinesIfRequired()
        {
            if (EditorCoverageColouringModeOff())
            {
                _trackedLines = null;
            }
            else
            {
                TryCreateTrackedLines();
            }
        }

        private void TryCreateTrackedLines() => TryCreateTrackedLines(CreateTrackedLines);

        private void TryCreateTrackedLines(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                _logger.LogFileAndForget($"Error creating tracked lines for {_textInfo.FilePath}", e.ToString());
            }
        }

        private void CreateTrackedLinesIfRequiredWithMessage()
        {
            bool hadTrackedLines = _trackedLines != null;
            if (!_textBufferLastChanged.HasValue || _textBufferLastChanged.Value < _lastTestExecutionStarting)
            {
                CreateTrackedLinesIfRequired();
            }
            else
            {
                _logger.LogFileAndForget($"Not creating editor marks for {_textInfo.FilePath} as it was changed after test execution started");
                _trackedLines = null;
            }

            bool hasTrackedLines = _trackedLines != null;
            if (!hadTrackedLines && !hasTrackedLines)
            {
                return;
            }

            SendCoverageChangedMessage();
        }

        private void CreateTrackedLines()
        {
            _fileLines = _fileLineCoverage.GetLines(_textInfo.FilePath);
            if (_fileLines == null)
            {
                _trackedLines = null;
            }
            else
            {
                CreateTrackedLinesFromFileLines();
            }
        }

        private void CreateTrackedLinesFromFileLines()
        {
            string filePath = _textInfo.FilePath;
            ITextSnapshot currentSnapshot = _textBuffer.CurrentSnapshot;
            _trackedLines = _trackedLinesFactory.Create(_fileLines.Lines, currentSnapshot, filePath);
            _fileLines.SetTrackedLines(_trackedLines);
        }

        private bool EditorCoverageColouringModeOff()
        {
            // as handling the event do not need to check the value again
            if (_editorCoverageModeOff.HasValue)
            {
                return _editorCoverageModeOff.Value;
            }

            _editorCoverageModeOff = _editorCoverageColouringOptionsProvider.Get().EditorCoverageColouringMode == EditorCoverageColouringMode.Off;
            return _editorCoverageModeOff.Value;
        }

        private void TextBuffer_ChangedOnBackground(object sender, TextContentChangedEventArgs textContentChangedEventArgs)
        {
            _textBufferLastChanged = DateTime.Now;
            if (_trackedLines == null)
            {
                return;
            }

            TryUpdateTrackedLines(textContentChangedEventArgs);
        }

        private void TryUpdateTrackedLines(TextContentChangedEventArgs textContentChangedEventArgs)
        {
            try
            {
                UpdateTrackedLines(textContentChangedEventArgs);
            }
            catch (Exception e)
            {
                _logger.LogFileAndForget($"Error updating tracked lines for {_textInfo.FilePath}", e.ToString());
            }
        }

        private void UpdateTrackedLines(TextContentChangedEventArgs textContentChangedEventArgs)
        {
            IEnumerable<int> changedLineNumbers = _trackedLines.GetChangedLineNumbers(
                textContentChangedEventArgs.After,
                textContentChangedEventArgs.Changes.Select(change => change.NewSpan).ToList())
            .Where(changedLine => changedLine >= 0 && changedLine < textContentChangedEventArgs.After.LineCount);
            SendCoverageChangedMessageIfChanged(changedLineNumbers);
        }

        private void SendCoverageChangedMessageIfChanged(IEnumerable<int> changedLineNumbers)
        {
            if (!changedLineNumbers.Any())
            {
                return;
            }

            SendCoverageChangedMessage(changedLineNumbers);
        }

        private void SendCoverageChangedMessage(IEnumerable<int> changedLineNumbers = null)
            => _eventAggregator.SendMessage(new CoverageChangedMessage(_textInfo.FilePath, changedLineNumbers));

        public IEnumerable<IDynamicLine> GetLines(int startLineNumber, int endLineNumber)
            => _trackedLines == null ? Enumerable.Empty<IDynamicLine>() : _trackedLines.GetLines(startLineNumber, endLineNumber);

        public void Handle(NewCoverageLinesMessage message) => UpdateCoverageLines(message.CoverageLines);

        private void UpdateCoverageLines(IFileLineCoverage fileLineCoverage)
        {
            if (!_isApplicableContentType)
            {
                return;
            }

            _fileLineCoverage = fileLineCoverage;

            bool hadTrackedLines = _trackedLines != null;
            if (_fileLineCoverage == null)
            {
                _trackedLines = null;
                if (hadTrackedLines)
                {
                    SendCoverageChangedMessage();
                }
            }
            else
            {
                CreateTrackedLinesIfRequiredWithMessage();
            }
        }

        public void Handle(TestExecutionStartingMessage message) => _lastTestExecutionStarting = DateTime.Now;

        public void Handle(ClearLinesMessage message) => UpdateCoverageLines(null);
    }
}
