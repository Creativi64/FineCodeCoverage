using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Core.Model
{
    [Export(typeof(ICoverageSettingsOptionsProvider))]
    internal sealed class CoverageSettingsOptionsProvider : ICoverageSettingsOptionsProvider
    {
        private readonly List<IProvideOptions> _optionsProviders = new List<IProvideOptions>();

        [ImportingConstructor]
        public CoverageSettingsOptionsProvider(
            IOptionsProvider<IncludesExcludesOptions> includesExcludesOptionsProvider,
            IOptionsProvider<RunOptions> runOptionsProvider,
            IOptionsProvider<CoverletOptions> coverletOptionsProvider,
            IOptionsProvider<OpenCoverOptions> openCoverOptionsProvider)
        {
            _optionsProviders.Add(includesExcludesOptionsProvider);
            _optionsProviders.Add(runOptionsProvider);
            _optionsProviders.Add(coverletOptionsProvider);
            _optionsProviders.Add(openCoverOptionsProvider);
        }

        public IEnumerable<object> Get() => _optionsProviders.Select(g => g.Options);
    }
}
