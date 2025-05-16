using System;

namespace FineCodeCoverage.Options
{
    internal interface IOptionsProvider<TOptions> : IOptionsGetter
    {
        event Action<TOptions> OptionsChanged;
        TOptions Get();
    }
}
