using System;
using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Model;
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
        private readonly IDynamicCoverageStore dynamicCoverageStore;
        private readonly IAppOptionsProvider appOptionsProvider;
        private readonly ICoverageContentTypes coverageContentTypes;
        private readonly ILogger logger;
        private readonly ITextBuffer2 textBuffer;
        private bool? editorCoverageModeOff;
        private IFileLineCoverage fileLineCoverage;
        private DateTime? lastChanged;
        private DateTime lastTestExecutionStarting;
        private bool applicableContentType = true;

        private ITrackedLines trackedLines;
        public bool HasCoverage => this.trackedLines != null;

        internal enum SerializedCoverageState
        {
            NotSerialized, OutOfDate, Ok
        }

        public BufferLineCoverage(
            ILastCoverage lastCoverage,
            ITextInfo textInfo,
            IEventAggregator eventAggregator,
            ITrackedLinesFactory trackedLinesFactory,
            IDynamicCoverageStore dynamicCoverageStore,
            IAppOptionsProvider appOptionsProvider,
            ICoverageContentTypes coverageContentTypes,
            ILogger logger
        )
        {
            if (lastCoverage != null)
            {
                this.fileLineCoverage = lastCoverage.FileLineCoverage;
                this.lastTestExecutionStarting = lastCoverage.TestExecutionStartingDate;
            }

            this.textBuffer = textInfo.TextBuffer;
            this.textBuffer.ContentTypeChanged += this.ContentTypeChanged;
            this.textInfo = textInfo;
            this.eventAggregator = eventAggregator;
            this.trackedLinesFactory = trackedLinesFactory;
            this.dynamicCoverageStore = dynamicCoverageStore;
            this.appOptionsProvider = appOptionsProvider;
            this.coverageContentTypes = coverageContentTypes;
            this.logger = logger;
            void AppOptionsChanged(IAppOptions appOptions)
            {
                bool newEditorCoverageModeOff = appOptions.EditorCoverageColouringMode == EditorCoverageColouringMode.Off;
                this.editorCoverageModeOff = newEditorCoverageModeOff;
                if (this.trackedLines != null && newEditorCoverageModeOff)
                {
                    this.trackedLines = null;
                    this.SendCoverageChangedMessage();
                }
            }

            appOptionsProvider.OptionsChanged += AppOptionsChanged;
            if (this.fileLineCoverage != null)
            {
                this.CreateTrackedLinesIfRequired(true);
            }

            _ = eventAggregator.AddListener(this);
            this.textBuffer.ChangedOnBackground += this.TextBuffer_ChangedOnBackground;
            void textViewClosedHandler(object s, EventArgs e)
            {
                if (s is ITextView textView)
                {
                    this.UpdateDynamicCoverageStore(((ITextView)s).TextSnapshot);
                }

                this.textBuffer.ChangedOnBackground -= this.TextBuffer_ChangedOnBackground;
                this.textBuffer.ContentTypeChanged -= this.ContentTypeChanged;
                textInfo.TextView.Closed -= textViewClosedHandler;
                appOptionsProvider.OptionsChanged -= AppOptionsChanged;
                _ = eventAggregator.RemoveListener(this);
            }

            textInfo.TextView.Closed += textViewClosedHandler;
        }

        private void ContentTypeChanged(object sender, ContentTypeChangedEventArgs args)
        {
            // purpose is so do not create tracked lines for a content type that is not applicable when new coverage
            this.applicableContentType = this.coverageContentTypes.IsApplicable(args.AfterContentType.TypeName);
            if (this.applicableContentType)
            {
                // this currently does not work as Roslyn is not ready.
                // could fallback to single lines but would have to look at other uses of IFileCodeSpanRangeService
                // this is low priority 
                this.CreateTrackedLinesIfRequired(true);
            }
            else
            {
                this.trackedLines = null;
            }

            this.SendCoverageChangedMessage();
        }

        private void UpdateDynamicCoverageStore(ITextSnapshot textSnapshot)
        {
            if (this.trackedLines != null)
            {
                string snapshotText = textSnapshot.GetText();
                if (this.FileSystemReflectsTrackedLines(snapshotText))
                {
                    // this only applies to the last coverage run.
                    // the DynamicCoverageStore ensures this is removed when next coverage is run
                    this.dynamicCoverageStore.SaveSerializedCoverage(
                        this.textInfo.FilePath,
                        this.trackedLinesFactory.Serialize(this.trackedLines, snapshotText)
                    );
                }
                else
                {
                    this.dynamicCoverageStore.RemoveSerializedCoverage(this.textInfo.FilePath);
                }
            }
            else
            {
                this.dynamicCoverageStore.RemoveSerializedCoverage(this.textInfo.FilePath);
            }
        }

        //todo - behaviour if exception reading text
        private bool FileSystemReflectsTrackedLines(string snapshotText)
            => this.textInfo.GetFileText() == snapshotText;

        private void CreateTrackedLinesIfRequired(bool fromStore)
        {
            if (this.EditorCoverageColouringModeOff())
            {
                this.trackedLines = null;
            }
            else
            {
                this.TryCreateTrackedLines(fromStore);
            }
        }

        private void TryCreateTrackedLines(bool fromStore)
        {
            try
            {
                this.CreateTrackedLines(fromStore);
            }
            catch (Exception e)
            {
                this.logger.Log($"Error creating tracked lines for {this.textInfo.FilePath}", e.ToString());
            }
        }

        private void CreateTrackedLinesIfRequiredWithMessage()
        {
            bool hadTrackedLines = this.trackedLines != null;
            if (!this.lastChanged.HasValue || this.lastChanged < this.lastTestExecutionStarting)
            {
                this.CreateTrackedLinesIfRequired(false);
            }
            else
            {
                this.logger.Log($"Not creating editor marks for {this.textInfo.FilePath} as it was changed after test execution started");
                this.trackedLines = null;
            }

            bool hasTrackedLines = this.trackedLines != null;
            if (hadTrackedLines || hasTrackedLines)
            {
                this.SendCoverageChangedMessage();
            }
        }

        private (SerializedCoverageState,string) GetSerializedCoverageInfo(SerializedCoverageWhen serializedCoverageWhen)
        {
            DateTime lastWriteTime = this.textInfo.GetLastWriteTime();

            if (serializedCoverageWhen == null)
            {
                SerializedCoverageState state = lastWriteTime > this.lastTestExecutionStarting ?
                    SerializedCoverageState.OutOfDate :
                    SerializedCoverageState.NotSerialized;
                return (state, null);
            }

            /*
                If there is a When then it applies to the current coverage run ( as DynamicCoverageStore removes )
                as When is written when the text view is closed it is always - LastWriteTime < When
            */
            return serializedCoverageWhen.When < lastWriteTime
                ? ((SerializedCoverageState, string))(SerializedCoverageState.OutOfDate, null)
                : (SerializedCoverageState.Ok, serializedCoverageWhen.Serialized);
        }

        private void CreateTrackedLines(bool fromStore)
        {
            string filePath = this.textInfo.FilePath;
            ITextSnapshot currentSnapshot = this.textBuffer.CurrentSnapshot;
            if (fromStore)
            {
                SerializedCoverageWhen serializedCoverageWhen = this.dynamicCoverageStore.GetSerializedCoverage(
                    filePath
                );
                (SerializedCoverageState state, string serializedCoverage) = this.GetSerializedCoverageInfo(serializedCoverageWhen);
                switch (state)
                {
                    case SerializedCoverageState.NotSerialized:
                        break;
                    case SerializedCoverageState.Ok:
                        this.trackedLines = this.trackedLinesFactory.Create(
                            serializedCoverage, currentSnapshot, filePath);
                        return;
                    default: // Out of date
                        this.logger.Log($"Not creating editor marks for {this.textInfo.FilePath} as coverage is out of date");
                        return;
                }
            }

            var coberturaLines = this.fileLineCoverage.GetLines(this.textInfo.FilePath).ToList();
            this.trackedLines = this.trackedLinesFactory.Create(coberturaLines, currentSnapshot, filePath);
        }

        private bool EditorCoverageColouringModeOff()
        {
            // as handling the event do not need to check the value again
            if (this.editorCoverageModeOff.HasValue)
            {
                return this.editorCoverageModeOff.Value;
            }

            this.editorCoverageModeOff = this.appOptionsProvider.Get().EditorCoverageColouringMode == EditorCoverageColouringMode.Off;
            return this.editorCoverageModeOff.Value;
        }

        private void TextBuffer_ChangedOnBackground(object sender, TextContentChangedEventArgs textContentChangedEventArgs)
        {
            this.lastChanged = DateTime.Now;
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
                this.logger.Log($"Error updating tracked lines for {this.textInfo.FilePath}", e.ToString());
            }
        }

        private void UpdateTrackedLines(TextContentChangedEventArgs textContentChangedEventArgs)
        {
            IEnumerable<int> changedLineNumbers = this.trackedLines.GetChangedLineNumbers(textContentChangedEventArgs.After, textContentChangedEventArgs.Changes.Select(change => change.NewSpan).ToList())
                .Where(changedLine => changedLine >= 0 && changedLine < textContentChangedEventArgs.After.LineCount);
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
            if (!this.applicableContentType) return;

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
