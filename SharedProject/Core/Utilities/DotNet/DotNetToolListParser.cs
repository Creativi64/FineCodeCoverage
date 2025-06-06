using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IDotNetToolListParser))]
    internal sealed class DotNetToolListParser : IDotNetToolListParser
    {
        private static readonly char[] s_newLineSeparators = new[] { '\r', '\n' };
        private static readonly char[] s_spacingSeparators = new[] { ' ', '\t' };

        public List<DotNetToolInfo> Parse(string output)
        {
            // note that if included Manifest this code will need to change
            string[] outputLines = output.Split(s_newLineSeparators, StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<string> toolLines = outputLines.Skip(2);
            return toolLines.Select(l =>
            {
                string[] tokens = l.Split(s_spacingSeparators, StringSplitOptions.RemoveEmptyEntries);
                return new DotNetToolInfo
                {
                    PackageId = tokens[0].Trim(),
                    Version = tokens[1].Trim(),
                    Commands = tokens[2].Trim(),
                };
            }).ToList();
        }
    }
}
