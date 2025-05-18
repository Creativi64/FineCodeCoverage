using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output
{
    internal interface ICommand
    {
        Task InitializeAsync(CancellationToken cancellationToken, IMenuCommandService commandService);
    }

    internal abstract class CommandBase : ICommand
    {
        protected abstract int CommandId { get; }
        protected abstract Guid CommandSet { get; }
        protected MenuCommand Command { get; private set; }
        public async Task InitializeAsync(CancellationToken cancellationToken, IMenuCommandService commandService)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var menuCommandID = new CommandID(CommandSet, CommandId);
            Command = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(Command);
            FinalInitialization();
        }

        protected virtual void FinalInitialization() { }

        protected abstract void Execute(object sender, EventArgs e);
    }
}
