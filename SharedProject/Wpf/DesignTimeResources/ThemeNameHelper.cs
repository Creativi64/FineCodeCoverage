using System.Windows;

namespace FineCodeCoverage.Wpf
{
    public static class ThemeNameHelper
    {
        public static string FindRootThemeName(DependencyObject targetObject)
        {
            DependencyObject current = targetObject;
            while (current != null)
            {
                var themeName = ThemeNameManager.GetThemeName(current);
                if (themeName != null)
                {
                    return themeName;
                }

                current = LogicalTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
