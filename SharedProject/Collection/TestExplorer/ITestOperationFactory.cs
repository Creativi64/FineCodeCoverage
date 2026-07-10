using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Collection.TestExplorer
{
    internal interface ITestOperationFactory
    {
        ITestOperation Create(IOperation operation);
    }
}
