using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using System.Threading;

namespace FineCodeCoverage.Output
{
    internal class CommandPackageServices : ICommandPackageServices
    {
        private readonly AsyncPackage package;
        private readonly ILogger logger;

        public CommandPackageServices(AsyncPackage package, IMenuCommandService menuCommandService, ILogger logger)
        {
            MenuCommandService = menuCommandService;
            this.logger = logger;
            this.package = package;
        }

        public CancellationToken DisposalToken { get; }
        public IMenuCommandService MenuCommandService { get; }

        public void ShowOptionPage(Type optionsPageType)
        {
            this.package.ShowOptionPage(optionsPageType);
        }

        public Task<ToolWindowPane> ShowToolWindowAsync(Type toolWindowType, int id, bool create, CancellationToken cancellationToken)
        {
            return package.ShowToolWindowAsync(toolWindowType, id, create, cancellationToken);
        }

        public static async Task<ICommandPackageServices> CreateAsync(AsyncPackage package, ILogger logger)
        {
            var menuCommandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as IMenuCommandService;
            return new CommandPackageServices(package, menuCommandService, logger);
        }

        public void RunAsyncWithExceptionLogging<T>(Func<Task<T>> asyncMethod)
        {
            package.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    await asyncMethod();
                }
                catch (Exception exception)
                {
                    await logger.LogAsync(exception.ToString());
                }
            }).FileAndForget("");
        }
    }
}
