using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Options.IncludesExcludes;
using FineCodeCoverage.Options.Run;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    [Export(typeof(ICoverageSettingsOptionsProvider))]
    internal sealed class CoverageSettingsOptionsProvider : ICoverageSettingsOptionsProvider
    {
        private readonly List<IProvideOptions> _optionsProviders = new List<IProvideOptions>();

        [ImportingConstructor]
        public CoverageSettingsOptionsProvider(
            IOptionsProvider<IncludesExcludesOptions> includesExcludesOptionsProvider,
            IOptionsProvider<RunOptions> runOptionsProvider)
        {
            _optionsProviders.Add(includesExcludesOptionsProvider);
            _optionsProviders.Add(runOptionsProvider);
        }

        public IEnumerable<object> Get() => _optionsProviders.Select(g => g.Options);
    }
}
