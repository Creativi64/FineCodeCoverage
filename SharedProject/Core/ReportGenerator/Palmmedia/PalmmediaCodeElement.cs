using System;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    internal class PalmmediaCodeElement : ICodeElement
    {
        public PalmmediaCodeElement(CodeElement codeElement, CodeFile codeFile)
        {
            CodeElementType = ConvertCodeElementType(codeElement.CodeElementType);
            Name = codeElement.Name;
            StartLine = codeElement.FirstLine;
            var lineVisitStatuses = codeFile.LineVisitStatus.Skip(codeElement.FirstLine)
            .Take(codeElement.LastLine - codeElement.FirstLine + 1);
            SetLines(lineVisitStatuses);

            Path = codeFile.Path;
            var methodMetrics = codeFile.MethodMetrics.FirstOrDefault(methodMetric => methodMetric.FullName == codeElement.FullName);
            if (methodMetrics != null)
            {
                var metricTypes = this.SetMetricProperties(methodMetrics.Metrics);
                PalmmediaReportResult.AddMetricTypes(metricTypes);
            }
            codeFile.BranchesByLine
                .Where(kvp => kvp.Key >= codeElement.FirstLine && kvp.Key <= codeElement.LastLine)
                .ToList()
                .ForEach(kvp =>
                {
                    var branches = kvp.Value;
                    TotalBranches += branches.Count;
                    BranchesCovered += branches.Count(b => b.BranchVisits > 0);
                });
        }

        private void SetLines(IEnumerable<LineVisitStatus> lineVisitStatuses)
        {
            var coberturaLines = new List<ICoberturaLine>();
            int lineNumber = StartLine;
            foreach (LineVisitStatus lineVisitStatus in lineVisitStatuses)
            {
                if (lineVisitStatus != LineVisitStatus.NotCoverable)
                {
                    coberturaLines.Add(new CoberturaLine(lineNumber, this.ConvertLineVisitStatus(lineVisitStatus)));
                }

                lineNumber++;
            }
            Lines = coberturaLines;
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

        private CodeElementType ConvertCodeElementType(Palmmedia.ReportGenerator.Core.Parser.Analysis.CodeElementType cet)
        {
            switch (cet)
            {
                case Palmmedia.ReportGenerator.Core.Parser.Analysis.CodeElementType.Property:
                    return CodeElementType.Property;
                case Palmmedia.ReportGenerator.Core.Parser.Analysis.CodeElementType.Method:
                    return CodeElementType.Method;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public CodeElementType CodeElementType { get; }
        public string Name { get; }
        public int StartLine { get; }
        public string Path { get; }
        public int BlocksCovered { get; set; }
        public int BlocksNotCovered { get; set; }
        public int TotalBranches { get; set; }
        public int BranchesCovered { get; set; }
        public int CyclomaticComplexity { get; set; }
        public int NPathComplexity { get; set; }
        public decimal CrapScore { get; set; }
        public IReadOnlyList<ICoberturaLine> Lines { get; private set; }
    }

}
