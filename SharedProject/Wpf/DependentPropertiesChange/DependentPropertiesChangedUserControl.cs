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
        private static readonly Dictionary<Type, DependentPropertiesChangedNotifier<T>> s_notifierCache = new Dictionary<Type, DependentPropertiesChangedNotifier<T>>();
        private static int s_numInstances;

        private readonly DependentPropertiesChangedNotifier<T> _notifier;
        private readonly int _id;
        private bool _listening = true;

        public event PropertyChangedEventHandler PropertyChanged;

        protected DependentPropertiesChangedUserControl()
        {
            this._id = s_numInstances++;
            this._notifier = s_notifierCache.GetOrAdd(typeof(T), () => DependentPropertiesChangedNotifierBuilder.Build<T>());

            this._notifier.NotifyOfChanges((T)this);
            this.Loaded += this.DependentPropertiesChangedUserControl_Loaded;
            this.Unloaded += this.OnUnloaded;
        }

        private void DependentPropertiesChangedUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this._listening)
            {
                return;
            }

            this._notifier.NotifyOfChanges((T)this);
            this._listening = true;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this._listening = false;
            this._notifier.RemoveNotificationOfChanges((T)this);
        }

        public void NotifyDependentPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
