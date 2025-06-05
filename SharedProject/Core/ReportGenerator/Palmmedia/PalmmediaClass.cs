using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    internal class PalmmediaClass : IClass
    {
        public PalmmediaClass(Class classReport)
        {
            DisplayName = classReport.Name;
            FileCodeElements = classReport.Files.ToDictionary(
                cf => cf.Path,
                f => (IReadOnlyList<ICodeElement>)f.CodeElements.Select(ce => new PalmmediaCodeElement(ce, f) as ICodeElement).ToList());
            CodeElements = FileCodeElements.Values.SelectMany(ces => ces).ToList();
        }

        public string DisplayName { get; }

        public IReadOnlyDictionary<string, IReadOnlyList<ICodeElement>> FileCodeElements { get; }

        public IReadOnlyList<ICodeElement> CodeElements { get; }
    }
}
