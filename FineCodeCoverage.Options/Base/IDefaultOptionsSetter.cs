namespace FineCodeCoverage.Options
{
    internal interface IDefaultOptionsSetter<TOptions>
    {
        void Set(TOptions options);
    }
}
