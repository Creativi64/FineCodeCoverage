using System;
using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    internal class PalmmediaCodeElement : ICodeElement
    {
        public PalmmediaCodeElement(CodeElement codeElement, CodeFile codeFile)
        {
            this.CodeElementType = this.ConvertCodeElementType(codeElement.CodeElementType);
            this.Name = codeElement.Name;
            this.StartLine = codeElement.FirstLine;
            IEnumerable<LineVisitStatus> lineVisitStatuses = codeFile.LineVisitStatus.Skip(codeElement.FirstLine)
            .Take(codeElement.LastLine - codeElement.FirstLine + 1);
            this.SetLines(lineVisitStatuses);

            this.Path = codeFile.Path;
            MethodMetric methodMetrics = codeFile.MethodMetrics.FirstOrDefault(methodMetric => methodMetric.FullName == codeElement.FullName);
            if (methodMetrics != null)
            {
                List<MetricType> metricTypes = this.SetMetricProperties(methodMetrics.Metrics);
                PalmmediaReportResult.AddMetricTypes(metricTypes);
            }

            codeFile.BranchesByLine
                .Where(kvp => kvp.Key >= codeElement.FirstLine && kvp.Key <= codeElement.LastLine)
                .ToList()
                .ForEach(kvp =>
                {
                    ICollection<Branch> branches = kvp.Value;
                    this.TotalBranches += branches.Count;
                    this.BranchesCovered += branches.Count(b => b.BranchVisits > 0);
                });
        }

        private void SetLines(IEnumerable<LineVisitStatus> lineVisitStatuses)
        {
            var coberturaLines = new List<ICoberturaLine>();
            int lineNumber = this.StartLine;
            foreach (LineVisitStatus lineVisitStatus in lineVisitStatuses)
            {
                if (lineVisitStatus != LineVisitStatus.NotCoverable)
                {
                    coberturaLines.Add(new CoberturaLine(lineNumber, this.ConvertLineVisitStatus(lineVisitStatus)));
                }

                lineNumber++;
            }

            this.Lines = coberturaLines;
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
