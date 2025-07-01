using System;

namespace FineCodeCoverage.Options
{
    public interface IOptionsProvider<TOptions> : IProvideOptions
    {
        event Action<TOptions> OptionsChanged;

        TOptions Get();
    }
}
