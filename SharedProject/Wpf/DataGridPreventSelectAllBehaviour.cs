using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FineCodeCoverage.Wpf
{
    public class DataGridPreventSelectAllBehaviour
    {
        public static bool GetValue(DependencyObject obj)
        {
            return (bool)obj.GetValue(ValueProperty);
        }

        public static void SetValue(DependencyObject obj, bool value)
        {
            obj.SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.RegisterAttached("Value", typeof(bool), typeof(DataGridPreventSelectAllBehaviour), new PropertyMetadata(false,
                (o, _) =>
                {
                    if (o is DataGrid dg)
                    {
                        var commandBinding = new CommandBinding(
                            ApplicationCommands.SelectAll,
                            null,
                            (object sender, CanExecuteRoutedEventArgs ce) =>
                            {
                                ce.CanExecute = false;
                                ce.Handled = true; // Prevent the default behavior
                            });

                        // Register the command binding for this specific DataGrid instance
                        dg.CommandBindings.Add(commandBinding);
                    }
                }));
    }
}
