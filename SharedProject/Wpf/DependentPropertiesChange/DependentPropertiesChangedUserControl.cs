using FineCodeCoverage.Core.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace FineCodeCoverage.Wpf
{
    public abstract class DependentPropertiesChangedUserControl<T> : UserControl, INotifyPropertyChanged, IPropertyDependencyChanged
       where T : DependentPropertiesChangedUserControl<T>
    {
        private static readonly Dictionary<Type, DependentPropertiesChangedNotifier<T>> NotifierCache = new Dictionary<Type, DependentPropertiesChangedNotifier<T>>();

        private readonly DependentPropertiesChangedNotifier<T> notifier;

        public event PropertyChangedEventHandler PropertyChanged;
        private bool listening = true;

        private static int numInstances;
        private int id;

        protected DependentPropertiesChangedUserControl()
        {
            id = numInstances++;
            this.notifier = NotifierCache.GetOrAdd(typeof(T), () => DependentPropertiesChangedNotifierBuilder.Build<T>());

            notifier.NotifyOfChanges((T)this);
            this.Loaded += DependentPropertiesChangedUserControl_Loaded;
            this.Unloaded += OnUnloaded;
        }

        private void DependentPropertiesChangedUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if ((!listening))
            {
                notifier.NotifyOfChanges((T)this);
                listening = true;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            listening = false;
            notifier.RemoveNotificationOfChanges((T)this);
        }

        public void NotifyDependentPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
