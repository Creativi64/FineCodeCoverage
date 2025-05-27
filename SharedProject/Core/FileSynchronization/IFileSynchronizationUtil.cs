using System.Collections.Generic;

namespace FineCodeCoverage.Engine.FileSynchronization
{
    internal interface IFileSynchronizationUtil
    {
        List<string> Synchronize(
            string sourceFolder, string destinationFolder, string fineCodeCoverageFolderName);
    }
}
