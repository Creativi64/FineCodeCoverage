using System;
using System.ComponentModel;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Settings;

namespace FineCodeCoverage.Options
{
    internal abstract class OptionsProviderBase<TOptions> :
        IProfileOptionsProvider,
        IOptionsProvider<TOptions>,
        IDialogPageOptionsProvider<TOptions> where TOptions: class, new()
    {
        private readonly ILogger logger;
        private readonly IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider;
        private readonly IJsonConvertService jsonConvertService;
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
            options = new TOptions();
            defaultOptionsSetter.Set(options);
            LazyOptionsPropertyDescriptorCollection = new Lazy<PropertyDescriptorCollection>(() => TypeDescriptor.GetProperties(typeof(TOptions)));
        }

        private void RaiseOptionsChanged(TOptions options)
        {
            OptionsChanged?.Invoke(options);
        }

        public TOptions Get() => options;

        private WritableSettingsStore EnsureStore()
        {
            var settingsStore = writableUserSettingsStoreProvider.LazySettingsStore.GetValue();
            if (!settingsStore.CollectionExists(Vsix.Code))
            {
                settingsStore.CreateCollection(Vsix.Code);
            }
            return settingsStore;
        }

        void IDialogPageOptionsProvider<TOptions>.LoadSettingsFromStorage()
        {
            this.Initializing = true;
            var settingsStore = EnsureStore();

            foreach (PropertyDescriptor property in LazyOptionsPropertyDescriptorCollection.Value)
            {
                try
                {
                    if (!settingsStore.PropertyExists(Vsix.Code, property.Name))
                    {
                        continue;
                    }

                    var strValue = settingsStore.GetString(Vsix.Code, property.Name);

                    if (string.IsNullOrWhiteSpace(strValue))
                    {
                        continue;
                    }

                    var objValue = jsonConvertService.DeserializeObject(strValue, property.PropertyType);
                    property.SetValue(options, objValue);
                }
                catch (Exception exception)
                {
                    logger.LogFileAndForget($"Failed to load '{property.Name}' setting", exception.ToString());
                }
            }
            this.Initializing = false;
        }

        void IDialogPageOptionsProvider<TOptions>.SaveSettingsToStorage() => SaveSettingsToStorage();

        TOptions IDialogPageOptionsProvider<TOptions>.Options => options;

        void IProfileOptionsProvider.SaveSettingsToStorage() => SaveSettingsToStorage();

        object IProfileOptionsProvider.LoadSettingsFromStorage() => Get();

        private void SaveSettingsToStorage()
        {
            if (!this.Initializing)
            {
                SaveSettingsToStorageActual();
            }
        }

        private void SaveSettingsToStorageActual()
        {
            var settingsStore = EnsureStore();

            foreach (PropertyDescriptor property in LazyOptionsPropertyDescriptorCollection.Value)
            {
                try
                {
                    var objValue = property.GetValue(options);
                    var strValue = jsonConvertService.SerializeObject(objValue);

                    settingsStore.SetString(Vsix.Code, property.Name, strValue);
                }
                catch (Exception exception)
                {
                    logger.LogFileAndForget($"Failed to save '{property.Name}' setting", exception.ToString());
                }
            }
            RaiseOptionsChanged(options);
        }

        public object GetOptionsAsObject() => Get();
    }
}
