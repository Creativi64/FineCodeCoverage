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
        {
            element.SetValue(HandleSelectionProperty, value);
        }

        public static bool GetHandleSelection(DependencyObject element)
        {
            return (bool)element.GetValue(HandleSelectionProperty);
        }

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

            var dataContext = dataGrid.DataContext;
            if (dataContext == null) return;

            var itemsSourceType = dataGrid.ItemsSource?.GetType();
            if (itemsSourceType == null) return;

            // Find generic argument from ICollection<T>
            var itemType = GetGenericArgumentFromICollection(itemsSourceType);
            if (itemType == null) return;

            var handlerInterface = typeof(ISelectionHandler<>).MakeGenericType(itemType);
            if (!handlerInterface.IsAssignableFrom(dataContext.GetType())) return;

            var method = handlerInterface.GetMethod(nameof(ISelectionHandler<object>.SelectionChanged));

            var selectedItems = ExtractTypedSelectedItems(dataGrid.SelectedItems, itemType);
            method.Invoke(dataContext, new[] { selectedItems });
        }

        private static Type GetGenericArgumentFromICollection(Type type)
        {
            var interfaceType = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));
            return interfaceType?.GetGenericArguments()[0];
        }

        private static object ExtractTypedSelectedItems(IList selectedItems, Type targetType)
        {
            var listType = typeof(List<>).MakeGenericType(targetType);
            var result = (IList)Activator.CreateInstance(listType);

            foreach (var item in selectedItems)
            {
                result.Add(item);
            }

            return result;
        }
    }

}
