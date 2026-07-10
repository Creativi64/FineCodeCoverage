using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Vs.WindowServices.ToolWindows
{
    internal interface IToolWindowService
    {
        Task<ToolWindowPane> ShowToolWindowAsync(Type toolWindowType, int id, bool create, CancellationToken cancellationToken);

        Task<ToolWindowPane> ShowToolWindowAsync(Type toolWindowType, int id, bool create);
    }
}
