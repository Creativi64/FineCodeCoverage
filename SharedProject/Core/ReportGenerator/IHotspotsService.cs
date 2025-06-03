using System.Collections.Generic;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.ReportGeneration
{
    internal interface IHotspotsService
    {
        void WriteHotspotsToXml(IEnumerable<IAssembly> reportAssemblies, string hotspotsPath);
    }
}