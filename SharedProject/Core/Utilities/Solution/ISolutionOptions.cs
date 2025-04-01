using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Utilities.Solution
{
    internal interface ISolutionOptions
    {
        Task<IEnumerable<string>> GetKeysAsync();
        void LoadOptions(string key, Stream stream);
        void SaveOptions(string key, Stream stream);
    }
}
