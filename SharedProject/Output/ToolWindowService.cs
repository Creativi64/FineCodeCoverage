using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IToolWindowService))]
    [Export(typeof(IToolWindowServiceInit))]
    internal class ToolWindowService : IToolWindowService, IToolWindowServiceInit
    {
        public AsyncPackage Package { get; set; }

        public Task<ToolWindowPane> ShowToolWindowAsync(Type toolWindowType, int id, bool create, CancellationToken cancellationToken)
            => Package.ShowToolWindowAsync(toolWindowType, id, create, cancellationToken);

        public Task<ToolWindowPane> ShowToolWindowAsync(Type toolWindowType, int id, bool create)
            => ShowToolWindowAsync(toolWindowType, id, create, Package.DisposalToken);
    }
}
