using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Output
{
    internal class Changeset : IChangeset
    {
        private readonly IDictionary<string, HashSet<int>> changeLookup;

        public Changeset(IDictionary<string, HashSet<int>> changeLookup)
        {
            this.changeLookup = changeLookup;
        }

        public List<int> GetLineNumbers(string filePath)
        {
            if (changeLookup.TryGetValue(filePath, out var lineNumbers))
            {
                return lineNumbers.ToList();
            }
            return Enumerable.Empty<int>().ToList();
        }
    }
}
