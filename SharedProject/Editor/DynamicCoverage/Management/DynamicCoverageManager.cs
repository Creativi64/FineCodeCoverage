using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage.Utilities;
using FineCodeCoverage.Engine.ReportGenerator;
using FineCodeCoverage.Impl;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [Export(typeof(IInitializable))]
    [Export(typeof(IDynamicCoverageManager))]
    internal class DynamicCoverageManager :
        IDynamicCoverageManager,
        IListener<TestExecutionStartingMessage>,
        IListener<NewReportMessage>,
        IInitializable
    {
        private readonly IEventAggregator eventAggregator;
        private readonly ITrackedLinesFactory trackedLinesFactory;
        private readonly IBufferLineCoverageFactory bufferLineCoverageFactory;
        private readonly IDateTimeService dateTimeService;
        private LastCoverage lastCoverage;
        private DateTime lastTestExecutionStartingDate;


        internal class ReportFileLineCoverage : IFileLineCoverage
        {
            private Dictionary<string, ISourceFile> sourceFileLookup;
            private readonly Func<IDirectory> directoryProvider;

            private Dictionary<string, ISourceFile> SourceFileLookup
            {
                get
                {
                    if (sourceFileLookup == null)
                    {
                        CollectSourceFiles();
                    }
                    return sourceFileLookup;
                }
            }

            public ReportFileLineCoverage(Func<IDirectory> directoryProvider)
            {
                this.directoryProvider = directoryProvider;
            }

            public List<ICoberturaLine> GetLines(string filePath)
            {
                if (!SourceFileLookup.TryGetValue(filePath, out var sourceFile))
                {
                    return Enumerable.Empty<ICoberturaLine>().ToList();
                }

                var codeElements = sourceFile.Classes.SelectMany(c => c.CodeElements);
                return codeElements.SelectMany(codeElement => GetLines(codeElement)).ToList();
            }

            private CoverageType ConvertLineVisitStatus(LineVisitStatus lineVisitStatus)
            {
                switch (lineVisitStatus)
                {
                    case LineVisitStatus.Covered:
                        return CoverageType.Covered;
                    case LineVisitStatus.NotCovered:
                        return CoverageType.NotCovered;
                    case LineVisitStatus.PartiallyCovered:
                        return CoverageType.Partial;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private List<ICoberturaLine> GetLines(ICodeElement codeElement)
            {
                var coberturaLines = new List<ICoberturaLine>();
                var lineNumber = codeElement.StartLine;
                foreach (var lineVisitStatus in codeElement.LineVisitStatuses)
                {
                    if (lineVisitStatus != LineVisitStatus.NotCoverable)
                    {
                        coberturaLines.Add(new CoberturaLine(lineNumber, ConvertLineVisitStatus(lineVisitStatus)));
                    }
                    lineNumber++;
                }
                return coberturaLines;
            }

            private void CollectSourceFiles()
            {
                sourceFileLookup = new Dictionary<string, ISourceFile>();
                CollectSourceFiles(directoryProvider(), sourceFileLookup);
            }

            private void CollectSourceFiles(IDirectory directory, Dictionary<string, ISourceFile> sourceFileLookup)
            {
                foreach (var sourceFile in directory.SourceFiles)
                {
                    sourceFileLookup.Add(sourceFile.Path, sourceFile);
                }
                foreach (var subDirectory in directory.SubDirectories)
                {
                    CollectSourceFiles(subDirectory, sourceFileLookup);
                }
            }
        }



        [ImportingConstructor]
        public DynamicCoverageManager(
            IEventAggregator eventAggregator,
            ITrackedLinesFactory trackedLinesFactory,
            IBufferLineCoverageFactory bufferLineCoverageFactory,
            IDateTimeService dateTimeService)
        {
            this.bufferLineCoverageFactory = bufferLineCoverageFactory;
            this.dateTimeService = dateTimeService;
            _ = eventAggregator.AddListener(this);
            this.eventAggregator = eventAggregator;
            this.trackedLinesFactory = trackedLinesFactory;
        }

        public void Handle(NewReportMessage message) {
            var fileLineCoverage = new ReportFileLineCoverage(() => message.Report.Directory);
            this.lastCoverage = new LastCoverage(fileLineCoverage, this.lastTestExecutionStartingDate);
            eventAggregator.SendMessage(new NewCoverageLinesMessage(fileLineCoverage));
        }


        public void Handle(TestExecutionStartingMessage message) => this.lastTestExecutionStartingDate = this.dateTimeService.Now;

        public IBufferLineCoverage Manage(ITextInfo textInfo)
            => textInfo.TextBuffer.Properties.GetOrCreateSingletonProperty(
                () => this.bufferLineCoverageFactory.Create(this.lastCoverage, textInfo, this.eventAggregator, this.trackedLinesFactory)
            );
    }
}
