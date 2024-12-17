using System;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IToolWindowService))]
    [Export(typeof(IToolWindowServiceInit))]
    internal class ToolWindowService : IToolWindowService, IToolWindowServiceInit
    {
        public AsyncPackage Package { get; set; }

        public Task<ToolWindowPane> ShowToolWindowAsync(Type toolWindowType, int id, bool create, CancellationToken cancellationToken)
            => this.Package.ShowToolWindowAsync(toolWindowType, id, create, cancellationToken);

        public Task<ToolWindowPane> ShowToolWindowAsync(Type toolWindowType, int id, bool create)
            => this.ShowToolWindowAsync(toolWindowType, id, create, this.Package.DisposalToken);
    }
}
