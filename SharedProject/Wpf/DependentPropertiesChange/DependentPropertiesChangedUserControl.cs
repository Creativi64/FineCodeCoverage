using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using FineCodeCoverage.Core.Utilities;

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
        private readonly int id;

        protected DependentPropertiesChangedUserControl()
        {
            this.id = numInstances++;
            this.notifier = NotifierCache.GetOrAdd(typeof(T), () => DependentPropertiesChangedNotifierBuilder.Build<T>());

            this.notifier.NotifyOfChanges((T)this);
            this.Loaded += this.DependentPropertiesChangedUserControl_Loaded;
            this.Unloaded += this.OnUnloaded;
        }

        private void DependentPropertiesChangedUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.listening)
            {
                this.notifier.NotifyOfChanges((T)this);
                this.listening = true;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.listening = false;
            this.notifier.RemoveNotificationOfChanges((T)this);
        }

        public void NotifyDependentPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}