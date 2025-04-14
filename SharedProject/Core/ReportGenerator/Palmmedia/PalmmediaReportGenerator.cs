using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core;
using System.Text.RegularExpressions;
using PalmmediaVerbosityLevel = Palmmedia.ReportGenerator.Core.Logging.VerbosityLevel;
using PalmmediaMetric = Palmmedia.ReportGenerator.Core.Parser.Analysis.Metric;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    internal static class MetricNames
    {
        private static string crap;
        public static string Crap
        {
            get
            {
                if(crap == null)
                {
                    crap = PalmmediaMetric.CrapScore(0).Name;
                }
                return crap;
            }
        }
        private static string nPath;
        public static string NPath
        {
            get
            {
                if (nPath == null)
                {
                    nPath = PalmmediaMetric.NPathComplexity(0).Name;
                }
                return nPath;
            }
        }
        private static string cyclomaticComplexity;
        public static string CyclomaticComplexity
        {
            get
            {
                if (cyclomaticComplexity == null)
                {
                    cyclomaticComplexity = PalmmediaMetric.CyclomaticComplexity(0).Name;
                }
                return cyclomaticComplexity;
            }
        }
        private static string blocksCovered;
        public static string BlocksCovered
        {
            get
            {
                if (blocksCovered == null)
                {
                    blocksCovered = PalmmediaMetric.BlocksCovered(0).Name;
                }
                return blocksCovered;
            }
        }
        private static string blocksNotCovered;
        public static string BlocksNotCovered
        {
            get
            {
                if (blocksNotCovered == null)
                {
                    blocksNotCovered = PalmmediaMetric.BlocksNotCovered(0).Name;
                }
                return blocksNotCovered;
            }
        }
    }

    internal static class MetricSetter
    {
        private static readonly Dictionary<string, Func<PalmmediaCodeElement, decimal?,MetricType>> metricSetters = new Dictionary<string, Func<PalmmediaCodeElement, decimal?, MetricType>>
        {
            { MetricNames.BlocksCovered, (pce,value) => {
                    pce.BlocksCovered = (int)value;
                    return MetricType.BlocksCovered;
                }
            },
            { MetricNames.BlocksNotCovered, (pce,value) => {
                pce.BlocksNotCovered = (int)value;
                return MetricType.BlocksNotCovered;
            } },
            { MetricNames.Crap, (pce,value) => {
                pce.CrapScore = (int)value;
                return MetricType.Crap;
            } },
            { MetricNames.NPath, (pce,value) => {
                pce.NPathComplexity = (int)value;
                return MetricType.NPath;
            } },
            { MetricNames.CyclomaticComplexity, (pce,value) => {
                pce.CyclomaticComplexity = (int)value;
                return MetricType.CyclomaticComplexity;
            }}
        };

        public static List<MetricType> SetMetricProperties(this PalmmediaCodeElement palmmediaCodeElement,IEnumerable<PalmmediaMetric> metrics)
        {
            var metricTypes = new List<MetricType>();
            foreach (var metric in metrics)
            {
                metricSetters.TryGetValue(metric.Name, out var setter);
                if (setter != null)
                {

                    var metricType = setter(palmmediaCodeElement, metric.Value);
                    metricTypes.Add(metricType);
                }
            }
            return metricTypes;
        }
    }

    internal class PalmmediaAssembly : IAssembly
    {
        public PalmmediaAssembly(Assembly assemblyReport)
        {
            Name = assemblyReport.Name;
            ShortName = assemblyReport.ShortName;
            PalmmediaClasses = assemblyReport.Classes.Select(c => new PalmmediaAssemblyClass(c)).ToList();
            Classes = PalmmediaClasses;
        }
        public List<PalmmediaAssemblyClass> PalmmediaClasses { get; }
        public string Name { get; }
        public string ShortName { get; }
        public IReadOnlyList<IClass> Classes { get; }
    }

    internal class PalmmediaAssemblyClass : IClass
    {
        public PalmmediaAssemblyClass(Class classReport)
        {
            DisplayName = classReport.Name;
            this.FileCodeElements = classReport.Files.ToDictionary(
                cf => cf.Path,
                f => f.CodeElements.Select(ce => new PalmmediaCodeElement(ce, f) as ICodeElement).ToList());
            CodeElements = this.FileCodeElements.Values.SelectMany(ces => ces).ToList();
        }

        public string DisplayName { get; }
        public IReadOnlyDictionary<string, List<ICodeElement>> FileCodeElements { get; }
        public IReadOnlyList<ICodeElement> CodeElements { get; }
    }

    internal class SourceFileClass : IClass
    {
        public SourceFileClass(string displayName,string path, List<ICodeElement> codeElements)
        {
            DisplayName = displayName;
            CodeElements = codeElements;
            FileCodeElements = new Dictionary<string, List<ICodeElement>>
            {
                {path, codeElements},
            };
        }
        public string DisplayName { get; }
        public IReadOnlyList<ICodeElement> CodeElements { get; }
        public IReadOnlyDictionary<string, List<ICodeElement>> FileCodeElements { get; }
    }

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
        public int CyclomaticComplexity { get; set; }
        public int NPathComplexity { get; set; }
        public decimal CrapScore { get; set; }
        public List<ICoberturaLine> Lines { get; private set; }
    }

    internal class CoberturaLine : ICoberturaLine
    {
        // probably add the containing CodeElement or as an interface for change
        public CoberturaLine(int number,CoverageType coverageType)
        {
            Number = number;
            CoverageType = coverageType;
        }
        public int Number { get; }
        public CoverageType CoverageType { get; }
    }

    internal class SourceFile : ISourceFile
    {
        public SourceFile(string path, List<SourceFileClass> classes)
        {
            Path = path;
            Classes = classes;
        }
        public string Path { get; }
        public IReadOnlyList<IClass> Classes { get; }
    }

    internal class PalmmediaReportResult : IReportResult
    {
        public PalmmediaReportResult(ParserResult parserResult)
        {
            staticMetricTypes.Clear();
            if (parserResult.SupportsBranchCoverage)
            {
                staticMetricTypes.Add(MetricType.Branches);
            }
            Assemblies = parserResult.Assemblies.Select(a => (IAssembly)new PalmmediaAssembly(a)).ToList();
        }
        public IReadOnlyList<MetricType> MetricTypes => staticMetricTypes.ToList();
        private static readonly HashSet<MetricType> staticMetricTypes = new HashSet<MetricType>();
        public static void AddMetricTypes(List<MetricType> metricTypes)
        {
            metricTypes.ForEach(metricType => staticMetricTypes.Add(metricType));
        }

        public IReadOnlyList<IAssembly> Assemblies { get; }
    }

    [Export(typeof(IFCCReportGenerator))]
    internal class PalmmediaReportGenerator
        : IFCCReportGenerator
    {
        private readonly Regex fileDoesNotExistAnymoreRegex = new Regex(@"File '.*' does not exist \(any more\)\.", RegexOptions.Compiled);
        private readonly IHtmlFilesToFolder htmlFilesToFolder;

        [ImportingConstructor]
        public PalmmediaReportGenerator(
            IHtmlFilesToFolder htmlFilesToFolder
            )
        {
            this.htmlFilesToFolder = htmlFilesToFolder;
        }

        public IReportResult Generate(IEnumerable<string> coverageFiles, string reportDirectory, IEnumerable<string> reportTypes)
        {
            var empty = Enumerable.Empty<string>();
            var defaultFilter = new DefaultFilter(new string[] { });
            var config = new ReportConfiguration(
                new ReadOnlyCollection<string>(coverageFiles.ToList()),
                reportDirectory,
                empty,
                null,
                new ReadOnlyCollection<string>(reportTypes.ToList()),
                empty,
                empty,
                empty,
                empty,
                verbosityLevel.ToString(),
                ""
                );

            var parser = new CoverageReportParser(1, 1, Enumerable.Empty<string>(), defaultFilter, defaultFilter, defaultFilter);
            ReadOnlyCollection<string> collection = new ReadOnlyCollection<string>(coverageFiles.ToList());
            var parserResult = parser.ParseFiles(collection);
            new Generator().GenerateReport(config, parserResult);
            htmlFilesToFolder.Collate(reportDirectory);
            return new PalmmediaReportResult(parserResult);
        }

        private VerbosityLevel verbosityLevel;
        public void SetLogger(VerbosityLevel verbosityLevel, Action<VerbosityLevel, string> logger)
        {
            this.verbosityLevel = verbosityLevel;
            LoggerFactory.Configure((palmmediaVerbosityLevel, message) =>
            {
                var shouldLog = true;
                if (palmmediaVerbosityLevel != PalmmediaVerbosityLevel.Error)
                {
                    Match matched = this.fileDoesNotExistAnymoreRegex.Match(message);
                    shouldLog = !matched.Success;

                }
                if (shouldLog)
                {
                    logger((VerbosityLevel)palmmediaVerbosityLevel, message);
                }
            });
            LoggerFactory.VerbosityLevel = (PalmmediaVerbosityLevel)verbosityLevel;
        }
    }

}
