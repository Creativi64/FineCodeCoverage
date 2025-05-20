using Markdig.Syntax;
using System;
using System.Collections.Generic;

namespace OptionsExtractor
{
    public static class FCCReadme
    {
        public static string ReplaceReadMeMarkerWithOptionsTable(string markedReadme, string marker)
        {
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
