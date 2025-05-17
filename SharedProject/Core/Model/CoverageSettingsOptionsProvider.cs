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
        private readonly List<IOptionsGetter> optionsGetters = new List<IOptionsGetter>();
        [ImportingConstructor]
        public CoverageSettingsOptionsProvider(
            IOptionsProvider<IncludesExcludesOptions> includesExcludesOptionsProvider,
            IOptionsProvider<RunOptions> runOptionsProvider,
            IOptionsProvider<CoverletOptions> coverletOptionsProvider,
            IOptionsProvider<OpenCoverOptions> openCoverOptionsProvider
        )
        {
            optionsGetters.Add(includesExcludesOptionsProvider);
            optionsGetters.Add(runOptionsProvider);
            optionsGetters.Add(coverletOptionsProvider);
            optionsGetters.Add(openCoverOptionsProvider);
        }

        public IEnumerable<object> Get() => this.optionsGetters.Select(g => g.GetOptionsAsObject());
    }
}
