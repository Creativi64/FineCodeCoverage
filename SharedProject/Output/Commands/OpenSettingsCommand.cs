using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ICommandInitializer))]
    internal sealed class OpenSettingsCommand : CommandInitializerBase
    {
        private readonly IOpenOptionPageTypeProvider _openOptionPageTypeProvider;

        [ImportingConstructor]
        public OpenSettingsCommand(IOpenOptionPageTypeProvider openOptionPageTypeProvider)
            => this._openOptionPageTypeProvider = openOptionPageTypeProvider;

        protected override int CommandId { get; } = PackageIds.cmdidOpenSettingsCommand;
        protected override Guid CommandSet { get; } = PackageGuids.guidFCCPackageCmdSet;

        protected override void Execute(object sender, EventArgs e)
            => this.PackageServices.ShowOptionPage(this._openOptionPageTypeProvider.Get());
    }
}
