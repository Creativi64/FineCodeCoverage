using System.ComponentModel;

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
        [Description("Specifies whether FCC collects coverage during Microsoft.Testing.Platform Test Explorer runs. Native injection requires MTP 2.3.0 or later and Microsoft.Testing.Extensions.CodeCoverage. Older projects with a supported VSTest adapter and Microsoft.NET.Test.Sdk automatically use VSTest compatibility, based on their restored packages. Tests are never re-run automatically. Restore and reload projects after changing test packages or this setting. Default true.")]
        [DisplayName("Collect Testing Platform Coverage After Test Run")]
        public bool CollectTestingPlatformCoverageAfterTestRun { get; set; }

        [Category(TestingPlatformCategory)]
        [Description("Specify true to force VSTest-compatible Microsoft.Testing.Platform projects through VSTest, including newer MTP versions. False allows per-project automatic compatibility for older MTP versions while keeping newer projects native. Requires the framework's VSTest adapter; not supported for TUnit. Reload projects after changing this setting. Default false.")]
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
