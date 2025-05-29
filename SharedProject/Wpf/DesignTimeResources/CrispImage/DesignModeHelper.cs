using System.Diagnostics;

namespace FineCodeCoverage.Wpf
{
    public static class DesignModeHelper
    {
        static DesignModeHelper()
        {
            string currentProcessName = Process.GetCurrentProcess().ProcessName;
            IsInDesignMode = currentProcessName == "WpfSurface";
        }

        public static bool IsInDesignMode { get; }
    }
}
