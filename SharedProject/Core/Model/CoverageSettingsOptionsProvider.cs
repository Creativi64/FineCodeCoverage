using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Core.Model
{
    [Export(typeof(ICoverageSettingsOptionsProvider))]
    internal class CoverageSettingsOptionsProvider : ICoverageSettingsOptionsProvider
    {
        private readonly List<IProvideOptions> _optionsProviders = new List<IProvideOptions>();
        [ImportingConstructor]
        public CoverageSettingsOptionsProvider(
            IOptionsProvider<IncludesExcludesOptions> includesExcludesOptionsProvider,
            IOptionsProvider<RunOptions> runOptionsProvider,
            IOptionsProvider<CoverletOptions> coverletOptionsProvider,
            IOptionsProvider<OpenCoverOptions> openCoverOptionsProvider
        )
        {
            this._optionsProviders.Add(includesExcludesOptionsProvider);
            this._optionsProviders.Add(runOptionsProvider);
            this._optionsProviders.Add(coverletOptionsProvider);
            this._optionsProviders.Add(openCoverOptionsProvider);
        }

        public IEnumerable<object> Get() => this._optionsProviders.Select(g => g.Options);
    }
}
