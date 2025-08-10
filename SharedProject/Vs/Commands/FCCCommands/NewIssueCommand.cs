using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Feedback.Github;
using FineCodeCoverage.Vs.Commands.CommandInitializer;

namespace FineCodeCoverage.Vs.Commands.FCCCommands
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class NewIssueCommand : CommandInitializerBase
    {
        private readonly IFCCGithubService _fccGithubService;

        protected override int CommandId { get; } = PackageIds.cmdidNewIssueCommand;

        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public NewIssueCommand(IFCCGithubService fccGithubService) => _fccGithubService = fccGithubService;

        protected override void Execute(object sender, EventArgs e) => _fccGithubService.NewIssue();
    }
}
