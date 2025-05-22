namespace OptionsExtractor
{
    internal class PropertyCategoryNamesDescription : PropertyNamesDescription { 
        public PropertyCategoryNamesDescription(string displayName, string description, string category,string name)
            : base(displayName, description, name)
        {
            Category = category;
        }
        public string Category { get; }

    }
}
