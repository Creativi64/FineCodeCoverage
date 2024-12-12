using System.Windows;
using System.Windows.Media;


namespace TreeGrid
{
    public static class VisualTreeHelperExtensions
    {
        public static T GetAscendant<T>(this DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null)
            {
                return null;
            }

            if (parent is T t)
            {
                return t;
            }

            return GetAscendant<T>(parent);
        }
        public static T FindVisualChild<T>(this DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                {
                    return t;
                }

                var result = FindVisualChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

    }
}
