using System.Collections.Generic;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.ReportGeneration
{
    internal interface IHotspotsWriter
    {
        void WriteHotspotsToXml(IEnumerable<IAssembly> reportAssemblies, string hotspotsPath);
    }
}
