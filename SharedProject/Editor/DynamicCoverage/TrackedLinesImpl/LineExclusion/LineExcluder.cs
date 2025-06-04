using System.Linq;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class LineExcluder : ILineExcluder
    {
        private readonly string[] _startsWithExclusions;

        public LineExcluder(string[] startsWithExclusions) => this._startsWithExclusions = startsWithExclusions;

        public bool ExcludeIfNotCode(string text)
        {
            string trimmedLineText = text.Trim();
            return trimmedLineText.Length == 0 || this.StartsWithExclusion(trimmedLineText);
        }

        private bool StartsWithExclusion(string text)
            => this._startsWithExclusions.Any(languageExclusion => text.StartsWith(languageExclusion));
    }
}