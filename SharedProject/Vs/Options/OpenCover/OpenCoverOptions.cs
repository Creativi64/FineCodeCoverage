using System.ComponentModel;

namespace FineCodeCoverage.Options
{
    /*
        Note that option properties must not be renamed
        Interfaces to be retained for reflection - AppOptions => CoverageSettings
    */
    internal sealed class OpenCoverOptions : IOpenCoverOptions
    {
        private const string CustomTargetCategory = "Custom target";

        [Description("Specify path to open cover exe if you need functionality that the FCC version does not provide.")]
        [DisplayName("Custom Path")]
        public string OpenCoverCustomPath { get; set; }

        [Description("Change from Default if FCC determination of path32 or path64 is incorrect.")]
        [DisplayName("Register")]
        public OpenCoverRegister OpenCoverRegister { get; set; }

        [Category(CustomTargetCategory)]
        [Description("Supply your own target if required.")]
        [DisplayName("Target")]
        public string OpenCoverTarget { get; set; }

        [Category(CustomTargetCategory)]
        [Description("If supplying your own target you can also supply additional arguments.  FCC supplies the test dll path.")]
        [DisplayName("Target Args")]
        public string OpenCoverTargetArgs { get; set; }
    }
}
