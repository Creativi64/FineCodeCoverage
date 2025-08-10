using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    internal interface IBuildHelper
    {
        event EventHandler<BuildStartEndArgs> ExternalBuildEvent;

        Task<bool> BuildAsync(List<IVsHierarchy> projects, System.Threading.CancellationToken cancellationToken);
    }
}
