using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Output;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Settings;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IReadMeService))]
    internal class ReadMeService : IReadMeService
    {
        private readonly WritableSettingsStore writableUserSettingsStore;
        private readonly IToolWindowService toolWindowService;
        private const string readMeShowCollection = "FCCReadmeShowCollection";
        private const string readMeShownProperty = "FCCReadmeShown";

        public event EventHandler ReadMeShown;

        [ImportingConstructor]
        public ReadMeService(
            IToolWindowService toolWindowService,
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider
        )
        {
            this.writableUserSettingsStore = writableUserSettingsStoreProvider.LazySettingsStore.GetValue();
            this.HasShownReadMe = this.writableUserSettingsStore.GetBoolean(readMeShowCollection, readMeShownProperty, false);
            this.toolWindowService = toolWindowService;
        }

        public bool HasShownReadMe { get; private set; }

        public void ShowReadMe()
        {
            if (!this.HasShownReadMe)
            {
                if(!this.writableUserSettingsStore.CollectionExists(readMeShowCollection))
                {
                    this.writableUserSettingsStore.CreateCollection(readMeShowCollection);
                }
                this.writableUserSettingsStore.SetBoolean(readMeShowCollection, readMeShownProperty, true);
            }
            this.HasShownReadMe = true;
            ReadMeShown?.Invoke(this, EventArgs.Empty);

            _ = this.toolWindowService.ShowToolWindowAsync(typeof(ReadmeToolWindow), 0, true);
        }
    }
}
