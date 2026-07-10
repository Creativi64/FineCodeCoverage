using System.ComponentModel.Design;

namespace FineCodeCoverage.Vs.Commands.CommandInitializer
{
    internal interface ICommandPackageServices : IPackageServices
    {
        IMenuCommandService MenuCommandService { get; }
    }
}
