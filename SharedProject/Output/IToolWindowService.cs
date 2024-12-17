using System;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using System.Threading;

namespace FineCodeCoverage.Output
{
    internal interface IToolWindowService
    {
        Task<ToolWindowPane> ShowToolWindowAsync(Type toolWindowType, int id, bool create, CancellationToken cancellationToken);
        Task<ToolWindowPane> ShowToolWindowAsync(Type toolWindowType, int id, bool create);
    }
}
