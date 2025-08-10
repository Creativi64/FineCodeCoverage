using System.Threading.Tasks;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Collection.TestExplorer
{
    internal interface ITestOperationStateInvocationManager
    {
        Task<bool> CanInvokeAsync(TestOperationStates testOperationState);
    }
}
