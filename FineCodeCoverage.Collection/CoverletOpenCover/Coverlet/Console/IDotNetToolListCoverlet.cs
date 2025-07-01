using System.Threading.Tasks;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Coverlet.Console
{
    internal interface IDotNetToolListCoverlet
    {
        Task<CoverletDotNetToolDetails> LocalAsync(string directory);

        Task<CoverletDotNetToolDetails> GlobalAsync();

        Task<CoverletDotNetToolDetails> GlobalToolsPathAsync(string directory);
    }
}
