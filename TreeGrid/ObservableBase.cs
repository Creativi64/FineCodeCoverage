using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TreeGrid
{
    public class ObservableBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly ConcurrentDictionary<string, IEnumerable<string>> _errors = new ConcurrentDictionary<string, IEnumerable<string>>();

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool HasErrors => this._errors.Count > 0;

        public IEnumerable GetErrors(string propertyName)
        {
            IEnumerable<string> errors;
            this._errors.TryGetValue(propertyName, out errors);
            return (IEnumerable)errors;
        }

        public void AddErrors(string propertyName, IEnumerable<string> errors)
        {
            this._errors.TryRemove(propertyName, out IEnumerable<string> _);
            this._errors.TryAdd(propertyName, errors);
            EventHandler<DataErrorsChangedEventArgs> errorsChanged = this.ErrorsChanged;
            if (errorsChanged != null)
                errorsChanged((object)this, new DataErrorsChangedEventArgs(propertyName));
            this.OnPropertyChanged("HasErrors");
        }

        protected void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals((object)storage, (object)value))
                return;
            this._errors.TryRemove(propertyName, out IEnumerable<string> _);
            storage = value;
            this.OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged == null)
                return;
            propertyChanged((object)this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
