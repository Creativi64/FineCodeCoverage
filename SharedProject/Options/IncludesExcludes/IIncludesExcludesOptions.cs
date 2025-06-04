namespace FineCodeCoverage.Options
{
    /*
        Note that option properties must not be renamed
    */
    internal interface IIncludesExcludesOptions :
        IMsCodeCoverageIncludesExcludesOptions,
        IOpenCoverCoverletExcludeIncludeOptions,
        IFCCCommonIncludesExcludes
    { }
}
