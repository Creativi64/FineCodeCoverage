using System.Collections.Generic;
using FineCodeCoverage.Collection.ReportGeneration;

namespace FineCodeCoverage.Hotspots
{
    public interface IHotspotsWriter
    {
        void WriteHotspotsToXml(IEnumerable<IAssembly> reportAssemblies, string hotspotsPath);
    }
}
