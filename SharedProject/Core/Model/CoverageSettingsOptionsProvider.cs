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
        public CoverageSettingsOptionsProvider(
            IOptionsProvider<AppOptions> appOptionsProvider,
            IOptionsProvider<RunOptions> runOptionsProvider,
            IOptionsProvider<CoverletOptions> coverletOptionsProvider
        )
        {
            optionsGetters.Add(appOptionsProvider);
            optionsGetters.Add(runOptionsProvider);
            optionsGetters.Add(coverletOptionsProvider);
        }

        public IEnumerable<object> Get() => this.optionsGetters.Select(g => g.GetOptionsAsObject());
    }
}
