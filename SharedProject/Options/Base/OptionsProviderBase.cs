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
        private readonly ILogger logger;
        private readonly IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider;
        private readonly IJsonConvertService jsonConvertService;
        private readonly IDefaultOptionsSetter<TOptions> defaultOptionsSetter;
        private readonly TOptions options;
        public Lazy<PropertyDescriptorCollection> LazyOptionsPropertyDescriptorCollection { get; }

        public event Action<TOptions> OptionsChanged;

        public bool Initializing { get; set; }

        protected OptionsProviderBase(
            ILogger logger,
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider,
            IJsonConvertService jsonConvertService,
            IDefaultOptionsSetter<TOptions> defaultOptionsSetter)
        {
            this.logger = logger;
            this.writableUserSettingsStoreProvider = writableUserSettingsStoreProvider;
            this.jsonConvertService = jsonConvertService;
            this.defaultOptionsSetter = defaultOptionsSetter;
            this.options = new TOptions();
            defaultOptionsSetter.Set(this.options);
            this.LazyOptionsPropertyDescriptorCollection = new Lazy<PropertyDescriptorCollection>(() => TypeDescriptor.GetProperties(typeof(TOptions)));
        }

        private void RaiseOptionsChanged(TOptions options) => OptionsChanged?.Invoke(options);

        public TOptions Get() => this.options;

        private WritableSettingsStore EnsureStore()
        {
            WritableSettingsStore settingsStore = this.writableUserSettingsStoreProvider.LazySettingsStore.GetValue();
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

                    object objValue = this.jsonConvertService.DeserializeObject(strValue, property.PropertyType);
                    property.SetValue(this.options, objValue);
                }
                catch (Exception exception)
                {
                    this.logger.LogFileAndForget($"Failed to load '{property.Name}' setting", exception.ToString());
                }
            }

            this.Initializing = false;
        }

        void IDialogPageOptionsProvider<TOptions>.SaveSettingsToStorage() => this.SaveSettingsToStorage();

        TOptions IDialogPageOptionsProvider<TOptions>.Options => this.options;

        void IProfileOptionsProvider.SaveSettingsToStorage() => this.SaveSettingsToStorage();

        public object Options => this.options;

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
                    object objValue = property.GetValue(this.options);
                    string strValue = this.jsonConvertService.SerializeObject(objValue);

                    settingsStore.SetString(Vsix.Code, property.Name, strValue);
                }
                catch (Exception exception)
                {
                    this.logger.LogFileAndForget($"Failed to save '{property.Name}' setting", exception.ToString());
                }
            }

            this.RaiseOptionsChanged(this.options);
        }

        public void Reset()
        {
            this.Initializing = true;
            foreach (PropertyDescriptor property in this.LazyOptionsPropertyDescriptorCollection.Value)
            {
                object defaultValue = property.PropertyType.IsValueType
                    ? Activator.CreateInstance(property.PropertyType)
                    : null;

                property.SetValue(this.options, defaultValue);
            }

            this.defaultOptionsSetter.Set(this.options);
            this.Initializing = false;
            this.SaveSettingsToStorage();
        }
    }
}
