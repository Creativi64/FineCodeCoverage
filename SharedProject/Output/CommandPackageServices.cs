using System;
using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Output
{
    internal class CommandPackageServices : ICommandPackageServices
    {
        private readonly AsyncPackage package;
        private readonly ILogger logger;

        public CommandPackageServices(AsyncPackage package, IMenuCommandService menuCommandService, ILogger logger)
        {
            this.MenuCommandService = menuCommandService;
            this.logger = logger;
            this.package = package;
        }

        public CancellationToken DisposalToken { get; }
        public IMenuCommandService MenuCommandService { get; }

        public void ShowOptionPage(Type optionsPageType) => this.package.ShowOptionPage(optionsPageType);

        public Task<ToolWindowPane> ShowToolWindowAsync(
            Type toolWindowType,
            int id,
            bool create,
            CancellationToken cancellationToken
        ) => this.package.ShowToolWindowAsync(toolWindowType, id, create, cancellationToken);

        public static async Task<ICommandPackageServices> CreateAsync(AsyncPackage package, ILogger logger)
        {
            var menuCommandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as IMenuCommandService;
            return new CommandPackageServices(package, menuCommandService, logger);
        }

        public void RunAsyncWithExceptionLogging<T>(Func<Task<T>> asyncMethod)
            => this.package.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    _ = await asyncMethod();
                }
                catch (Exception exception)
                {
                    await this.logger.LogAsync(exception.ToString());
                }
            }).FileAndForget("RunAsyncWithExceptionLogging");
    }
}
