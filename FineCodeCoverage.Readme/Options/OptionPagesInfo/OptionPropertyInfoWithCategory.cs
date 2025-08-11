namespace FineCodeCoverage.Readme.Options.OptionPagesInfo
{
    internal sealed class OptionPropertyInfoWithCategory : OptionPropertyInfo
    {
        public OptionPropertyInfoWithCategory(
            string displayName,
            string description,
            string category,
            string name,
            bool isCoverageSetting)
            : base(displayName, description, name, isCoverageSetting) => Category = category;

        public string Category { get; }
    }
}
