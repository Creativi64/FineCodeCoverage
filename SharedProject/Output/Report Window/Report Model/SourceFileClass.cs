using System.Collections.Generic;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Output
{
    internal class SourceFileClass : IClass
    {
        public SourceFileClass(string displayName, string path, IReadOnlyList<ICodeElement> codeElements)
        {
            DisplayName = displayName;
            CodeElements = codeElements;
            FileCodeElements = new Dictionary<string, IReadOnlyList<ICodeElement>>
            {
                {path, codeElements},
            };
        }
        public string DisplayName { get; }
        public IReadOnlyList<ICodeElement> CodeElements { get; }
        public IReadOnlyDictionary<string, IReadOnlyList<ICodeElement>> FileCodeElements { get; }
    }
}
