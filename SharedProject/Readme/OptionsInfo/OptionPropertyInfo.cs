namespace FineCodeCoverage.Readme
{
    internal class OptionPropertyInfo
    {
        public OptionPropertyInfo(string displayName, string description, string name, bool isCoverageSetting)
        {
            this.DisplayName = displayName;
            this.Description = description;
            this.Name = name;
            this.IsCoverageSetting = isCoverageSetting;
        }

        public string DisplayName { get; }

        public string Description { get; }

        public string Name { get; }

        public bool IsCoverageSetting { get; }
    }
}