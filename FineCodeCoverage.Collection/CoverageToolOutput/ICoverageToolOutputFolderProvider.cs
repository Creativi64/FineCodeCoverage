using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine
{
    public interface ICoverageToolOutputFolderProvider
    {
        string Provide(List<ICoverageProject> coverageProjects);
    }
}
