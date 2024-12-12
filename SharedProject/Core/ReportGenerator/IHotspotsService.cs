using System.Collections.Generic;
using FineCodeCoverage.Engine.ReportGenerator;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;

namespace FineCodeCoverage.ReportGeneration
{
    internal interface IHotspotsService
    {
        void WriteHotspotsToXml(IEnumerable<IAssembly> reportAssemblies, string hotspotsPath);
    }
}
