using System.Threading.Tasks;

namespace FineCodeCoverage.Engine.Coverlet
{
    internal class CoverletToolDetails
    {
        public string Version { get; set; }
        public string Command { get; set; }
    }

    internal interface IDotNetToolListCoverlet
    {
        Task<CoverletToolDetails> LocalAsync(string directory);
        Task<CoverletToolDetails> GlobalAsync();
        Task<CoverletToolDetails> GlobalToolsPathAsync(string directory);
    }
}
