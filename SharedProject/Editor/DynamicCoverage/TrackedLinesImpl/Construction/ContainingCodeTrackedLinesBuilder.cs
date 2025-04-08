using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage.TrackedLinesImpl.Construction;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [Export(typeof(ITrackedLinesFactory))]
    internal class ContainingCodeTrackedLinesBuilder : ITrackedLinesFactory
    {
        private readonly ICoverageContentType[] coverageContentTypes;
        private readonly ICodeSpanRangeContainingCodeTrackerFactory containingCodeTrackerFactory;
        private readonly IContainingCodeTrackedLinesFactory containingCodeTrackedLinesFactory;
        private readonly INewCodeTrackerFactory newCodeTrackerFactory;
        private readonly IJsonConvertService jsonConvertService;
        private readonly ITextSnapshotText textSnapshotText;
        private readonly ILogger logger;

        [ImportingConstructor]
        public ContainingCodeTrackedLinesBuilder(
            [ImportMany]
            ICoverageContentType[] coverageContentTypes,
            ICodeSpanRangeContainingCodeTrackerFactory containingCodeTrackerFactory,
            IContainingCodeTrackedLinesFactory containingCodeTrackedLinesFactory,
            INewCodeTrackerFactory newCodeTrackerFactory,
            IJsonConvertService jsonConvertService,
            ITextSnapshotText textSnapshotText,
            ILogger logger
        )
        {
            this.coverageContentTypes = coverageContentTypes;
            this.containingCodeTrackerFactory = containingCodeTrackerFactory;
            this.containingCodeTrackedLinesFactory = containingCodeTrackedLinesFactory;
            this.newCodeTrackerFactory = newCodeTrackerFactory;
            this.jsonConvertService = jsonConvertService;
            this.textSnapshotText = textSnapshotText;
            this.logger = logger;
        }

        private ICoverageContentType GetCoverageContentType(ITextSnapshot textSnapshot)
        {
            string contentTypeName = textSnapshot.ContentType.TypeName;
            return this.coverageContentTypes.First(
                coverageContentType => coverageContentType.ContentTypeName == contentTypeName);
        }

        private IFileCodeSpanRangeService GetFileCodeSpanRangeServiceForChanges(ICoverageContentType coverageContentType)
            => coverageContentType.UseFileCodeSpanRangeServiceForChanges ? coverageContentType.FileCodeSpanRangeService : null;

        private INewCodeTracker GetNewCodeTrackerIfProvidesLineExcluder(ILineExcluder lineExcluder)
            => lineExcluder == null ? null : this.newCodeTrackerFactory.Create(lineExcluder);

        public ITrackedLines Create(List<ICoberturaLine> coberturaLines, ITextSnapshot textSnapshot, string filePath)
        {
            if (this.AnyLinesOutsideTextSnapshot(coberturaLines, textSnapshot))
            {
                this.logger.Log($"Not creating editor marks for {filePath} as some coverage lines are outside the text snapshot");
                return null;
            }

            ICoverageContentType coverageContentType = this.GetCoverageContentType(textSnapshot);
            IFileCodeSpanRangeService fileCodeSpanRangeService = coverageContentType.FileCodeSpanRangeService;
            (List<IContainingCodeTracker> containingCodeTrackers, bool usedFileCodeSpanRangeService) = this.CreateContainingCodeTrackers(
                coberturaLines, textSnapshot, fileCodeSpanRangeService, coverageContentType.CoverageOnlyFromFileCodeSpanRangeService);

            IContainingCodeTrackerTrackedLines trackedLines = this.containingCodeTrackedLinesFactory.Create(
                containingCodeTrackers,
                this.GetNewCodeTrackerIfProvidesLineExcluder(coverageContentType.LineExcluder),
                this.GetFileCodeSpanRangeServiceForChanges(coverageContentType));

            return new ContainingCodeTrackerTrackedLinesWithState(trackedLines, usedFileCodeSpanRangeService);
        }

        private (List<IContainingCodeTracker> containingCodeTrackers, bool usedFileCodeSpanRangeService) CreateContainingCodeTrackers(
            List<ICoberturaLine> coberturaLines,
            ITextSnapshot textSnapshot,
            IFileCodeSpanRangeService fileCodeSpanRangeService,
            bool coverageOnlyFromFileCodeSpanRangeService
        )
        {
            if (fileCodeSpanRangeService != null)
            {
                List<CodeSpanRange> codeSpanRanges = fileCodeSpanRangeService.GetFileCodeSpanRanges(textSnapshot);
                if (codeSpanRanges != null)
                {
                    return (this.CreateContainingCodeTrackersFromCodeSpanRanges(
                        coberturaLines, textSnapshot, codeSpanRanges, coverageOnlyFromFileCodeSpanRangeService), true);
                }
            }

            return (coberturaLines.ConvertAll(coberturaLine => this.CreateSingleLineContainingCodeTracker(textSnapshot, coberturaLine)), false);
        }

        private bool AnyLinesOutsideTextSnapshot(List<ICoberturaLine> coberturaLines, ITextSnapshot textSnapshot)
            => coberturaLines.Any(coberturaLine => coberturaLine.Number - 1 >= textSnapshot.LineCount);

        private IContainingCodeTracker CreateSingleLineContainingCodeTracker(ITextSnapshot textSnapshot, ICoberturaLine coberturaLine)
            => this.CreateCoverageLines(textSnapshot, new List<ICoberturaLine> { coberturaLine }, CodeSpanRange.SingleLine(coberturaLine.Number - 1));

        private IContainingCodeTracker CreateOtherLines(ITextSnapshot textSnapshot, CodeSpanRange codeSpanRange)
            => this.containingCodeTrackerFactory.CreateOtherLines(
                    textSnapshot,
                    codeSpanRange,
                    SpanTrackingMode.EdgeNegative
                );

        private IContainingCodeTracker CreateCoverageLines(ITextSnapshot textSnapshot, List<ICoberturaLine> coberturaLines, CodeSpanRange containingRange)
            => this.containingCodeTrackerFactory.CreateCoverageLines(textSnapshot, coberturaLines, containingRange, SpanTrackingMode.EdgeExclusive);

        private IContainingCodeTracker CreateNotIncluded(ITextSnapshot textSnapshot, CodeSpanRange containingRange)
            => this.containingCodeTrackerFactory.CreateNotIncluded(textSnapshot, containingRange, SpanTrackingMode.EdgeExclusive);

        private List<IContainingCodeTracker> CreateContainingCodeTrackersFromCodeSpanRanges(
            List<ICoberturaLine> coberturaLines,
            ITextSnapshot textSnapshot,
            List<CodeSpanRange> codeSpanRanges,
            bool coverageOnlyFromFileCodeSpanRangeService
        )
        {
            var containingCodeTrackers = new List<IContainingCodeTracker>();
            Func<T> GetNextCreator<T>(List<T> list)
            {
                T GetNext()
                {
                    T next = list.FirstOrDefault();
                    if (next != null)
                    {
                        list.RemoveAt(0);
                    }

                    return next;
                }

                return GetNext;
            }

            Func<ICoberturaLine> GetNextLine = GetNextCreator(coberturaLines);
            Func<CodeSpanRange> GetNextCodeSpanRange = GetNextCreator(codeSpanRanges);

            ICoberturaLine coberturaLine = GetNextLine();
            CodeSpanRange codeSpanRange = GetNextCodeSpanRange();
            var containedCoberturaLines = new List<ICoberturaLine>();
            bool InCodeSpanRange(int lineNumber) => codeSpanRange != null && codeSpanRange.StartLine <= lineNumber && codeSpanRange.EndLine >= lineNumber;
            bool AtEndOfCodeSpanRange(int lineNumber) => codeSpanRange != null && codeSpanRange.EndLine == lineNumber;
            bool LineAtLineNumber(int lineNumber) => coberturaLine != null && coberturaLine.Number - 1 == lineNumber;
            void CreateOtherLine(int otherCodeLine)
            {
                string lineText = this.textSnapshotText.GetLineText(textSnapshot, otherCodeLine);
                if (!string.IsNullOrWhiteSpace(lineText))
                {
                    containingCodeTrackers.Add(
                            this.CreateOtherLines(
                                textSnapshot,
                                CodeSpanRange.SingleLine(otherCodeLine)
                            )
                    );
                }
            }

            for (int lineNumber = 0; lineNumber < textSnapshot.LineCount; lineNumber++)
            {
                bool inCodeSpanRange = InCodeSpanRange(lineNumber);
                if(LineAtLineNumber(lineNumber))
                {
                    if(inCodeSpanRange)
                    {
                        containedCoberturaLines.Add(coberturaLine);
                    }
                    else
                    {
                        if (!coverageOnlyFromFileCodeSpanRangeService)
                        {
                            containingCodeTrackers.Add(this.CreateSingleLineContainingCodeTracker(textSnapshot, coberturaLine));
                        }
                        else
                        {
                            CreateOtherLine(lineNumber);
                        }
                    }

                    coberturaLine = GetNextLine();
                }
                else if (!inCodeSpanRange)
                {
                    CreateOtherLine(lineNumber);
                }

                if (AtEndOfCodeSpanRange(lineNumber))
                {
                    IContainingCodeTracker containingCodeTracker = containedCoberturaLines.Count > 0
                        ? this.CreateCoverageLines(textSnapshot, containedCoberturaLines, codeSpanRange)
                        : this.CreateNotIncluded(textSnapshot, codeSpanRange);
                    containingCodeTrackers.Add(containingCodeTracker);

                    containedCoberturaLines = new List<ICoberturaLine>();
                    codeSpanRange = GetNextCodeSpanRange();
                }
            }

            return containingCodeTrackers;
        }

        #region Serialization

        private IContainingCodeTrackerTrackedLines RecreateTrackedLinesNoFileCodeSpanRangeService(
            List<SerializedContainingCodeTracker> serializedContainingCodeTrackers,
            ITextSnapshot currentSnapshot,
            ILineExcluder lineExcluder,
            List<int> newCodeLines
        )
        {
            List<IContainingCodeTracker> containingCodeTrackers = serializedContainingCodeTrackers.ConvertAll(
                serializedContainingCodeTracker => this.RecreateCoverageLines(
                    serializedContainingCodeTracker, currentSnapshot)
            );
            return this.containingCodeTrackedLinesFactory.Create(
                containingCodeTrackers,
                this.GetNewCodeTrackerIfProvidesLineExcluder(lineExcluder, newCodeLines, currentSnapshot),
                null);
        }

        private INewCodeTracker GetNewCodeTrackerIfProvidesLineExcluder(ILineExcluder lineExcluder, List<int> newCodeLines, ITextSnapshot textSnapshot)
            => lineExcluder == null ? null : this.newCodeTrackerFactory.Create(lineExcluder, newCodeLines, textSnapshot);

        private IContainingCodeTracker RecreateCoverageLines(
            SerializedContainingCodeTracker serializedContainingCodeTracker, ITextSnapshot currentSnapshot)
        {
            CodeSpanRange codeSpanRange = serializedContainingCodeTracker.CodeSpanRange;
            return serializedContainingCodeTracker.Lines[0].CoverageType == DynamicCoverageType.Dirty
                ? this.containingCodeTrackerFactory.CreateDirty(currentSnapshot, codeSpanRange, SpanTrackingMode.EdgeExclusive)
                : this.CreateCoverageLines(currentSnapshot, this.AdjustCoverageLines(serializedContainingCodeTracker.Lines), codeSpanRange);
        }

        private List<ICoberturaLine> AdjustCoverageLines(List<DynamicLine> dynamicLines)
            => dynamicLines.Select(dynamicLine => new AdjustedCoberturaLine(dynamicLine)).Cast<ICoberturaLine>().ToList();

        private List<IContainingCodeTracker> RecreateContainingCodeTrackers(
            List<SerializedContainingCodeTracker> serializedContainingCodeTrackers,
            ITextSnapshot currentSnapshot
        ) => serializedContainingCodeTrackers.ConvertAll(
            serializedContainingCodeTracker => this.RecreateContainingCodeTracker(
                serializedContainingCodeTracker, currentSnapshot)
            );

        private IContainingCodeTracker RecreateContainingCodeTracker(
            SerializedContainingCodeTracker serializedContainingCodeTracker,
            ITextSnapshot currentSnapshot
        )
        {
            CodeSpanRange codeSpanRange = serializedContainingCodeTracker.CodeSpanRange;
            switch (serializedContainingCodeTracker.Type)
            {
                case ContainingCodeTrackerType.OtherLines:
                    return this.CreateOtherLines(currentSnapshot, codeSpanRange);
                case ContainingCodeTrackerType.NotIncluded:
                    return this.CreateNotIncluded(currentSnapshot, codeSpanRange);
                case ContainingCodeTrackerType.CoverageLines:
                    return this.RecreateCoverageLines(serializedContainingCodeTracker, currentSnapshot);
            }

            return null;
        }

        private IContainingCodeTrackerTrackedLines RecreateTrackedLinesFileCodeSpanRangeService(
            List<SerializedContainingCodeTracker> serializedContainingCodeTrackers,
            ITextSnapshot currentSnapshot,
            ICoverageContentType coverageContentType)
        {
            List<IContainingCodeTracker> containingCodeTrackers = this.RecreateContainingCodeTrackers(
                serializedContainingCodeTrackers, currentSnapshot);
            List<CodeSpanRange> codeSpanRanges = coverageContentType.FileCodeSpanRangeService.GetFileCodeSpanRanges(currentSnapshot);
            INewCodeTracker newCodeTracker = this.RecreateNewCodeTracker(
                serializedContainingCodeTrackers,
                currentSnapshot,
                coverageContentType,
                codeSpanRanges);
            return this.containingCodeTrackedLinesFactory.Create(
                containingCodeTrackers,
                newCodeTracker,
                this.GetFileCodeSpanRangeServiceForChanges(coverageContentType)
            );
        }

        private INewCodeTracker RecreateNewCodeTracker(
            List<SerializedContainingCodeTracker> serializedContainingCodeTrackers,
            ITextSnapshot currentSnapshot,
            ICoverageContentType coverageContentType,
            List<CodeSpanRange> codeSpanRanges
        )
        {
            if (coverageContentType.LineExcluder == null) return null;

            List<CodeSpanRange> newCodeSpanRanges = this.GetNewCodeSpanRanges(
                codeSpanRanges,
                serializedContainingCodeTrackers.Select(serializedContainingCodeTracker => serializedContainingCodeTracker.CodeSpanRange));
            IEnumerable<int> newCodeLineNumbers = this.GetRecreateNewCodeLineNumbers(newCodeSpanRanges, coverageContentType.UseFileCodeSpanRangeServiceForChanges);
            return this.newCodeTrackerFactory.Create(coverageContentType.LineExcluder, newCodeLineNumbers, currentSnapshot);
        }

        private List<CodeSpanRange> GetNewCodeSpanRanges(List<CodeSpanRange> currentCodeSpanRanges, IEnumerable<CodeSpanRange> serializedCodeSpanRanges)
        {
            foreach (CodeSpanRange serializedCodeSpanRange in serializedCodeSpanRanges)
            {
                _ = currentCodeSpanRanges.Remove(serializedCodeSpanRange);
            }

            return currentCodeSpanRanges;
        }

        private IEnumerable<int> GetRecreateNewCodeLineNumbers(List<CodeSpanRange> newCodeCodeRanges, bool hasFileCodeSpanRangeServiceForChanges)
            => hasFileCodeSpanRangeServiceForChanges
                ? this.StartLines(newCodeCodeRanges)
                : this.EveryLineInCodeSpanRanges(newCodeCodeRanges);

        private IEnumerable<int> StartLines(List<CodeSpanRange> newCodeCodeRanges)
            => newCodeCodeRanges.Select(newCodeCodeRange => newCodeCodeRange.StartLine);
        private IEnumerable<int> EveryLineInCodeSpanRanges(List<CodeSpanRange> newCodeCodeRanges)
            => newCodeCodeRanges.SelectMany(
                newCodeCodeRange => Enumerable.Range(
                    newCodeCodeRange.StartLine,
                    newCodeCodeRange.EndLine - newCodeCodeRange.StartLine + 1)
                );

        public ITrackedLines Create(string serializedCoverage, ITextSnapshot currentSnapshot, string filePath)
        {
            SerializedEditorDynamicCoverage serializedEditorDynamicCoverage = this.jsonConvertService.DeserializeObject<SerializedEditorDynamicCoverage>(serializedCoverage);
            bool usedFileCodeSpanRangeService = serializedEditorDynamicCoverage.UsedFileCodeSpanRangeService;
            bool textUnchanged = this.TextUnchanged(serializedEditorDynamicCoverage, currentSnapshot);
            if (!textUnchanged)
            {
                this.logger.Log($"Not creating editor marks for {filePath} as text has changed");
                return null;
            }

            IContainingCodeTrackerTrackedLines trackedLines = this.RecreateTrackedLines(
                serializedEditorDynamicCoverage.SerializedContainingCodeTrackers,
                serializedEditorDynamicCoverage.NewCodeLineNumbers,
                currentSnapshot,
                usedFileCodeSpanRangeService
            );

            return new ContainingCodeTrackerTrackedLinesWithState(trackedLines, usedFileCodeSpanRangeService);
        }

        private IContainingCodeTrackerTrackedLines RecreateTrackedLines(
            List<SerializedContainingCodeTracker> serializedContainingCodeTrackers,
            List<int> newCodeLineNumbers,
            ITextSnapshot currentSnapshot,
            bool usedFileCodeSpanRangeService
        )
        {
            ICoverageContentType coverageContentType = this.GetCoverageContentType(currentSnapshot);
            return usedFileCodeSpanRangeService ?
                this.RecreateTrackedLinesFileCodeSpanRangeService(serializedContainingCodeTrackers, currentSnapshot, coverageContentType) :
                this.RecreateTrackedLinesNoFileCodeSpanRangeService(serializedContainingCodeTrackers, currentSnapshot, coverageContentType.LineExcluder, newCodeLineNumbers);
        }

        private bool TextUnchanged(SerializedEditorDynamicCoverage serializedEditorDyamicCoverage, ITextSnapshot textSnapshot)
        {
            string previousText = serializedEditorDyamicCoverage.Text;
            string currentText = textSnapshot.GetText();
            return previousText == currentText;
        }

        public string Serialize(ITrackedLines trackedLines, string text)
        {
            var containingCodeTrackerTrackedLinesWithState = trackedLines as ContainingCodeTrackerTrackedLinesWithState;
            List<SerializedContainingCodeTracker> serializedContainingCodeTrackers = this.GetSerializedContainingCodeTrackers(containingCodeTrackerTrackedLinesWithState);
            var newCodeLineNumbers = new List<int>();
            if (containingCodeTrackerTrackedLinesWithState.NewCodeTracker != null)
            {
                newCodeLineNumbers = containingCodeTrackerTrackedLinesWithState.NewCodeTracker.Lines.Select(l => l.Number).ToList();
            }

            return this.jsonConvertService.SerializeObject(
                new SerializedEditorDynamicCoverage {
                    SerializedContainingCodeTrackers = serializedContainingCodeTrackers,
                    Text = text,
                    NewCodeLineNumbers = newCodeLineNumbers,
                    UsedFileCodeSpanRangeService = containingCodeTrackerTrackedLinesWithState.UsedFileCodeSpanRangeService
                });
        }

        private List<SerializedContainingCodeTracker> GetSerializedContainingCodeTrackers(IContainingCodeTrackerTrackedLines trackedLines)
            => trackedLines.ContainingCodeTrackers.Select(
                containingCodeTracker => SerializedContainingCodeTracker.From(containingCodeTracker.GetState())).ToList();

        private class AdjustedCoberturaLine : ICoberturaLine
        {
            public AdjustedCoberturaLine(IDynamicLine dynamicLine)
            {
                this.Number = dynamicLine.Number + 1;
                this.CoverageType = DynamicCoverageTypeConverter.Convert(dynamicLine.CoverageType);
            }

            public int Number { get; }

            public CoverageType CoverageType { get; }
        }
        #endregion
    }
}
