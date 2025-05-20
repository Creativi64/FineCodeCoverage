namespace OptionsExtractor
{
    public class PropertyDisplayNameDescription
    {
        public PropertyDisplayNameDescription(string displayName, string description)
        {
            DisplayName = displayName;
            Description = description;
        }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
}
