using System.Windows.Media;

namespace FineCodeCoverage.Output
{
    // https://learn.microsoft.com/en-us/visualstudio/extensibility/ux-guidelines/images-and-icons-for-visual-studio?view=vs-2022#notifications
    public static class VisualStudioNotificationColors
    {
        public static Color Neutral = (Color)ColorConverter.ConvertFromString("#1BA1E2");
        public static Color Positive = (Color)ColorConverter.ConvertFromString("#339933");
        public static Color Negative = (Color)ColorConverter.ConvertFromString("#E51400");
        public static Color Warning = (Color)ColorConverter.ConvertFromString("#FFCC00");
    }
}
