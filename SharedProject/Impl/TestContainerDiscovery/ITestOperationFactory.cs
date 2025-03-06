using Microsoft.VisualStudio.TestWindow.Extensibility;
using System.Threading.Tasks;

namespace FineCodeCoverage.Impl
{
    internal interface ITestOperationFactory
    {
        Task<ITestOperation> CreateAsync(IOperation operation);
    }
}


