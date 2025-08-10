using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Editor.DynamicCoverage.ContentTypes;
using FineCodeCoverage.Editor.DynamicCoverage.Management;
using FineCodeCoverage.Editor.DynamicCoverage.NewCode;
using FineCodeCoverage.Utilities.Events;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [Export(typeof(ITrackedLinesFactory))]
    internal sealed class ContainingCodeTrackedLinesBuilder : ITrackedLinesFactory
    {
        private readonly ICoverageContentType[] _coverageContentTypes;
        private readonly ICodeSpanRangeContainingCodeTrackerFactory _containingCodeTrackerFactory;
        private readonly IContainingCodeTrackedLinesFactory _containingCodeTrackedLinesFactory;
        private readonly INewCodeTrackerFactory _newCodeTrackerFactory;
        private readonly ITextSnapshotText _textSnapshotText;
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;

        [ImportingConstructor]
        public ContainingCodeTrackedLinesBuilder(
            [ImportMany]
            ICoverageContentType[] coverageContentTypes,
            ICodeSpanRangeContainingCodeTrackerFactory containingCodeTrackerFactory,
            IContainingCodeTrackedLinesFactory containingCodeTrackedLinesFactory,
            INewCodeTrackerFactory newCodeTrackerFactory,
            ITextSnapshotText textSnapshotText,
            ILogger logger,
            IEventAggregator eventAggregator)
        {
            _coverageContentTypes = coverageContentTypes;
            _containingCodeTrackerFactory = containingCodeTrackerFactory;
            _containingCodeTrackedLinesFactory = containingCodeTrackedLinesFactory;
            _newCodeTrackerFactory = newCodeTrackerFactory;
            _textSnapshotText = textSnapshotText;
            _logger = logger;
            _eventAggregator = eventAggregator;
        }

        private ICoverageContentType GetCoverageContentType(ITextSnapshot textSnapshot)
        {
            string contentTypeName = textSnapshot.ContentType.TypeName;
            return _coverageContentTypes.First(
                coverageContentType => coverageContentType.ContentTypeName == contentTypeName);
        }

        private static IFileCodeSpanRangeService GetFileCodeSpanRangeServiceForChanges(
            ICoverageContentType coverageContentType) => coverageContentType.UseFileCodeSpanRangeServiceForChanges ?
            coverageContentType.FileCodeSpanRangeService : null;

        private INewCodeTracker GetNewCodeTrackerIfProvidesLineExcluder(ILineExcluder lineExcluder)
        {
            if (lineExcluder == null)
            {
                return null;
            }

            INewCodeTracker newCodeTracker = _newCodeTrackerFactory.Create(lineExcluder);
            newCodeTracker.HasNewCodeChanged += (_, args)
                => _eventAggregator.SendMessage(new NewCodeChangedMessage(args.FilePath, args.HasNewCode), null);

            return newCodeTracker;
        }

        public ITrackedLines Create(List<ICoberturaLine> coberturaLines, ITextSnapshot textSnapshot, string filePath)
        {
            if (AnyLinesOutsideTextSnapshot(coberturaLines, textSnapshot))
            {
                _logger.LogFileAndForget($"Not creating editor marks for {filePath} as some coverage lines are outside the text snapshot");
                return null;
            }

            ICoverageContentType coverageContentType = GetCoverageContentType(textSnapshot);
            IFileCodeSpanRangeService fileCodeSpanRangeService = coverageContentType.FileCodeSpanRangeService;
            List<IContainingCodeTracker> containingCodeTrackers = CreateContainingCodeTrackers(
                coberturaLines, textSnapshot, fileCodeSpanRangeService, coverageContentType.CoverageOnlyFromFileCodeSpanRangeService);
            return _containingCodeTrackedLinesFactory.Create(
                containingCodeTrackers,
                GetNewCodeTrackerIfProvidesLineExcluder(coverageContentType.LineExcluder),
                GetFileCodeSpanRangeServiceForChanges(coverageContentType));
        }

        private List<IContainingCodeTracker> CreateContainingCodeTrackers(
            List<ICoberturaLine> coberturaLines,
            ITextSnapshot textSnapshot,
            IFileCodeSpanRangeService fileCodeSpanRangeService,
            bool coverageOnlyFromFileCodeSpanRangeService)
        {
            if (fileCodeSpanRangeService != null)
            {
                List<CodeSpanRange> codeSpanRanges = fileCodeSpanRangeService.GetFileCodeSpanRanges(textSnapshot);
                if (codeSpanRanges != null)
                {
                    return CreateContainingCodeTrackersFromCodeSpanRanges(
                        coberturaLines, textSnapshot, codeSpanRanges, coverageOnlyFromFileCodeSpanRangeService);
                }
            }

            return coberturaLines.ConvertAll(coberturaLine => CreateSingleLineContainingCodeTracker(textSnapshot, coberturaLine));
        }

        private static bool AnyLinesOutsideTextSnapshot(
            List<ICoberturaLine> coberturaLines,
            ITextSnapshot textSnapshot) => coberturaLines.Any(coberturaLine => coberturaLine.Number - 1 >= textSnapshot.LineCount);

        private IContainingCodeTracker CreateSingleLineContainingCodeTracker(ITextSnapshot textSnapshot, ICoberturaLine coberturaLine)
            => CreateCoverageLines(textSnapshot, new List<ICoberturaLine> { coberturaLine }, CodeSpanRange.SingleLine(coberturaLine.Number - 1));

        private IContainingCodeTracker CreateOtherLines(ITextSnapshot textSnapshot, CodeSpanRange codeSpanRange)
            => _containingCodeTrackerFactory.CreateOtherLines(
                    textSnapshot,
                    codeSpanRange,
                    SpanTrackingMode.EdgeNegative);

        private IContainingCodeTracker CreateCoverageLines(ITextSnapshot textSnapshot, List<ICoberturaLine> coberturaLines, CodeSpanRange containingRange)
            => _containingCodeTrackerFactory.CreateCoverageLines(textSnapshot, coberturaLines, containingRange, SpanTrackingMode.EdgeExclusive);

        private IContainingCodeTracker CreateNotIncluded(ITextSnapshot textSnapshot, CodeSpanRange containingRange)
            => _containingCodeTrackerFactory.CreateNotIncluded(textSnapshot, containingRange, SpanTrackingMode.EdgeExclusive);

        private List<IContainingCodeTracker> CreateContainingCodeTrackersFromCodeSpanRanges(
            List<ICoberturaLine> coberturaLines,
            ITextSnapshot textSnapshot,
            List<CodeSpanRange> codeSpanRanges,
            bool coverageOnlyFromFileCodeSpanRangeService)
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
                string lineText = _textSnapshotText.GetLineText(textSnapshot, otherCodeLine);
                if (string.IsNullOrWhiteSpace(lineText))
                {
                    return;
                }

                containingCodeTrackers.Add(
                    CreateOtherLines(
                        textSnapshot,
                        CodeSpanRange.SingleLine(otherCodeLine)));
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
                            containingCodeTrackers.Add(CreateSingleLineContainingCodeTracker(textSnapshot, coberturaLine));
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
                        ? CreateCoverageLines(textSnapshot, containedCoberturaLines, codeSpanRange)
                        : CreateNotIncluded(textSnapshot, codeSpanRange);
                    containingCodeTrackers.Add(containingCodeTracker);

                    containedCoberturaLines = new List<ICoberturaLine>();
                    codeSpanRange = GetNextCodeSpanRange();
                }
            }

            return containingCodeTrackers;
        }
    }
}
