using FineCodeCoverage.Editor.DynamicCoverage;
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
        IReadOnlyList<ICodeElement> CodeElements { get; }
    }



    public interface ICodeElement
    {
        CodeElementType CodeElementType { get; }
        string Name { get; }
        int StartLine { get; }
        string Path { get; }
        IReadOnlyList<LineVisitStatus> LineVisitStatuses { get; }
        int BlocksCovered { get; }
        int BlocksNotCovered { get; }
        int CyclomaticComplexity { get; }
        int NPathComplexity { get; }
        decimal CrapScore { get; }

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

    internal interface IReportResult : IFileLineCoverage
    {
        IReadOnlyList<IAssembly> Assemblies { get; }
        IDirectory Directory { get; }
        List<MetricType> MetricTypes { get; }
    }
}
