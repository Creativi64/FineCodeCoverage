using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StylableFindFlowDocumentReader
{
    public static class ReplaceElementExtension
    {
        public static void Replace(this FrameworkElement original, FrameworkElement replacement)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(original);
            if (parent == null)
            {
                return;
            }

            if (parent is Decorator parentDecorator)
            {
                parentDecorator.Child = null; // ❗️disconnect from tree
                parentDecorator.Child = replacement; // ✅ safe to reparent now
            }

            if (parent is ContentControl contentControl)
            {
                contentControl.Content = replacement;
            }

            if (!(parent is Panel panel))
            {
                return;
            }

            int index = panel.Children.IndexOf(original);
            if (index < 0)
            {
                return;
            }

            panel.Children.RemoveAt(index); // ❗️disconnect from tree
            panel.Children.Insert(index, replacement); // ✅ safe to reparent now

        }
    }
}
