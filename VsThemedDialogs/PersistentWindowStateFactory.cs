using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Settings;

namespace VsThemedDialogs
{
    [Export(typeof(IPersistentWindowStateFactory))]
    public class PersistentWindowStateFactory : IPersistentWindowStateFactory
    {
        public IPersistWindowState Create(
            string collectionName,
            WritableSettingsStore writableSettingsStore,
            bool persistPosition)
            => new PersistWindowState(collectionName, writableSettingsStore, persistPosition);
    }
}
