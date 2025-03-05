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
    public static class MetricNames
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

    public static class MetricSetter
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

        public static List<MetricType> SetMetrics(this PalmmediaCodeElement palmmediaCodeElement,IEnumerable<PalmmediaMetric> metrics)
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

    public class PalmmediaAssembly : IAssembly
    {
        public PalmmediaAssembly(Assembly assemblyReport)
        {
            Name = assemblyReport.Name;
            ShortName = assemblyReport.ShortName;
            PalmmediaClasses = assemblyReport.Classes.Select(c => new PalmmediaClass(c)).ToList();
            Classes = PalmmediaClasses;
        }
        public List<PalmmediaClass> PalmmediaClasses { get; }
        public string Name { get; }
        public string ShortName { get; }
        public IReadOnlyCollection<IClass> Classes { get; }
    }

    public class PalmmediaClass : IClass
    {
        public PalmmediaClass(Class classReport) : this(classReport.Name, classReport.Files)
        {
        }

        public PalmmediaClass(string displayName, CodeFile codeFile) : this(displayName, new List<CodeFile> { codeFile })
        {

        }
        private PalmmediaClass(string displayName, IEnumerable<CodeFile> codeFiles)
        {
            DisplayName = displayName;
            CodeElements = codeFiles.SelectMany(f => f.CodeElements.Select(ce => new PalmmediaCodeElement(ce, f))).ToList();
            CodeFiles = codeFiles.ToList();
        }

        public List<CodeFile> CodeFiles { get; }

        public string DisplayName { get; }
        public IReadOnlyCollection<ICodeElement> CodeElements { get; }
    }

    public class PalmmediaCodeElement : ICodeElement
    {
        public PalmmediaCodeElement(CodeElement codeElement, CodeFile codeFile)
        {
            CodeElementType = ConvertCodeElementType(codeElement.CodeElementType);
            Name = codeElement.Name;
            StartLine = codeElement.FirstLine;

            LineVisitStatuses = codeFile.LineVisitStatus.Skip(codeElement.FirstLine)
            .Take(codeElement.LastLine - codeElement.FirstLine + 1).Select(ConvertLineVisitStatus).ToList();
            Path = codeFile.Path;
            var methodMetrics = codeFile.MethodMetrics.FirstOrDefault(methodMetric => methodMetric.FullName == codeElement.FullName);
            if (methodMetrics != null)
            {
                var metricTypes = this.SetMetrics(methodMetrics.Metrics);
                PalmmediaReportResult.AddMetricTypes(metricTypes);
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

        private LineVisitStatus ConvertLineVisitStatus(Palmmedia.ReportGenerator.Core.Parser.Analysis.LineVisitStatus lvs)
        {
            switch (lvs)
            {
                case Palmmedia.ReportGenerator.Core.Parser.Analysis.LineVisitStatus.NotCoverable:
                    return LineVisitStatus.NotCoverable;
                case Palmmedia.ReportGenerator.Core.Parser.Analysis.LineVisitStatus.NotCovered:
                    return LineVisitStatus.NotCovered;
                case Palmmedia.ReportGenerator.Core.Parser.Analysis.LineVisitStatus.PartiallyCovered:
                    return LineVisitStatus.PartiallyCovered;
                case Palmmedia.ReportGenerator.Core.Parser.Analysis.LineVisitStatus.Covered:
                    return LineVisitStatus.Covered;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public CodeElementType CodeElementType { get; }
        public string Name { get; }
        public int StartLine { get; }
        public IReadOnlyCollection<LineVisitStatus> LineVisitStatuses { get; }
        public string Path { get; }
        public int BlocksCovered { get; set; }
        public int BlocksNotCovered { get; set; }
        public int CyclomaticComplexity { get; set; }
        public int NPathComplexity { get; set; }
        public decimal CrapScore { get; set; }
    }

    class PalmmediaSourceFile : ISourceFile
    {
        public PalmmediaSourceFile(string path, IEnumerable<IClass> classes)
        {
            Path = path;
            Classes = classes.ToList();
        }
        public string Path { get; }
        public IReadOnlyCollection<IClass> Classes { get; }
    }

    public class PalmmediaCodeFileClass
    {
        public PalmmediaCodeFileClass(CodeFile codeFile, PalmmediaClass pamlmediaClass)
        {
            CodeFile = codeFile;
            PalmmediaClass = pamlmediaClass;
        }

        public CodeFile CodeFile { get; }
        public PalmmediaClass PalmmediaClass { get; }
    }

    class PalmmediaReportResult : IReportResult
    {
        private IDirectory directory;
        public PalmmediaReportResult(ParserResult parserResult)
        {
            staticMetricTypes.Clear();
            if (parserResult.SupportsBranchCoverage)
            {
                staticMetricTypes.Add(MetricType.Branches);
            }
            palmmediaAssemblies = parserResult.Assemblies.Select(a => new PalmmediaAssembly(a)).ToList();
        }
        public List<MetricType> MetricTypes => staticMetricTypes.ToList();
        private static readonly HashSet<MetricType> staticMetricTypes = new HashSet<MetricType>();
        public static void AddMetricTypes(List<MetricType> metricTypes)
        {
            metricTypes.ForEach(metricType => staticMetricTypes.Add(metricType));
        }

        private readonly IReadOnlyCollection<PalmmediaAssembly> palmmediaAssemblies;
        public IReadOnlyCollection<IAssembly> Assemblies => palmmediaAssemblies;
        public IDirectory Directory
        {
            get
            {
                return directory ?? (directory = CreateDirectory());
            }
        }


        private IEnumerable<PalmmediaSourceFile> GetSourceFiles()
        {
            return this.palmmediaAssemblies.SelectMany(a =>
            {
                return a.PalmmediaClasses.SelectMany(pc => pc
                    .CodeFiles
                    .Select(cf => new PalmmediaCodeFileClass(cf, new PalmmediaClass(pc.DisplayName, cf))));
            }).GroupBy(pcfc => pcfc.CodeFile.Path).Select(g => new PalmmediaSourceFile(g.Key, g.Select(pcfc => pcfc.PalmmediaClass)));
        }
        private IDirectory CreateDirectory()
        {
            return CreateDirectory(GetSourceFiles());
        }

        private IDirectory CreateDirectory(IEnumerable<ISourceFile> sourceFiles)
        {
            return DirectoryResultsTreeBuilder.BuildDirectoryTree(sourceFiles.ToList());
        }


    }

    [Export(typeof(IFCCReportGenerator))]
    class PalmmediaReportGenerator
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
