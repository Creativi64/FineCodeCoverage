using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Github;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommand))]
    internal sealed class NewIssueCommand : CommandBase
    {
        private readonly IFCCGithubService fccGithubService;

        protected override int CommandId { get; } = PackageIds.cmdidNewIssueCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public NewIssueCommand(IFCCGithubService fccGithubService)
        {
            this.fccGithubService = fccGithubService;
        }

        protected override void Execute(object sender, EventArgs e) => this.fccGithubService.NewIssue();
    }
}