using System.Linq;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal sealed class LineExcluder : ILineExcluder
    {
        private readonly string[] _startsWithExclusions;

        public LineExcluder(string[] startsWithExclusions) => _startsWithExclusions = startsWithExclusions;

        public bool ExcludeIfNotCode(string text)
        {
            string trimmedLineText = text.Trim();
            return trimmedLineText.Length == 0 || StartsWithExclusion(trimmedLineText);
        }

        private bool StartsWithExclusion(string text)
            => _startsWithExclusions.Any(languageExclusion => text.StartsWith(languageExclusion));
    }
}
