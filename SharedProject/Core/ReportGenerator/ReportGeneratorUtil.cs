using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Palmmedia.ReportGenerator.Core;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using PalmediaVerbosityLevel = Palmmedia.ReportGenerator.Core.Logging.VerbosityLevel;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    public interface IAssembly
    {
        string Name { get; }
        string ShortName { get; }
        IReadOnlyCollection<IClass> Classes { get; }
    }

    public interface IClass
    {
        string DisplayName { get; }
        IReadOnlyCollection<ICodeFile> Files { get; }
        
    }

    public interface ICodeFile
    {
        string Path { get; }
        IReadOnlyCollection<ICodeElement> CodeElements { get; }
    }

    public interface ICodeElement
    {
        CodeElementType CodeElementType { get; }
        string Name { get; }
        int StartLine { get; }
        IReadOnlyCollection<LineVisitStatus> LineVisitStatuses { get; }
    }

    interface IReportResult
    {
        IReadOnlyCollection<IAssembly> Assemblies { get; }
    }

    public class PalmediaAssembly : IAssembly
    {
        public PalmediaAssembly(Assembly assemblyReport)
        {
            Name = assemblyReport.Name;
            ShortName = assemblyReport.ShortName;
        }

        public string Name { get; }
        public string ShortName { get; }
        public IReadOnlyCollection<IClass> Classes { get; }
    }

    public class PalmediaClass : IClass
    {
        public PalmediaClass(Class classReport)
        {
            DisplayName = classReport.DisplayName;
            Files = classReport.Files.Select(f => new PalmediaFile(f)).ToList<ICodeFile>();
        }

        public string DisplayName { get; }
        public IReadOnlyCollection<ICodeFile> Files { get; }
    }

    public class PalmediaFile : ICodeFile
    {
        public PalmediaFile(CodeFile codeFile)
        {
            Path = codeFile.Path;
            CodeElements = codeFile.CodeElements.Select(ce => new PalmediaCodeElement(ce, codeFile)).ToList<ICodeElement>();
        }

        public string Path { get; }
        public IReadOnlyCollection<ICodeElement> CodeElements { get; }
    }

    public class PalmediaCodeElement : ICodeElement
    {
        public PalmediaCodeElement(CodeElement codeElement,CodeFile codeFile)
        {
            CodeElementType = ConvertCodeElementType(codeElement.CodeElementType);
            Name = codeElement.Name;
            StartLine = codeElement.FirstLine;

            LineVisitStatuses = codeFile.LineVisitStatus.Skip(codeElement.FirstLine)
            .Take(codeElement.LastLine - codeElement.FirstLine + 1).Select(ConvertLineVisitStatus).ToList();

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
    }

    public enum LineVisitStatus
    {
        /// <summary>
        /// Line can not be covered.
        /// </summary>
        NotCoverable,

        /// <summary>
        /// Line was not covered.
        /// </summary>
        NotCovered,

        /// <summary>
        /// Line was partially covered.
        /// </summary>
        PartiallyCovered,

        /// <summary>
        /// Line was covered.
        /// </summary>
        Covered
    }

    public enum CodeElementType
    {
        /// <summary>
        /// Represents a property.
        /// </summary>
        Property,

        /// <summary>
        /// Represents a method.
        /// </summary>
        Method
    }

    class PalmediaReportResult : IReportResult
    {
        public PalmediaReportResult(ParserResult parserResult)
        {
            Assemblies = new ReadOnlyCollection<IAssembly>(parserResult.Assemblies.Select(a => new PalmediaAssembly(a)).ToList<IAssembly>());
        }

        public IReadOnlyCollection<IAssembly> Assemblies { get; private set; }
    }

    interface IFCCReportGenerator
    {
        void SetLogger(VerbosityLevel verbosityLevel, Action<VerbosityLevel, string> logger);
        IReportResult Generate(IEnumerable<string> coverageFiles,string reportDirectory, IEnumerable<string> reportTypes);
    }

    [Export(typeof(IFCCReportGenerator))]
    class PalmediaReportGenerator : IFCCReportGenerator
    {
        private readonly Regex fileDoesNotExistAnymoreRegex = new Regex(@"File '.*' does not exist \(any more\)\.", RegexOptions.Compiled);
        private readonly IHtmlFilesToFolder htmlFilesToFolder;

        [ImportingConstructor]
        public PalmediaReportGenerator(
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
                new ReadOnlyCollection<string>(new string[] { "HtmlSummary"}),
                empty,
                empty,
                empty,
                empty,
                verbosityLevel.ToString(),
                ""
                );

            var parser = new CoverageReportParser(1, 1, Enumerable.Empty<string>(), defaultFilter,defaultFilter, defaultFilter);
            ReadOnlyCollection<string> collection = new ReadOnlyCollection<string>(coverageFiles.ToList());
            var parserResult =  parser.ParseFiles(collection);
            new Generator().GenerateReport(config, parserResult);
            htmlFilesToFolder.Collate(reportDirectory);
            return new PalmediaReportResult(parserResult);

        }
        private VerbosityLevel verbosityLevel;
        public void SetLogger(VerbosityLevel verbosityLevel, Action<VerbosityLevel, string> logger)
        {
            LoggerFactory.Configure((palmediaVerbosityLevel,message) =>
            {
                var shouldLog = true;
                if (palmediaVerbosityLevel != PalmediaVerbosityLevel.Error)
                {
                    Match matched = this.fileDoesNotExistAnymoreRegex.Match(message);
                    shouldLog = !matched.Success;
                    
                }
                if (shouldLog)
                {
                    logger((VerbosityLevel)palmediaVerbosityLevel, message);
                }
            });
        }
    }

    public enum VerbosityLevel
    {
        /// <summary>
        /// All messages are logged.
        /// </summary>
        Verbose,

        /// <summary>
        /// Only important messages are logged.
        /// </summary>
        Info,

        /// <summary>
        /// Only warnings and errors are logged.
        /// </summary>
        Warning,

        /// <summary>
        /// Only errors are logged.
        /// </summary>
        Error,

        /// <summary>
        /// Nothing is logged.
        /// </summary>
        Off
    }

    [Export(typeof(IReportGeneratorUtil))]
    internal partial class ReportGeneratorUtil : IReportGeneratorUtil
    {
        private readonly IFCCReportGenerator reportGenerator;

        [ImportingConstructor]
        public ReportGeneratorUtil(
            IFCCReportGenerator reportGenerator,
            ILogger logger
        )
        {
            this.reportGenerator = reportGenerator;
            this.reportGenerator.SetLogger(VerbosityLevel.Info, (level, message) =>
            {
                logger.Log(message);
            });
        }

        public ReportGeneratorResult Generate(
            IEnumerable<string> coverOutputFiles, 
            string reportOutputFolder, 
            CancellationToken cancellationToken)
        {
            var summaryResult = this.reportGenerator.Generate(coverOutputFiles, reportOutputFolder, new List<string> { "Cobertura", "HtmlSummary" });


            return  new ReportGeneratorResult
            {
                ReportResult = summaryResult,
                UnifiedXmlFile = Path.Combine(reportOutputFolder, "Cobertura.xml"),
            };
        }
    }
}
