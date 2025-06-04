using System.Collections.Generic;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    public interface IAssembly
    {
        string Name { get; }
        string ShortName { get; }
        IReadOnlyList<IClass> Classes { get; }
    }
}
