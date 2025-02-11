using System.Diagnostics;

namespace FineCodeCoverage.Wpf
{
    public static class DesignModeHelper
    {
        private static readonly bool isInDesignMode;

        static DesignModeHelper()
        {
            var currentProcessName = Process.GetCurrentProcess().ProcessName;
            isInDesignMode = currentProcessName == "WpfSurface";
        }

        public static bool IsInDesignMode => isInDesignMode;
    }
}
