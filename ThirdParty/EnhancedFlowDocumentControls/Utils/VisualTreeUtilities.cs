using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace EnhancedFlowDocumentControls.Utils
{
    internal static class VisualTreeUtilities
    {
        public static T FindByName<T>(DependencyObject parent, string name)
            where T : FrameworkElement
        {
            // Check if parent itself matches
            if (parent is T parentAsT && parentAsT.Name == name)
            {
                return parentAsT;
            }

            if (parent is Visual || parent is Visual3D)
            {
                // Search visual children
                int visualChildrenCount = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < visualChildrenCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                    T result = FindByName<T>(child, name);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            // If not found in visual tree, search logical children
            foreach (object logicalChild in LogicalTreeHelper.GetChildren(parent))
            {
                if (logicalChild is DependencyObject depObj)
                {
                    T result = FindByName<T>(depObj, name);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}
