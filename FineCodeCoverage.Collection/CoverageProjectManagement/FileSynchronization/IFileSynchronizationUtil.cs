using System.Collections.Generic;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.FileSynchronization
{
    public interface IFileSynchronizationUtil
    {
        List<string> Synchronize(
            string sourceFolder, string destinationFolder, string fineCodeCoverageFolderName);
    }
}
