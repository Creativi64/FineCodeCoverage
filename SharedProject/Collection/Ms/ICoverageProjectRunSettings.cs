using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.Ms
{
    internal interface ICoverageProjectRunSettings
    {
        ICoverageProject CoverageProject { get; set; }

        string RunSettings { get; set; }
    }
}
