using FineCodeCoverage.Readme;
using System;
using System.Collections.Generic;

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
                    rows.Add(new string[] { $"**{optionPageInfo.PageName} - {c.Category}**", "", "" });
                    foreach (var p in c.OptionPropertyInfos)
                    {
                        var isCoverageProjectSetting = p.IsCoverageSetting ? "Yes" : "No";
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
