using System.ComponentModel;

namespace FineCodeCoverage.Options
{
    /*
        Note that option properties must not be renamed
    */
    internal class OutputOptions
    {
        private const string coverletOpenCoverCategory = "Coverlet / OpenCover";
        private const string commonCategory = "Common";
        [Description("To have fcc output visible in a sub folder of your solution provide this name")]
        [Category(commonCategory)]
        [DisplayName("Solution Output Directory Name")]
        public string FCCSolutionOutputDirectoryName { get; set; }

        [Description("If your tests are dependent upon their path set this to true. OpenCover / Coverlet")]
        [Category(coverletOpenCoverCategory)]
        [DisplayName("Adjacent Build Output")]
        public bool AdjacentBuildOutput { get; set; }
    }
}
