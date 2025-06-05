using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Core.Utilities
{
    /*
        If there are issues with .imagemanifest files - for use with CrispImage and when referenced in vsct
        This attribute can be used to register the trace level and trace file name for the ImageLibrary.
        If not absolute the trace file name will be relative to the %UserProfile%

        When changing .imagemanifest and vsct
        devenv.exe /rootSuffix Exp /updateconfiguration
    */

    internal class TraceImageLibraryRegistrationAttribute : RegistrationAttribute
    {
        private readonly string _traceFileName;
        private readonly TraceLevel _traceLevel;

        public TraceImageLibraryRegistrationAttribute(TraceLevel traceLevel = TraceLevel.Off, string traceFileName = "ImageLibrary.log")
        {
            _traceFileName = traceFileName;
            _traceLevel = traceLevel;
        }

        public override void Register(RegistrationContext context)
        {
            Key key = context.CreateKey("ImageLibrary");
            key.SetValue("TraceLevel", _traceLevel.ToString());
            key.SetValue("TraceFilename", _traceFileName);
        }

        public override void Unregister(RegistrationContext context)
        {
        }
    }
}
