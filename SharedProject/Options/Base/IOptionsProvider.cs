using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FineCodeCoverage.Options
{
    internal interface ICoverageSettingsOptionsProvider
    {
        IEnumerable<object> Get();
    }

    [Export(typeof(ICoverageSettingsOptionsProvider))]
    internal class CoverageSettingsOptionsProvider : ICoverageSettingsOptionsProvider
    {
        private readonly List<IOptionsGetter> optionsGetters = new List<IOptionsGetter>();
        public CoverageSettingsOptionsProvider(
            //IOptionsProvider<AppOptions> appOptionsProvider,
            //IOptionsProvider<RunOptions> runOptionsProvider
        )
        {
            //optionsGetters.Add(appOptionsProvider);
            //optionsGetters.Add(runOptionsProvider);
        }
        public IEnumerable<object> Get() => this.optionsGetters.Select(g => g.GetOptionsAsObject());
    }

    internal interface IOptionsGetter
    {
        object GetOptionsAsObject();
    }
    internal interface IOptionsProvider<TOptions> : IOptionsGetter
    {
        event Action<TOptions> OptionsChanged;
        TOptions Get();
    }
}
