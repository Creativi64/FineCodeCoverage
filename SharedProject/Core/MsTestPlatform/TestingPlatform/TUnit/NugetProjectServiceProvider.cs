using System;
using System.ComponentModel.Composition;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.ServiceBroker;
using Microsoft.VisualStudio.Threading;
using NuGet.VisualStudio.Contracts;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(INugetProjectServiceProvider))]
    internal class NugetProjectServiceProvider : INugetProjectServiceProvider
    {
        public AsyncLazy<INuGetProjectService> LazyNugetProjectService { get; }

        [ImportingConstructor]
        public NugetProjectServiceProvider(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
        ) => LazyNugetProjectService = new AsyncLazy<INuGetProjectService>(async () =>
            {
                IBrokeredServiceContainer brokeredServiceContainer = serviceProvider.GetService<SVsBrokeredServiceContainer, IBrokeredServiceContainer>();
                IServiceBroker serviceBroker = brokeredServiceContainer.GetFullAccessServiceBroker();
#pragma warning disable ISB001 // Dispose of proxies
                INuGetProjectService nugetProjectService = await serviceBroker.GetProxyAsync<INuGetProjectService>(NuGetServices.NuGetProjectServiceV1);
#pragma warning restore ISB001 // Dispose of proxies
                return nugetProjectService;
            }, ThreadHelper.JoinableTaskFactory);
    }
}
