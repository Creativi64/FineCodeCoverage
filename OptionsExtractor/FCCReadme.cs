using Markdig.Syntax;
using System;
using System.Collections.Generic;

namespace OptionsExtractor
{
    public static class FCCReadme
    {
        public static string ReplaceReadMeMarkerWithOptionsTable(string markedReadme, string marker)
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

            return Replace(markedReadme, marker, GenerateReadme());
        }

        private static string Replace(string originalReadme, string marker, string replacement)
        {
            var markerStartIndex = originalReadme.IndexOf(marker);
            if (markerStartIndex == -1)
            {
                throw new ArgumentException($"Marker {marker} not found in readme");
            }
            var replaced = originalReadme.Substring(0, markerStartIndex) + replacement + originalReadme.Substring(markerStartIndex + marker.Length);
            return replaced;
        }

        private static string GenerateReadme()
        {
            var markdownDocument = new MarkdownDocument();

            List<IEnumerable<string>> rows = new List<IEnumerable<string>>();
            foreach (var optionPageInfo in OptionPageInfoProvider.Provide())
            {
                foreach (var c in optionPageInfo.PropertyCategories)
                {
                    rows.Add(new string[] { $"**{optionPageInfo.PageName} - {c.Category}**" });
                    foreach (var p in c.PropertyDisplayNameDescriptions)
                    {
                        rows.Add(new string[] { p.DisplayName, p.Description });
                    }
                }
            }

            var optionsTableString = PipeTable.GetString(new PipeTableHeader[] { new PipeTableHeader("Property"), new PipeTableHeader("Description") }, rows);
            return Environment.NewLine + optionsTableString;
        }
    }
}
