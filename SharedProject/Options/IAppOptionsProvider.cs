using System;

namespace FineCodeCoverage.Options
{
    internal interface IAppOptionsProvider
    {
        // The argument is the same AppOptions from Get
        event Action<AppOptions> OptionsChanged;
        // returns the same instance each time
        AppOptions Get();
    }
}
