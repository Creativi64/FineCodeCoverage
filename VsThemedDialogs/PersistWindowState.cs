using Microsoft.VisualStudio.Settings;

namespace VsThemedDialogs
{
    internal class PersistWindowState : IPersistWindowState
    {
        private readonly string _collectionName;
        private readonly WritableSettingsStore _writableSettingsStore;

        public PersistWindowState(string collectionName, WritableSettingsStore writableSettingsStore, bool persistPosition)
        {
            _collectionName = collectionName;
            _writableSettingsStore = writableSettingsStore;
            PersistPosition = persistPosition;
        }

        public bool PersistPosition { get; }

        public WindowPersistence GetState()
        {
            if (!_writableSettingsStore.CollectionExists(_collectionName))
            {
                return null;
            }

            double left = _writableSettingsStore.GetInt32(_collectionName, "Left");
            double top = _writableSettingsStore.GetInt32(_collectionName, "Top");
            double width = _writableSettingsStore.GetInt32(_collectionName, "Width");
            double height = _writableSettingsStore.GetInt32(_collectionName, "Height");
            return new WindowPersistence
            {
                Left = left,
                Top = top,
                Width = width,
                Height = height
            };

        }

        public void SetState(WindowPersistence persistence)
        {
            _writableSettingsStore.CreateCollection(_collectionName);
            _writableSettingsStore.SetInt32(_collectionName, "Left", (int)persistence.Left);
            _writableSettingsStore.SetInt32(_collectionName, "Top", (int)persistence.Top);
            _writableSettingsStore.SetInt32(_collectionName, "Width", (int)persistence.Width);
            _writableSettingsStore.SetInt32(_collectionName, "Height", (int)persistence.Height);
        }
    }
}
