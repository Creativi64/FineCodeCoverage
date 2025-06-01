using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage.TrackedLinesImpl.Construction;
using FineCodeCoverage.Engine.ReportGenerator;
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
        private readonly ITextSnapshotText textSnapshotText;
        private readonly ILogger logger;
        private readonly IEventAggregator eventAggregator;

        [ImportingConstructor]
        public ContainingCodeTrackedLinesBuilder(
            [ImportMany]
            ICoverageContentType[] coverageContentTypes,
            ICodeSpanRangeContainingCodeTrackerFactory containingCodeTrackerFactory,
            IContainingCodeTrackedLinesFactory containingCodeTrackedLinesFactory,
            INewCodeTrackerFactory newCodeTrackerFactory,
            ITextSnapshotText textSnapshotText,
            ILogger logger,
            IEventAggregator eventAggregator
        )
        {
            this.coverageContentTypes = coverageContentTypes;
            this.containingCodeTrackerFactory = containingCodeTrackerFactory;
            this.containingCodeTrackedLinesFactory = containingCodeTrackedLinesFactory;
            this.newCodeTrackerFactory = newCodeTrackerFactory;
            this.textSnapshotText = textSnapshotText;
            this.logger = logger;
            this.eventAggregator = eventAggregator;
        }

        private ICoverageContentType GetCoverageContentType(ITextSnapshot textSnapshot)
        {
            string contentTypeName = textSnapshot.ContentType.TypeName;
            return this.coverageContentTypes.First(
                coverageContentType => coverageContentType.ContentTypeName == contentTypeName);
        }

        private static IFileCodeSpanRangeService GetFileCodeSpanRangeServiceForChanges(
            ICoverageContentType coverageContentType
        ) => coverageContentType.UseFileCodeSpanRangeServiceForChanges ?
            coverageContentType.FileCodeSpanRangeService : null;

        private INewCodeTracker GetNewCodeTrackerIfProvidesLineExcluder(ILineExcluder lineExcluder)
        {
            if (lineExcluder == null) return null;
            INewCodeTracker newCodeTracker = this.newCodeTrackerFactory.Create(lineExcluder);
            newCodeTracker.HasNewCodeChanged += (_, args)
                => this.eventAggregator.SendMessage(new NewCodeChangedMessage(args.FilePath, args.HasNewCode), null);

            return newCodeTracker;
        }

        public ITrackedLines Create(List<ICoberturaLine> coberturaLines, ITextSnapshot textSnapshot, string filePath)
        {
            if (AnyLinesOutsideTextSnapshot(coberturaLines, textSnapshot))
            {
                this.logger.LogFileAndForget($"Not creating editor marks for {filePath} as some coverage lines are outside the text snapshot");
                return null;
            }

            ICoverageContentType coverageContentType = this.GetCoverageContentType(textSnapshot);
            IFileCodeSpanRangeService fileCodeSpanRangeService = coverageContentType.FileCodeSpanRangeService;
            List<IContainingCodeTracker> containingCodeTrackers = this.CreateContainingCodeTrackers(
                coberturaLines, textSnapshot, fileCodeSpanRangeService, coverageContentType.CoverageOnlyFromFileCodeSpanRangeService);
            return this.containingCodeTrackedLinesFactory.Create(
                containingCodeTrackers,
                this.GetNewCodeTrackerIfProvidesLineExcluder(coverageContentType.LineExcluder),
                GetFileCodeSpanRangeServiceForChanges(coverageContentType));
        }

        private List<IContainingCodeTracker> CreateContainingCodeTrackers(
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
                    return this.CreateContainingCodeTrackersFromCodeSpanRanges(
                        coberturaLines, textSnapshot, codeSpanRanges, coverageOnlyFromFileCodeSpanRangeService);
                }
            }

            return coberturaLines.ConvertAll(coberturaLine => this.CreateSingleLineContainingCodeTracker(textSnapshot, coberturaLine));
        }

        private static bool AnyLinesOutsideTextSnapshot(
            List<ICoberturaLine> coberturaLines,
            ITextSnapshot textSnapshot
        ) => coberturaLines.Any(coberturaLine => coberturaLine.Number - 1 >= textSnapshot.LineCount);

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
                if (LineAtLineNumber(lineNumber))
                {
                    if (inCodeSpanRange)
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
    }
}
