using System;

namespace FineCodeCoverage.Options.Base
{
    public interface IOptionsProvider<TOptions> : IProvideOptions
    {
        event Action<TOptions> OptionsChanged;

        TOptions Get();
    }
}
