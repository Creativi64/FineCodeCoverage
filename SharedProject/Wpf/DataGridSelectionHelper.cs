using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FineCodeCoverage.Wpf
{
    public static class DataGridSelectionHelper
    {
        public static readonly DependencyProperty HandleSelectionProperty =
            DependencyProperty.RegisterAttached(
                "HandleSelection",
                typeof(bool),
                typeof(DataGridSelectionHelper),
                new PropertyMetadata(false, OnHandleSelectionChanged));

        public static void SetHandleSelection(DependencyObject element, bool value)
            => element.SetValue(HandleSelectionProperty, value);

        public static bool GetHandleSelection(DependencyObject element)
            => (bool)element.GetValue(HandleSelectionProperty);

        private static void OnHandleSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if ((bool)e.NewValue)
                {
                    dataGrid.SelectionChanged += DataGrid_SelectionChanged;
                }
                else
                {
                    dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
                }
            }
        }

        private static void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is DataGrid dataGrid)) return;

            object dataContext = dataGrid.DataContext;
            if (dataContext == null) return;

            Type itemsSourceType = dataGrid.ItemsSource?.GetType();
            if (itemsSourceType == null) return;

            // Find generic argument from ICollection<T>
            Type itemType = GetGenericArgumentFromICollection(itemsSourceType);
            if (itemType == null) return;

            Type handlerInterface = typeof(ISelectionHandler<>).MakeGenericType(itemType);
            if (!handlerInterface.IsAssignableFrom(dataContext.GetType())) return;

            System.Reflection.MethodInfo method = handlerInterface.GetMethod(nameof(ISelectionHandler<object>.SelectionChanged));

            object selectedItems = ExtractTypedSelectedItems(dataGrid.SelectedItems, itemType);
            _ = method.Invoke(dataContext, new[] { selectedItems });
        }

        private static Type GetGenericArgumentFromICollection(Type type)
        {
            Type interfaceType = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));
            return interfaceType?.GetGenericArguments()[0];
        }

        private static object ExtractTypedSelectedItems(IList selectedItems, Type targetType)
        {
            Type listType = typeof(List<>).MakeGenericType(targetType);
            var result = (IList)Activator.CreateInstance(listType);

            foreach (object item in selectedItems)
            {
                _ = result.Add(item);
            }

            return result;
        }
    }
}
