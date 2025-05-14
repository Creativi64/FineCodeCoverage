namespace FineCodeCoverage.Options
{
    interface IDefaultOptionsSetter<TOption>
    {
        void Set(TOption option);
    }
}
