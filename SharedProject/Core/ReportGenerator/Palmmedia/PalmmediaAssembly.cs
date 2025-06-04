using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Engine.ReportGenerator
{

    internal class PalmmediaAssembly : IAssembly
    {
        public PalmmediaAssembly(Assembly assemblyReport)
        {
            this.Name = assemblyReport.Name;
            this.ShortName = assemblyReport.ShortName;
            this.PalmmediaClasses = assemblyReport.Classes.Select(c => new PalmmediaClass(c)).ToList();
            this.Classes = this.PalmmediaClasses;
        }
        public List<PalmmediaClass> PalmmediaClasses { get; }
        public string Name { get; }
        public string ShortName { get; }
        public IReadOnlyList<IClass> Classes { get; }
    }
}
