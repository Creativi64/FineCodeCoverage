using System.Collections.Generic;

namespace OptionsExtractor
{
    internal interface IPipeTable
    {
        string GetString(
            IEnumerable<PipeTableHeader> headers,
            IEnumerable<IEnumerable<string>> rows,
            int numHeaderHyphens = 3,
            bool pipesOnEnd = true);
    }
}
