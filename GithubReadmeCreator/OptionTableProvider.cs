using System;
using System.Collections.Generic;
using FineCodeCoverage.Readme.Options.OptionPagesInfo;
using FineCodeCoverage.Readme.Options.OptionsTable;

namespace GithubReadmeCreator
{
    internal class OptionTableProvider : IOptionTableProvider
    {
        private readonly IFCCOptionPageInfoProvider optionPageInfoProvider;
        private readonly IPipeTable pipeTable;

        public OptionTableProvider() : this(new FCCOptionPageInfoProvider(), new PipeTable())
        {
        }

        internal OptionTableProvider(IFCCOptionPageInfoProvider optionPageInfoProvider, IPipeTable pipeTable)
        {
            this.optionPageInfoProvider = optionPageInfoProvider;
            this.pipeTable = pipeTable;
        }

        public string GetTableString()
        {
            /*
                was going to use MarkDig ast to replace but
                https://github.com/xoofx/markdig/issues/744 - PipeTable invalid rendering

                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

                var markdownDocument = Markdown.Parse(mdString, pipeline);
                var markerBlock = markdownDocument.FindBlockAtPosition(markerStartIndex);

                var replacement = Markdown.Parse(GenerateReadme(), pipeline);
                var table = replacement.Descendants<Table>().First();

                var markerBlockParent = markerBlock.Parent;
                var markerBlockIndex = markerBlockParent.IndexOf(markerBlock);
                table.Parent.Remove(table);
                markerBlockParent.Insert(markerBlockIndex, table);
                markerBlockParent.Remove(markerBlock);

            */
            List<IEnumerable<string>> rows = new List<IEnumerable<string>>();
            foreach (var optionPageInfo in optionPageInfoProvider.Provide())
            {
                foreach (var c in optionPageInfo.PropertyCategories)
                {
                    rows.Add(new string[] { $"**{OptionPageTableDisplayInfo.PageNameCategoryDisplay(optionPageInfo.PageName,c.Category)}**", "", "" });
                    foreach (var p in c.OptionPropertyInfos)
                    {
                        var isCoverageProjectSetting = p.IsCoverageSetting ?
                            OptionPageTableDisplayInfo.IsCoverageSettingYes : OptionPageTableDisplayInfo.IsCoverageSettingNo;
                        rows.Add(new string[] { p.DisplayName, p.Description, isCoverageProjectSetting });
                    }
                    rows.Add(new string[] { "<br>", "" });
                }
            }

            var optionsTableString = this.pipeTable.GetString(
                new PipeTableHeader[] {
                    new PipeTableHeader(
                        OptionPageTableDisplayInfo.OptionHeader,
                        GetColumnAlignment(OptionPageTableDisplayInfo.OptionCellAlignment)),
                    new PipeTableHeader(OptionPageTableDisplayInfo.DescriptionHeader,
                         GetColumnAlignment(OptionPageTableDisplayInfo.DescriptionCellAlignment)
                    ),
                    new PipeTableHeader(
                        OptionPageTableDisplayInfo.IsCoverageSettingHeader,
                        GetColumnAlignment(OptionPageTableDisplayInfo.IsCoverageSettingCellAlignment)
                    )
                }, rows);
            return Environment.NewLine + optionsTableString;
        }

        private PipeTableColumnAlignment GetColumnAlignment(OptionPageTableCellAlignment optionPageTableColumnAlignment)
        {
            switch (optionPageTableColumnAlignment)
            {
                case OptionPageTableCellAlignment.Center:
                    return PipeTableColumnAlignment.Center;
                case OptionPageTableCellAlignment.Right:
                    return PipeTableColumnAlignment.Right;
            }
            return PipeTableColumnAlignment.Left;
        }
    }
}
