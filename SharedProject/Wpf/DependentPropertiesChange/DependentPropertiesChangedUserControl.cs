using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace FineCodeCoverage.Wpf
{
    public abstract class DependentPropertiesChangedUserControl<T> : UserControl, INotifyPropertyChanged, IPropertyDependencyChanged
       where T : DependentPropertiesChangedUserControl<T>
    {
        private static readonly ConcurrentDictionary<Type, DependentPropertiesChangedNotifier<T>> NotifierCache = new ConcurrentDictionary<Type, DependentPropertiesChangedNotifier<T>>();

        private readonly DependentPropertiesChangedNotifier<T> notifier;

        public event PropertyChangedEventHandler PropertyChanged;

        protected DependentPropertiesChangedUserControl()
        {
            notifier = NotifierCache.GetOrAdd(
                typeof(T),
                _ => DependentPropertiesChangedNotifierBuilder.Build<T>()
            );


            this.Loaded += DependentPropertiesChangedUserControl_Loaded;
            this.Unloaded += OnUnloaded;
        }

        private void DependentPropertiesChangedUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            notifier.NotifyOfChanges((T)this);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            notifier.RemoveNotificationOfChanges((T)this);
        }

        public void NotifyDependentPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
