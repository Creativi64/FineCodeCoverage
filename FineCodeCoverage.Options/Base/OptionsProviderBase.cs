using System;
using System.ComponentModel;
using FineCodeCoverage.Output;
using FineCodeCoverage.Utilities.Wrappers;

namespace FineCodeCoverage.Options.Base
{
    internal abstract class OptionsProviderBase<TOptions> :
        IResetOptions,
        IProfileOptionsProvider,
        IOptionsProvider<TOptions>,
        IDialogPageOptionsProvider<TOptions>
        where TOptions : class, new()
    {
        private const string CollectionName = "FineCodeCoverage";
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
            IDefaultOptionsSetter<TOptions> defaultOptionsSetter
        )
        {
            _logger = logger;
            _writableUserSettingsStoreProvider = writableUserSettingsStoreProvider;
            _jsonConvertService = jsonConvertService;
            _defaultOptionsSetter = defaultOptionsSetter;
            _options = new TOptions();
            defaultOptionsSetter.Set(_options);
            LazyOptionsPropertyDescriptorCollection = new Lazy<PropertyDescriptorCollection>(() => TypeDescriptor.GetProperties(typeof(TOptions)));
        }

        private void RaiseOptionsChanged(TOptions options) => OptionsChanged?.Invoke(options);

        public TOptions Get() => _options;

        private IWritableSettingsStore EnsureStore()
        {
            IWritableSettingsStore settingsStore = _writableUserSettingsStoreProvider.Provide();
            if (!settingsStore.CollectionExists(CollectionName))
            {
                settingsStore.CreateCollection(CollectionName);
            }

            return settingsStore;
        }

        void IDialogPageOptionsProvider<TOptions>.LoadSettingsFromStorage()
        {
            Initializing = true;
            IWritableSettingsStore settingsStore = EnsureStore();

            foreach (PropertyDescriptor property in LazyOptionsPropertyDescriptorCollection.Value)
            {
                try
                {
                    if (!settingsStore.PropertyExists(CollectionName, property.Name))
                    {
                        continue;
                    }

                    string strValue = settingsStore.GetString(CollectionName, property.Name);

                    if (string.IsNullOrWhiteSpace(strValue))
                    {
                        continue;
                    }

                    object objValue = _jsonConvertService.DeserializeObject(strValue, property.PropertyType);
                    property.SetValue(_options, objValue);
                }
                catch (Exception exception)
                {
                    _logger.LogFileAndForget($"Failed to load '{property.Name}' setting", exception.ToString());
                }
            }

            Initializing = false;
        }

        void IDialogPageOptionsProvider<TOptions>.SaveSettingsToStorage() => SaveSettingsToStorage();

        TOptions IDialogPageOptionsProvider<TOptions>.Options => _options;

        void IProfileOptionsProvider.SaveSettingsToStorage() => SaveSettingsToStorage();

        public object Options => _options;

        private void SaveSettingsToStorage()
        {
            if (Initializing)
            {
                return;
            }

            SaveSettingsToStorageActual();
        }

        private void SaveSettingsToStorageActual()
        {
            IWritableSettingsStore settingsStore = EnsureStore();

            foreach (PropertyDescriptor property in LazyOptionsPropertyDescriptorCollection.Value)
            {
                try
                {
                    object objValue = property.GetValue(_options);
                    string strValue = _jsonConvertService.SerializeObject(objValue);

                    settingsStore.SetString(CollectionName, property.Name, strValue);
                }
                catch (Exception exception)
                {
                    _logger.LogFileAndForget($"Failed to save '{property.Name}' setting", exception.ToString());
                }
            }

            RaiseOptionsChanged(_options);
        }

        public void Reset()
        {
            Initializing = true;
            foreach (PropertyDescriptor property in LazyOptionsPropertyDescriptorCollection.Value)
            {
                object defaultValue = property.PropertyType.IsValueType
                    ? Activator.CreateInstance(property.PropertyType)
                    : null;

                property.SetValue(_options, defaultValue);
            }

            _defaultOptionsSetter.Set(_options);
            Initializing = false;
            SaveSettingsToStorage();
        }
    }
}
