namespace FineCodeCoverage.Options
{
    interface IDefaultOptionsSetter<TOptions>
    {
        void Set(TOptions options);
    }
}