using System;

namespace GithubReadmeCreator
{
    internal class StringReplacer : IStringReplacer
    {
        public string Replace(string originalReadme, string marker, string replacement)
        {
            var markerStartIndex = originalReadme.IndexOf(marker);
            if (markerStartIndex == -1)
            {
                throw new ArgumentException($"Marker {marker} not found in readme");
            }
            var replaced = originalReadme.Substring(0, markerStartIndex) + replacement + originalReadme.Substring(markerStartIndex + marker.Length);
            return replaced;
        }
    }
}
