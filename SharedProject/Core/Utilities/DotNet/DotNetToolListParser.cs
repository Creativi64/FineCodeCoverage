using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IDotNetToolListParser))]
    internal class DotNetToolListParser : IDotNetToolListParser
    {
        public List<DotNetToolInfo> Parse(string output)
        {
            // note that if included Manifest this code will need to change
            string[] outputLines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<string> toolLines = outputLines.Skip(2);
            return toolLines.Select(l =>
            {
                string[] tokens = l.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
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