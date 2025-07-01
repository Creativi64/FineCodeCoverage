namespace FineCodeCoverage.Options
{
    public interface IOpenCoverCoverletExcludeIncludeOptions
    {
        string[] Exclude { get; set; }

        string[] ExcludeByAttribute { get; set; }

        string[] ExcludeByFile { get; set; }

        string[] Include { get; set; }
    }
}
