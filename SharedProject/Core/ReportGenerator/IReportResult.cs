using System.Collections.Generic;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    public interface IDirectory
    {
        string Name { get; }
        IReadOnlyList<IDirectory> SubDirectories { get; }
        IReadOnlyList<ISourceFile> SourceFiles { get; }
    }

    public interface ISourceFile
    {
        string Path { get; }
        IReadOnlyList<IClass> Classes { get; }
    }

    public interface IAssembly
    {
        string Name { get; }
        string ShortName { get; }
        IReadOnlyList<IClass> Classes { get; }
    }

    public interface IClass
    {
        string DisplayName { get; }
        IReadOnlyDictionary<string, List<ICodeElement>> FileCodeElements { get; }
        IReadOnlyList<ICodeElement> CodeElements { get; }
    }

    public enum CoverageType { Covered, Partial, NotCovered }
    public interface ICoberturaLine
    {
        int Number { get; }
        CoverageType CoverageType { get; }
    }
    public interface ICodeElement
    {
        CodeElementType CodeElementType { get; }
        string Name { get; }
        int StartLine { get; }
        string Path { get; }
        List<ICoberturaLine> Lines { get; }
        int BlocksCovered { get; }
        int BlocksNotCovered { get; }
        int CyclomaticComplexity { get; }
        int NPathComplexity { get; }
        decimal CrapScore { get; }

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

    public enum MetricType
    {
        NotMetricType,
        Crap,
        NPath,
        CyclomaticComplexity,
        BlocksCovered,
        BlocksNotCovered,
        Branches
    }

    internal interface IReportResult
    {
        IReadOnlyList<IAssembly> Assemblies { get; }
        IReadOnlyList<MetricType> MetricTypes { get; }
    }
}
