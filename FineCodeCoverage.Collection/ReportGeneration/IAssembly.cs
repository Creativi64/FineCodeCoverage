using System.Collections.Generic;

namespace FineCodeCoverage.Collection.ReportGeneration
{
    public interface IAssembly
    {
        string Name { get; }

        string ShortName { get; }

        IReadOnlyList<IClass> Classes { get; }
    }
}
