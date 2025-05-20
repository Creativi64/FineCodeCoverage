namespace OptionsExtractor
{
    internal class PropertyCategoryDisplayNameDescription : PropertyDisplayNameDescription { 
        public PropertyCategoryDisplayNameDescription(string displayName, string description, string category)
            : base(displayName, description)
        {
            Category = category;
        }
        public string Category { get; }

    }
}
