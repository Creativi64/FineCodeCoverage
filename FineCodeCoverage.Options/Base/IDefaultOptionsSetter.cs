namespace FineCodeCoverage.Options.Base
{
    internal interface IDefaultOptionsSetter<TOptions>
    {
        void Set(TOptions options);
    }
}
