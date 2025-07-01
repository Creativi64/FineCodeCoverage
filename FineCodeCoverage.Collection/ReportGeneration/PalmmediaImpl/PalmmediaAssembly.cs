using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Collection.ReportGeneration;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Collection.ReportGeneration.PalmmediaImpl
{
    internal sealed class PalmmediaAssembly : IAssembly
    {
        public PalmmediaAssembly(Assembly assemblyReport)
        {
            Name = assemblyReport.Name;
            ShortName = assemblyReport.ShortName;
            PalmmediaClasses = assemblyReport.Classes.Select(c => new PalmmediaClass(c)).ToList();
            Classes = PalmmediaClasses;
        }

        public List<PalmmediaClass> PalmmediaClasses { get; }

        public string Name { get; }

        public string ShortName { get; }

        public IReadOnlyList<IClass> Classes { get; }
    }
}
