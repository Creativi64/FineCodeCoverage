using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using FineCodeCoverage.Utilities.Extensions;

namespace FineCodeCoverage.Wpf
{
    public abstract class DependentPropertiesChangedUserControl<T> : UserControl, INotifyPropertyChanged, IPropertyDependencyChanged
       where T : DependentPropertiesChangedUserControl<T>
    {
        private static readonly Dictionary<Type, DependentPropertiesChangedNotifier<T>> s_notifierCache = new Dictionary<Type, DependentPropertiesChangedNotifier<T>>();
        private static int s_numInstances;

        private readonly DependentPropertiesChangedNotifier<T> _notifier;
        private readonly int _id;
        private bool _listening = true;

        public event PropertyChangedEventHandler PropertyChanged;

        protected DependentPropertiesChangedUserControl()
        {
            _id = s_numInstances++;
            _notifier = s_notifierCache.GetOrAdd(typeof(T), () => DependentPropertiesChangedNotifierBuilder.Build<T>());

            _notifier.NotifyOfChanges((T)this);
            Loaded += DependentPropertiesChangedUserControl_Loaded;
            Unloaded += OnUnloaded;
        }

        private void DependentPropertiesChangedUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_listening)
            {
                return;
            }

            _notifier.NotifyOfChanges((T)this);
            _listening = true;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _listening = false;
            _notifier.RemoveNotificationOfChanges((T)this);
        }

        public void NotifyDependentPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
