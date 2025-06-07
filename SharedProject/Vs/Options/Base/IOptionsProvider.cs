using System;

namespace FineCodeCoverage.Options
{
    internal interface IOptionsProvider<TOptions> : IProvideOptions
    {
        event Action<TOptions> OptionsChanged;

        TOptions Get();
    }
}
