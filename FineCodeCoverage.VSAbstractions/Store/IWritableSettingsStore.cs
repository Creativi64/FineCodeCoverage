namespace FineCodeCoverage.Options
{
    public interface IWritableSettingsStore
    {
        bool CollectionExists(string collectionPath);

        void CreateCollection(string collectionPath);

        bool DeleteCollection(string collectionPath);

        bool GetBoolean(string collectionPath, string propertyName, bool defaultValue);
 
        string GetString(string collectionPath, string propertyName);

        bool PropertyExists(string collectionPath, string propertyName);

        void SetBoolean(string collectionPath, string propertyName, bool value);

        void SetString(string collectionName, string propertyName, string value);
    }
}
