using System.ComponentModel;

namespace FineCodeCoverage.Options
{
    /*
        Note that option properties must not be renamed
        Interface required for reflection - options => CoverageSettings
    */
    internal class RunOptions : IEnabledOption
    {
        private const string enablementCategory = "Enablement";
        private const string useMsCodeCoverageCategory = "Use Microsoft Code Coverage";
        private const string testsCategory = "Tests";
        #region enablement category
        [Category(enablementCategory)]
        [Description("Specifies whether or not coverage output is enabled")]
        public bool Enabled { get; set; }

        [Category(enablementCategory)]
        [Description("Set to false for VS Option Enabled=false to not disable coverage")]
        [DisplayName("Disabled No Coverage")]
        public bool DisabledNoCoverage { get; set; }
        #endregion

        #region use ms code coverage category
        [Category(useMsCodeCoverageCategory)]
        [Description("Specifies whether or not the ms code coverage is used.  No, IfInRunSettings, Yes ( default )")]
        [DisplayName("Run Ms Code Coverage)")]
        public RunMsCodeCoverage RunMsCodeCoverage { get; set; }
        #endregion

        #region tests category
        [Description("Specify false to prevent coverage when tests fail.  Cannot be used in conjunction with RunInParallel")]
        [Category(testsCategory)]
        [DisplayName("Run When Tests Fail")]
        public bool RunWhenTestsFail { get; set; }

        [Description("Specify a value to only run coverage based upon the number of executing tests.  Cannot be used in conjunction with RunInParallel")]
        [Category(testsCategory)]
        [DisplayName("Run When Tests Exceed")]
        public int RunWhenTestsExceed { get; set; }
        #endregion

        #region coverlet / opencover category
        [Description("Specify true to not wait for tests to finish before running OpenCover / Coverlet coverage")]
        [Category(CoverletOpenCoverCategory.Name)]
        [DisplayName("Run In Parallel")]
        public bool RunInParallel { get; set; }
        #endregion
    }
}