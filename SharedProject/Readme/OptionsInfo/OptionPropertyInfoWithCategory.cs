namespace FineCodeCoverage.Readme
{
    internal class OptionPropertyInfoWithCategory : OptionPropertyInfo
    {
        public OptionPropertyInfoWithCategory(
            string displayName,
            string description,
            string category,
            string name,
            bool isCoverageSetting)
            : base(displayName, description, name, isCoverageSetting) => this.Category = category;

        public string Category { get; }
    }
}
