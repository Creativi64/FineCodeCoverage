using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Settings;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IShowReadMeService))]
    internal class ShowReadMeService : IShowReadMeService
    {
        private readonly WritableSettingsStore _writableUserSettingsStore;
        private readonly IToolWindowService _toolWindowService;
        private const string readMeShowCollection = "FCCReadmeShowCollection";
        private const string readMeShownProperty = "FCCReadmeShown";

        public event EventHandler Shown;

        [ImportingConstructor]
        public ShowReadMeService(
            IToolWindowService toolWindowService,
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider
        )
        {
            this._writableUserSettingsStore = writableUserSettingsStoreProvider.LazySettingsStore.GetValue();
            this.HasShown = this._writableUserSettingsStore.GetBoolean(readMeShowCollection, readMeShownProperty, false);
            this._toolWindowService = toolWindowService;
        }

        public bool HasShown { get; private set; }

        public void Show()
        {
            if (!this.HasShown)
            {
                if (!this._writableUserSettingsStore.CollectionExists(readMeShowCollection))
                {
                    this._writableUserSettingsStore.CreateCollection(readMeShowCollection);
                }

                this._writableUserSettingsStore.SetBoolean(readMeShowCollection, readMeShownProperty, true);
            }

            this.HasShown = true;
            Shown?.Invoke(this, EventArgs.Empty);

            _ = this._toolWindowService.ShowToolWindowAsync(typeof(ReadmeToolWindow), 0, true);
        }
    }
}