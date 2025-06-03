namespace FineCodeCoverage.Core.Utilities
{
    internal static class IsVs2022
    {
#if VS2022
        public static bool Value => true;
#else
        public static bool Value => false;
#endif
    }
}