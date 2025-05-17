using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Options;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FineCodeCoverage.Core.Model
{
    [Export(typeof(ICoverageSettingsOptionsProvider))]
    internal class CoverageSettingsOptionsProvider : ICoverageSettingsOptionsProvider
    {
        private readonly List<IProvideOptions> optionsProviders = new List<IProvideOptions>();
        [ImportingConstructor]
        public CoverageSettingsOptionsProvider(
            IOptionsProvider<IncludesExcludesOptions> includesExcludesOptionsProvider,
            IOptionsProvider<RunOptions> runOptionsProvider,
            IOptionsProvider<CoverletOptions> coverletOptionsProvider,
            IOptionsProvider<OpenCoverOptions> openCoverOptionsProvider
        )
        {
            optionsProviders.Add(includesExcludesOptionsProvider);
            optionsProviders.Add(runOptionsProvider);
            optionsProviders.Add(coverletOptionsProvider);
            optionsProviders.Add(openCoverOptionsProvider);
        }

        public IEnumerable<object> Get() => this.optionsProviders.Select(g => g.Options);
    }
}
