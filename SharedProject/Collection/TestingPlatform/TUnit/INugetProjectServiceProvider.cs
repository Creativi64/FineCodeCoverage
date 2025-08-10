using Microsoft.VisualStudio.Threading;
using NuGet.VisualStudio.Contracts;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    internal interface INugetProjectServiceProvider
    {
        AsyncLazy<INuGetProjectService> LazyNugetProjectService { get; }
    }
}
