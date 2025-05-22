namespace OptionsExtractor
{
    public class PropertyNamesDescription
    {
        public PropertyNamesDescription(string displayName, string description, string name)
        {
            DisplayName = displayName;
            Description = description;
            Name = name;
        }
        public string DisplayName { get; }
        public string Description { get;}
        public string Name { get;}
    }
}
