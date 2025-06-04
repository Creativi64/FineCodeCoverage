using System;
using System.ComponentModel;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Settings;

namespace FineCodeCoverage.Options
{
    internal abstract class OptionsProviderBase<TOptions> :
        IResetOptions,
        IProfileOptionsProvider,
        IOptionsProvider<TOptions>,
        IDialogPageOptionsProvider<TOptions> where TOptions : class, new()
    {
        private readonly ILogger _logger;
        private readonly IWritableUserSettingsStoreProvider _writableUserSettingsStoreProvider;
        private readonly IJsonConvertService _jsonConvertService;
        private readonly IDefaultOptionsSetter<TOptions> _defaultOptionsSetter;
        private readonly TOptions _options;
        public Lazy<PropertyDescriptorCollection> LazyOptionsPropertyDescriptorCollection { get; }

        public event Action<TOptions> OptionsChanged;

        public bool Initializing { get; set; }

        protected OptionsProviderBase(
            ILogger logger,
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
            IJsonConvertService jsonConvertService,
            IDefaultOptionsSetter<TOptions> defaultOptionsSetter)
        {
            this._logger = logger;
            this._writableUserSettingsStoreProvider = writableUserSettingsStoreProvider;
            this._jsonConvertService = jsonConvertService;
            this._defaultOptionsSetter = defaultOptionsSetter;
            this._options = new TOptions();
            defaultOptionsSetter.Set(this._options);
            this.LazyOptionsPropertyDescriptorCollection = new Lazy<PropertyDescriptorCollection>(() => TypeDescriptor.GetProperties(typeof(TOptions)));
        }

        private void RaiseOptionsChanged(TOptions options) => OptionsChanged?.Invoke(options);

        public TOptions Get() => this._options;

        private WritableSettingsStore EnsureStore()
        {
            WritableSettingsStore settingsStore = this._writableUserSettingsStoreProvider.LazySettingsStore.GetValue();
            if (!settingsStore.CollectionExists(Vsix.Code))
            {
                settingsStore.CreateCollection(Vsix.Code);
            }

            return settingsStore;
        }

        void IDialogPageOptionsProvider<TOptions>.LoadSettingsFromStorage()
        {
            this.Initializing = true;
            WritableSettingsStore settingsStore = this.EnsureStore();

            foreach (PropertyDescriptor property in this.LazyOptionsPropertyDescriptorCollection.Value)
            {
                try
                {
                    if (!settingsStore.PropertyExists(Vsix.Code, property.Name))
                    {
                        continue;
                    }

                    string strValue = settingsStore.GetString(Vsix.Code, property.Name);

                    if (string.IsNullOrWhiteSpace(strValue))
                    {
                        continue;
                    }

                    object objValue = this._jsonConvertService.DeserializeObject(strValue, property.PropertyType);
                    property.SetValue(this._options, objValue);
                }
                catch (Exception exception)
                {
                    this._logger.LogFileAndForget($"Failed to load '{property.Name}' setting", exception.ToString());
                }
            }

            this.Initializing = false;
        }

        void IDialogPageOptionsProvider<TOptions>.SaveSettingsToStorage() => this.SaveSettingsToStorage();

        TOptions IDialogPageOptionsProvider<TOptions>.Options => this._options;

        void IProfileOptionsProvider.SaveSettingsToStorage() => this.SaveSettingsToStorage();

        public object Options => this._options;

        private void SaveSettingsToStorage()
        {
            if (!this.Initializing)
            {
                this.SaveSettingsToStorageActual();
            }
        }

        private void SaveSettingsToStorageActual()
        {
            WritableSettingsStore settingsStore = this.EnsureStore();

            foreach (PropertyDescriptor property in this.LazyOptionsPropertyDescriptorCollection.Value)
            {
                try
                {
                    object objValue = property.GetValue(this._options);
                    string strValue = this._jsonConvertService.SerializeObject(objValue);

                    settingsStore.SetString(Vsix.Code, property.Name, strValue);
                }
                catch (Exception exception)
                {
                    this._logger.LogFileAndForget($"Failed to save '{property.Name}' setting", exception.ToString());
                }
            }

            this.RaiseOptionsChanged(this._options);
        }

        public void Reset()
        {
            this.Initializing = true;
            foreach (PropertyDescriptor property in this.LazyOptionsPropertyDescriptorCollection.Value)
            {
                object defaultValue = property.PropertyType.IsValueType
                    ? Activator.CreateInstance(property.PropertyType)
                    : null;

                property.SetValue(this._options, defaultValue);
            }

            this._defaultOptionsSetter.Set(this._options);
            this.Initializing = false;
            this.SaveSettingsToStorage();
        }
    }
}