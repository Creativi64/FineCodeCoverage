using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Output
{
    internal enum TraceLevel
    {
        Off,
        Error,
        Warning,
        Info,
        Verbose
    }

    /*
        If there are issues with .imagemanifest files - for use with CrispImage and when referenced in vsct
        This attribute can be used to register the trace level and trace file name for the ImageLibrary.
        If not absolute the trace file name will be relative to the %UserProfile%

        When changing .imagemanifest and vsct 
        devenv.exe /rootSuffix Exp /updateconfiguration
    */

    internal class TraceImageLibraryRegistrationAttribute : RegistrationAttribute
    {
        private readonly string traceFileName;
        private readonly TraceLevel traceLevel;

        public TraceImageLibraryRegistrationAttribute(TraceLevel traceLevel = TraceLevel.Off, string traceFileName = "ImageLibrary.log")
        {
            this.traceFileName = traceFileName;
            this.traceLevel = traceLevel;
        }
        public override void Register(RegistrationContext context)
        {
            var key = context.CreateKey("ImageLibrary");
            key.SetValue("TraceLevel", traceLevel.ToString());
            key.SetValue("TraceFilename", traceFileName);
        }

        public override void Unregister(RegistrationContext context)
        {
            
        }
    }
}
