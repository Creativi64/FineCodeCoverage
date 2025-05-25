using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GithubReadmeCreator
{
    internal class PipeTable : IPipeTable
    {
        public string GetString(IEnumerable<PipeTableHeader> headers, IEnumerable<IEnumerable<string>> rows, int numHeaderHyphens = 3, bool pipesOnEnd = true)
        {
            if (numHeaderHyphens < 3)
            {
                throw new ArgumentException(nameof(numHeaderHyphens));
            }
            var hyphens = new string('-', numHeaderHyphens);
            var stringBuilder = new StringBuilder();

            var numHeaders = headers.Count();
            AddHeaders(stringBuilder, headers, hyphens, pipesOnEnd, numHeaders);
            foreach (var row in rows)
            {
                AddRow(stringBuilder, row, pipesOnEnd, numHeaders);
            }
            return stringBuilder.ToString();
        }

        private static void AddHeaders(StringBuilder sb, IEnumerable<PipeTableHeader> headers, string hyphens, bool pipesOnEnd, int numHeaders)
        {
            AddRow(sb, headers.Select(h => h.Contents), pipesOnEnd, numHeaders);
            var headerAlignments = headers.Select(h =>
            {
                var alignment = hyphens;
                switch (h.Alignment)
                {
                    case PipeTableColumnAlignment.Left:
                        alignment = $":{hyphens}";
                        break;
                    case PipeTableColumnAlignment.Center:
                        alignment = $":{hyphens}:";
                        break;
                    case PipeTableColumnAlignment.Right:
                        alignment = $"{hyphens}:";
                        break;

                }
                return alignment;
            });
            AddRow(sb, headerAlignments, pipesOnEnd, numHeaders);
        }

        private static void AddRow(StringBuilder sb, IEnumerable<string> cells, bool pipesOnEnd, int maxCells)
        {
            if (pipesOnEnd)
            {
                sb.Append("|");
            }

            var count = 0;
            foreach (var cell in cells)
            {
                if (count != 0)
                {
                    sb.Append("|");
                }
                sb.Append($" {cell} ");
                count++;
            }

            if (pipesOnEnd)
            {
                sb.Append("|");
            }
            sb.AppendLine();
        }
    }
}
