using System.ComponentModel;
using FineCodeCoverage.Options.Base;

namespace FineCodeCoverage.Options.Run
{
    /*
        Note that option properties must not be renamed
        Interface required for reflection - options => CoverageSettings
    */
    public sealed class RunOptions : IEnabledOption
    {
        private const string EnablementCategory = "Enablement";
        private const string TestsCategory = "Tests";
        private const string TestingPlatformCategory = "Microsoft Testing Platform";

        #region enablement category
        [Category(EnablementCategory)]
        [Description("Specifies whether or not coverage output is enabled")]
        public bool Enabled { get; set; }

        [Category(EnablementCategory)]
        [Description("Set to false for VS Option Enabled=false to not disable coverage")]
        [DisplayName("Disabled No Coverage")]
        public bool DisabledNoCoverage { get; set; }
        #endregion

        #region testing platform category
        [Category(TestingPlatformCategory)]
        [Description("Specifies whether or not FCC collects coverage during Microsoft.Testing.Platform Test Explorer runs. Requires Microsoft.Testing.Platform 2.3.0 or later and Microsoft.Testing.Extensions.CodeCoverage. If options cannot be injected, automatic coverage is skipped; tests are never re-run automatically. Use the manual MTP coverage command to run all enabled MTP projects. Default true.")]
        [DisplayName("Collect Testing Platform Coverage After Test Run")]
        public bool CollectTestingPlatformCoverageAfterTestRun { get; set; }

        [Category(TestingPlatformCategory)]
        [Description("Specify true to stop Microsoft.Testing.Platform test projects running natively in the Test Explorer, forcing them back to VSTest.  This is the escape hatch for when coverage cannot be injected into the native run.  Default false.")]
        [DisplayName("Disable Testing Platform Server")]
        public bool DisableTestingPlatformServer { get; set; }
        #endregion

        #region tests category
        [Description("Specify false to prevent coverage when tests fail.")]
        [Category(TestsCategory)]
        [DisplayName("Run When Tests Fail")]
        public bool RunWhenTestsFail { get; set; }

        [Description("Specify a value to only run coverage based upon the number of executing tests.")]
        [Category(TestsCategory)]
        [DisplayName("Run When Tests Exceed")]
        public int RunWhenTestsExceed { get; set; }
        #endregion
    }
}
