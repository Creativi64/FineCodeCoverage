using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.Engine
{
    /// <summary>
    /// What one collector produced for a test run.
    /// </summary>
    /// <remarks>
    /// A solution can mix classic VSTest projects with Microsoft.Testing.Platform projects, which are
    /// collected by entirely different mechanisms.  Each collector returns its own result and the
    /// caller combines them, so that a run produces a single report covering every project.
    /// </remarks>
    public sealed class CoverageCollectionResult
    {
        public static CoverageCollectionResult Empty { get; } = new CoverageCollectionResult(
            new List<string>(),
            new List<ICoverageProject>());

        public CoverageCollectionResult(
            IReadOnlyList<string> coberturaFiles,
            IReadOnlyList<ICoverageProject> coverageProjects)
        {
            CoberturaFiles = coberturaFiles;
            CoverageProjects = coverageProjects;
        }

        public IReadOnlyList<string> CoberturaFiles { get; }

        /// <summary>
        /// The projects this collector was responsible for, whether or not they produced a file.
        /// </summary>
        public IReadOnlyList<ICoverageProject> CoverageProjects { get; }

        public bool HasCoverage => CoberturaFiles.Count > 0;

        public static CoverageCollectionResult Combine(params CoverageCollectionResult[] results)
        {
            IEnumerable<CoverageCollectionResult> present = results.Where(result => result != null);
            return new CoverageCollectionResult(
                present.SelectMany(result => result.CoberturaFiles).Distinct().ToList(),
                present.SelectMany(result => result.CoverageProjects).ToList());
        }
    }
}
