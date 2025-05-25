namespace FineCodeCoverage.Readme
{
    public class OptionPropertyInfo
    {
        public OptionPropertyInfo(string displayName, string description, string name, bool isCoverageSetting)
        {
            DisplayName = displayName;
            Description = description;
            Name = name;
            IsCoverageSetting = isCoverageSetting;
        }
        public string DisplayName { get; }
        public string Description { get; }
        public string Name { get; }
        public bool IsCoverageSetting { get; }
    }
}
