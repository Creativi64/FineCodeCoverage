using System.Collections.Generic;
using System.Threading.Tasks;

namespace FineCodeCoverage.VSAbstractions.OutputWindow
{
    public interface ILogger
    {
        void LogFileAndForget(params string[] message);

        Task LogAsync(params string[] message);

        Task LogAsync(IEnumerable<string> message);
    }
}
