using System.Threading.Tasks;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Impl
{
    internal interface ITestOperationFactory
    {
        Task<ITestOperation> CreateAsync(IOperation operation);
    }
}


