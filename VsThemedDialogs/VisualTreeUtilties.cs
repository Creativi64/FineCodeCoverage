using System.Windows;
using System.Windows.Media;

namespace VsThemedDialogs
{
    internal static class VisualTreeUtilties
    {
        public static T FindByName<T>(DependencyObject parent, string name)
            where T : FrameworkElement
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typed && typed.Name == name)
                {
                    return typed;
                }

                var result = FindByName<T>(child, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
