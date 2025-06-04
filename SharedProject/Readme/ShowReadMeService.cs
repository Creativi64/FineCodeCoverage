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
        private const string ReadMeShowCollection = "FCCReadmeShowCollection";
        private const string ReadMeShownProperty = "FCCReadmeShown";

        public event EventHandler Shown;

        [ImportingConstructor]
        public ShowReadMeService(
            IToolWindowService toolWindowService,
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider
        )
        {
            this._writableUserSettingsStore = writableUserSettingsStoreProvider.LazySettingsStore.GetValue();
            this.HasShown = this._writableUserSettingsStore.GetBoolean(ReadMeShowCollection, ReadMeShownProperty, false);
            this._toolWindowService = toolWindowService;
        }

        public bool HasShown { get; private set; }

        public void Show()
        {
            if (!this.HasShown)
            {
                if (!this._writableUserSettingsStore.CollectionExists(ReadMeShowCollection))
                {
                    this._writableUserSettingsStore.CreateCollection(ReadMeShowCollection);
                }

                this._writableUserSettingsStore.SetBoolean(ReadMeShowCollection, ReadMeShownProperty, true);
            }

            this.HasShown = true;
            Shown?.Invoke(this, EventArgs.Empty);

            _ = this._toolWindowService.ShowToolWindowAsync(typeof(ReadmeToolWindow), 0, true);
        }
    }
}