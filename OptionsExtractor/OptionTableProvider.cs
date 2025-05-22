using System;
using System.Collections.Generic;
using System.Linq;

namespace OptionsExtractor
{
    internal class OptionTableProvider : IOptionTableProvider
    {
        private readonly IOptionPageInfoProvider optionPageInfoProvider;
        private readonly IPipeTable pipeTable;

        public OptionTableProvider() : this(new OptionPageInfoProvider(), new PipeTable())
        {
        }

        internal OptionTableProvider(IOptionPageInfoProvider optionPageInfoProvider, IPipeTable pipeTable)
        {
            this.optionPageInfoProvider = optionPageInfoProvider;
            this.pipeTable = pipeTable;
        }

        public string GetTableString(Type packageType, Type coverageSettingsType)
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
            var coverageSettingsPropertyNames = coverageSettingsType.GetProperties().Select(p => p.Name).ToList();
            List<IEnumerable<string>> rows = new List<IEnumerable<string>>();
            foreach (var optionPageInfo in optionPageInfoProvider.Provide(packageType))
            {
                foreach (var c in optionPageInfo.PropertyCategories)
                {
                    rows.Add(new string[] { $"**{optionPageInfo.PageName} - {c.Category}**","","" });
                    foreach (var p in c.PropertyNamesDescriptions)
                    {
                        var isCoverageProjectSetting = coverageSettingsPropertyNames.Contains(p.Name) ? "Yes" : "No";
                        rows.Add(new string[] { p.DisplayName, p.Description, isCoverageProjectSetting });
                    }
                    rows.Add(new string[] { "<br>", "" });
                }
            }

            var optionsTableString = this.pipeTable.GetString(
                new PipeTableHeader[] {
                    new PipeTableHeader("Property"),
                    new PipeTableHeader("Description"),
                    new PipeTableHeader("Is project setting", PipeTableColumnAlignment.Center)
                }, rows);
            return Environment.NewLine + optionsTableString;
        }
    }
}
