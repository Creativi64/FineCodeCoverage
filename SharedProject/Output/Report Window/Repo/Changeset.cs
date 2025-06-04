using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Output
{
    internal class Changeset : IChangeset
    {
        private readonly IDictionary<string, HashSet<int>> _changeLookup;

        public Changeset(IDictionary<string, HashSet<int>> changeLookup) => this._changeLookup = changeLookup;

        public List<int> GetLineNumbers(string filePath)
            => this._changeLookup.TryGetValue(filePath, out HashSet<int> lineNumbers)
                ? lineNumbers.ToList()
                : Enumerable.Empty<int>().ToList();
    }
}