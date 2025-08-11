namespace FineCodeCoverage.Readme.Options.OptionsTable
{
    public static class OptionPageTableDisplayInfo
    {
        public const string OptionHeader = "Option";
        public const OptionPageTableCellAlignment OptionCellAlignment = OptionPageTableCellAlignment.Left;
        public const string DescriptionHeader = "Description";
        public const OptionPageTableCellAlignment DescriptionCellAlignment = OptionPageTableCellAlignment.Left;
        public const string IsCoverageSettingHeader = "Is Coverage Setting";
        public const OptionPageTableCellAlignment IsCoverageSettingCellAlignment = OptionPageTableCellAlignment.Center;
        public const string IsCoverageSettingYes = "Yes";
        public const string IsCoverageSettingNo = "";

        public static string PageNameCategoryDisplay(string pageName, string category) => $"{pageName} - {category}";
    }
}
