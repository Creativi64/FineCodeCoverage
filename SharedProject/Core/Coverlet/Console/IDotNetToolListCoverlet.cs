using System.Threading.Tasks;

namespace FineCodeCoverage.Engine.Coverlet
{
    internal interface IDotNetToolListCoverlet
    {
        Task<CoverletDotNetToolDetails> LocalAsync(string directory);
        Task<CoverletDotNetToolDetails> GlobalAsync();
        Task<CoverletDotNetToolDetails> GlobalToolsPathAsync(string directory);
    }
}