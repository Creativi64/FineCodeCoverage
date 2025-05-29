using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Output
{
    internal class Changeset : IChangeset
    {
        private readonly IDictionary<string, HashSet<int>> changeLookup;

        public Changeset(IDictionary<string, HashSet<int>> changeLookup) => this.changeLookup = changeLookup;

        public List<int> GetLineNumbers(string filePath)
            => this.changeLookup.TryGetValue(filePath, out HashSet<int> lineNumbers)
                ? lineNumbers.ToList()
                : Enumerable.Empty<int>().ToList();
    }
}
