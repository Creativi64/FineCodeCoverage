using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Github;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class OpenFCCGithubCommand : CommandInitializerBase
    {
        private readonly IFCCGithubService _fccGithubService;

        protected override int CommandId { get; } = PackageIds.cmdidOpenFCCGithubCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        [ImportingConstructor]
        public OpenFCCGithubCommand(IFCCGithubService fccGithubService) => this._fccGithubService = fccGithubService;

        protected override void Execute(object sender, EventArgs e) => this._fccGithubService.Navigate();
    }
}