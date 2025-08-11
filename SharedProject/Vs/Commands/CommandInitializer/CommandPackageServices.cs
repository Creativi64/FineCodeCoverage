using System;
using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Vs.WindowServices.ToolWindows;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Vs.Commands.CommandInitializer
{
    internal sealed class CommandPackageServices : ICommandPackageServices
    {
        private readonly AsyncPackage _package;
        private readonly IToolWindowService _toolWindowService;
        private readonly ILogger _logger;

        public CommandPackageServices(
            AsyncPackage package,
            IToolWindowService toolWindowService,
            IMenuCommandService menuCommandService,
            ILogger logger)
        {
            MenuCommandService = menuCommandService;
            _logger = logger;
            _package = package;
            _toolWindowService = toolWindowService;
        }

        public CancellationToken DisposalToken { get; }

        public IMenuCommandService MenuCommandService { get; }

        public void ShowOptionPage(Type optionsPageType) => _package.ShowOptionPage(optionsPageType);

        public Task<ToolWindowPane> ShowToolWindowAsync(
            Type toolWindowType,
            int id,
            bool create,
            CancellationToken cancellationToken)
            => _toolWindowService.ShowToolWindowAsync(toolWindowType, id, create, cancellationToken);

        public static async Task<ICommandPackageServices> CreateAsync(
            AsyncPackage package,IToolWindowService toolWindowService, ILogger logger)
        {
            var menuCommandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as IMenuCommandService;
            return new CommandPackageServices(package, toolWindowService, menuCommandService, logger);
        }

        public void RunAsyncWithExceptionLogging<T>(Func<Task<T>> asyncMethod)
            => _package.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    _ = await asyncMethod();
                }
                catch (Exception exception)
                {
                    await _logger.LogAsync(exception.ToString());
                }
            }).FileAndForget("RunAsyncWithExceptionLogging");
    }
}
