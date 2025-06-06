using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Settings;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IShowReadMeService))]
    internal sealed class ShowReadMeService : IShowReadMeService
    {
        private const string ReadMeShowCollection = "FCCReadmeShowCollection";
        private const string ReadMeShownProperty = "FCCReadmeShown";
        private readonly WritableSettingsStore _writableUserSettingsStore;
        private readonly IToolWindowService _toolWindowService;

        public event EventHandler Shown;

        [ImportingConstructor]
        public ShowReadMeService(
            IToolWindowService toolWindowService,
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider)
        {
            _writableUserSettingsStore = writableUserSettingsStoreProvider.LazySettingsStore.GetValue();
            HasShown = _writableUserSettingsStore.GetBoolean(ReadMeShowCollection, ReadMeShownProperty, false);
            _toolWindowService = toolWindowService;
        }

        public bool HasShown { get; private set; }

        public void Show()
        {
            if (!HasShown)
            {
                if (!_writableUserSettingsStore.CollectionExists(ReadMeShowCollection))
                {
                    _writableUserSettingsStore.CreateCollection(ReadMeShowCollection);
                }

                _writableUserSettingsStore.SetBoolean(ReadMeShowCollection, ReadMeShownProperty, true);
            }

            HasShown = true;
            Shown?.Invoke(this, EventArgs.Empty);

            _ = _toolWindowService.ShowToolWindowAsync(typeof(ReadmeToolWindow), 0, true);
        }
    }
}
