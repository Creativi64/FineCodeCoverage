using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
namespace FineCodeCoverage.Output
{
    internal abstract class CommandInitializerBase : ICommandInitializer
    {
        protected abstract int CommandId { get; }
        protected abstract Guid CommandSet { get; }
        protected MenuCommand Command { get; private set; }
        protected IPackageServices PackageServices { get; private set; }
        public async Task InitializeAsync(ICommandPackageServices commandPackageServices)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(commandPackageServices.DisposalToken);
            var menuCommandID = new CommandID(this.CommandSet, this.CommandId);
            this.Command = new MenuCommand(this.Execute, menuCommandID);
            commandPackageServices.MenuCommandService.AddCommand(this.Command);
            this.PackageServices = commandPackageServices;
            this.Initialized();
        }

        protected virtual void Initialized() { }

        protected abstract void Execute(object sender, EventArgs e);
    }
}