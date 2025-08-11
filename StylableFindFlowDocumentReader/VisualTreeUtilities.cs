using System.Windows;
using System.Windows.Media;

namespace StylableFindFlowDocumentReader
{
    internal static class VisualTreeUtilities
    {
        public static T FindByName<T>(DependencyObject parent, string name)
            where T : FrameworkElement
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typed && typed.Name == name)
                {
                    return typed;
                }

                T result = FindByName<T>(child, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
