using System.ComponentModel.Design;

namespace FineCodeCoverage.Output
{
    internal interface ICommandPackageServices : IPackageServices
    {
        IMenuCommandService MenuCommandService { get; }
    }
}
