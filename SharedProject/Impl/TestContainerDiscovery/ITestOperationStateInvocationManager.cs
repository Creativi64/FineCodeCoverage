using Microsoft.VisualStudio.TestWindow.Extensibility;
using System.Threading.Tasks;

namespace FineCodeCoverage.Impl
{
    internal interface ITestOperationStateInvocationManager
    {
        Task<bool> CanInvokeAsync(TestOperationStates testOperationState);
    }
}
