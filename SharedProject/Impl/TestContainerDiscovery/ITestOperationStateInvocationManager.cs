using System.Threading.Tasks;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Impl
{
    internal interface ITestOperationStateInvocationManager
    {
        Task<bool> CanInvokeAsync(TestOperationStates testOperationState);
    }
}
