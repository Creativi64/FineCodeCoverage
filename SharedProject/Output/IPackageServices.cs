using System;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using System.Threading;

namespace FineCodeCoverage.Output
{
    internal interface IPackageServices
    {
        CancellationToken DisposalToken { get; }

        void RunAsyncWithExceptionLogging<T>(Func<Task<T>> asyncMethod);

        void ShowOptionPage(Type optionsPageType);

        Task<ToolWindowPane> ShowToolWindowAsync(Type toolWindowType, int id, bool create, CancellationToken cancellationToken);
    }
}
