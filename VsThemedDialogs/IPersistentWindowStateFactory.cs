using Microsoft.VisualStudio.Settings;

namespace VsThemedDialogs
{
    public interface IPersistentWindowStateFactory
    {
        IPersistWindowState Create(string collectionName, WritableSettingsStore writableSettingsStore, bool persistPosition);
    }
}
