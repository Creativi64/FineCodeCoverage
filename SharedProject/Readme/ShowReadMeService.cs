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
        private readonly WritableSettingsStore writableUserSettingsStore;
        private readonly IToolWindowService toolWindowService;
        private const string readMeShowCollection = "FCCReadmeShowCollection";
        private const string readMeShownProperty = "FCCReadmeShown";

        public event EventHandler Shown;

        [ImportingConstructor]
        public ShowReadMeService(
            IToolWindowService toolWindowService,
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider
        )
        {
            this.writableUserSettingsStore = writableUserSettingsStoreProvider.LazySettingsStore.GetValue();
            this.HasShown = this.writableUserSettingsStore.GetBoolean(readMeShowCollection, readMeShownProperty, false);
            this.toolWindowService = toolWindowService;
        }

        public bool HasShown { get; private set; }

        public void Show()
        {
            if (!this.HasShown)
            {
                if (!this.writableUserSettingsStore.CollectionExists(readMeShowCollection))
                {
                    this.writableUserSettingsStore.CreateCollection(readMeShowCollection);
                }
                this.writableUserSettingsStore.SetBoolean(readMeShowCollection, readMeShownProperty, true);
            }
            this.HasShown = true;
            Shown?.Invoke(this, EventArgs.Empty);

            _ = this.toolWindowService.ShowToolWindowAsync(typeof(ReadmeToolWindow), 0, true);
        }
    }
}
