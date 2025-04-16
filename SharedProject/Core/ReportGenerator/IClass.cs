using System.Collections.Generic;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    public interface IClass
    {
        string DisplayName { get; }
        IReadOnlyDictionary<string, IReadOnlyList<ICodeElement>> FileCodeElements { get; }
        IReadOnlyList<ICodeElement> CodeElements { get; }
    }
}
